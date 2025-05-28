using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class HitIndicator : MonoBehaviour
{
    [SerializeField] private Image _hitIcon; 
    [SerializeField] private float _scaleUpDuration = 0.1f; 
    [SerializeField] private float _scaleDownDuration = 0.2f; 
    [SerializeField] private float _maxScale = 1.5f;
    [SerializeField] private AudioSource _audioSource;
    private RectTransform _rectTransform;
    [Inject] private SignalBus _signalBus;
    private Sequence _hitSequence;
    private void Awake()
    {
        _rectTransform = _hitIcon.GetComponent<RectTransform>();
        _hitIcon.transform.localScale = Vector3.zero;
        _signalBus.Subscribe<HitSignal>(OnHit);
        _hitSequence = DOTween.Sequence();
    }
    private void OnHit(HitSignal signal)
    {

        ShowHit(signal.ScreenPosition);
    }
    public void ShowHit(Vector2 screenPosition)
    {
        _audioSource.Play();
        transform.position = new Vector2(Input.mousePosition.x-120, Input.mousePosition.y + 120f);
        _hitSequence = DOTween.Sequence();

        _hitSequence
                    .Append(_hitIcon.transform.DOScale(_maxScale, _scaleUpDuration))
                    .Join(_hitIcon.DOFade(1, _scaleDownDuration))
                    .Append(_hitIcon.transform.DOScale(0f, _scaleDownDuration)) 
                    .Append(_hitIcon.DOFade(0, 0.2f)).SetAutoKill(true); 
    }
    
    private void OnDestroy()
    {
        _signalBus.Unsubscribe<HitSignal>(OnHit); 
        _hitIcon.transform.DOKill(); 
    }
}