using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Zenject;
using UniRx;
using System.Linq;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ConvoyCameraController : MonoBehaviour
{
    [Inject] private ConvoySystem _convoySystem;
    [Inject] private ConvoyPlacer _convoyPlace;
    [Inject] private SignalBus _signalBus;

    [SerializeField] private float _offsetValue;
    
    private CinemachineVirtualCamera _virtualCamera;
    private CinemachineTransposer _virtualCameraTransposer;
    private GameObject _cameraTarget;
    private readonly CompositeDisposable _disposables = new();

    private void Awake()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _virtualCameraTransposer =   _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        
        _cameraTarget = new GameObject("ConvoyCameraTarget");
        if (_virtualCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera component not found on this GameObject!");
            return;
        }

        _virtualCamera.Follow = _cameraTarget.transform;
        _virtualCamera.LookAt = _cameraTarget.transform;
        if (_convoyPlace == null)
        {
            Debug.LogError("ConvoyPlace not found in scene!");
            return;
        }
        _signalBus.Subscribe<StartMissionSignal>(SetConvoyFollowOffset);
    }

    private void Start()
    {
        if (_convoySystem == null)
        {
            Debug.LogError("ConvoySystem not injected into ConvoyCameraController!");
            return;
        }

        // Запускаем корутину для установки начальной позиции после первого кадра
        StartCoroutine(InitializeCameraPosition());

        Observable.EveryUpdate()
            .Subscribe(_ => UpdateCameraPosition())
            .AddTo(_disposables);
    }

    private IEnumerator InitializeCameraPosition()
    {
        // Ждём один кадр, чтобы Cinemachine полностью инициализировался
        yield return null;

        Vector3 initialPosition = CalculateInitialPosition();
        _cameraTarget.transform.position = initialPosition;

        var transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            Vector3 offset = transposer.m_FollowOffset;
            Vector3 desiredCameraPosition = initialPosition + offset;
            _virtualCamera.ForceCameraPosition(desiredCameraPosition, Quaternion.LookRotation(initialPosition - desiredCameraPosition));
        }
    }

    private Vector3 CalculateInitialPosition()
    {
        var activeUnits = _convoySystem?.Convoy.Where(u => u != null).ToList() ?? new List<UnitController>();

        if (activeUnits.Count == 0)
        {
            return GetSlotsCenter();
        }
        else
        {
            return CalculateConvoyCenter(activeUnits);
        }
    }

    private void UpdateCameraPosition()
    {
        var activeUnits = _convoySystem.Convoy.Where(u => u != null).ToList();

        if (activeUnits.Count == 0)
        {
            Vector3 slotsCenter = GetSlotsCenter();
            _cameraTarget.transform.position = slotsCenter;
        }
        else
        {
            Vector3 centerPos = CalculateConvoyCenter(activeUnits);
            _cameraTarget.transform.position = centerPos;
        }
    }

    private Vector3 CalculateConvoyCenter(List<UnitController> activeUnits)
    {
        if (activeUnits.Count == 1)
        {
            return activeUnits[0].transform.position;
        }

        Vector3 minPos = activeUnits[0].transform.position;
        Vector3 maxPos = activeUnits[0].transform.position;

        foreach (var unit in activeUnits)
        {
            Vector3 pos = unit.transform.position;
            minPos.x = Mathf.Min(minPos.x, pos.x);
            minPos.y = Mathf.Min(minPos.y, pos.y);
            minPos.z = Mathf.Min(minPos.z, pos.z);
            maxPos.x = Mathf.Max(maxPos.x, pos.x);
            maxPos.y = Mathf.Max(maxPos.y, pos.y);
            maxPos.z = Mathf.Max(maxPos.z, pos.z);
        }

        return (minPos + maxPos) / 2f;
    }

    private void SetConvoyFollowOffset()
    {
        if (_virtualCameraTransposer == null)
        {
            Debug.LogError("CinemachineTransposer not found!");
            return;
        }

        Debug.Log("VirtualCamera Transposer: " + _virtualCameraTransposer);

        // Целевое значение FollowOffset
        Vector3 targetOffset = new Vector3(
            _virtualCameraTransposer.m_FollowOffset.x,
            _virtualCameraTransposer.m_FollowOffset.y,
            _offsetValue
        );

        // Плавное изменение m_FollowOffset с помощью DOTween
        DOTween.To(
            () => _virtualCameraTransposer.m_FollowOffset,          // Текущее значение
            value => _virtualCameraTransposer.m_FollowOffset = value, // Установка нового значения
            targetOffset,                                           // Целевое значение
            1f                                                     // Длительность анимации (в секундах)
        ).SetEase(Ease.OutQuad); // Тип сглаживания (можно изменить, например, на Ease.InOutSine)
    }
    private Vector3 GetSlotsCenter()
    {
        if (_convoyPlace.Slots.Count == 0)
        {
            Debug.LogWarning("No slots found in ConvoyPlace!");
            return Vector3.zero;
        }

        Vector3 firstSlotPos = _convoyPlace.Slots[0].transform.position;
        Vector3 lastSlotPos = _convoyPlace.Slots[_convoyPlace.Slots.Count - 1].transform.position;
        return (firstSlotPos + lastSlotPos) / 2f;
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _signalBus.Unsubscribe<StartMissionSignal>(SetConvoyFollowOffset);

        if (_cameraTarget != null) Object.Destroy(_cameraTarget);
    }
}