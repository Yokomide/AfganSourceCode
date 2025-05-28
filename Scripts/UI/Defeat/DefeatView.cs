using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DefeatView : MonoBehaviour, IView
{
    [SerializeField] private GameObject _defeatPanel;
    [SerializeField] private Button _restartButton; 
    [SerializeField] private TextMeshProUGUI _defeatReasonDistance;
    public Button RestartButton => _restartButton;

    [Inject] private UnitPark _unitPark;
    [SerializeField] private MissionInfoHandler _missionInfoHandler;

    public void Show()
    {
        _defeatPanel.SetActive(true);
        _missionInfoHandler.SetSuccess(false);
        for (int i = 0; i < _unitPark.AvailableUnits.Count; i++)
        {
            Debug.Log("AvaiableUnitsInDefeat: " + _unitPark.AvailableUnits[i].Name);
        }

        for (int i = 0; i < _unitPark.AvailableUnits.Count; i++)
        {
            Debug.Log("UnitInfoDurability: " + _unitPark.AvailableUnits[i].Durability);
        }
    }

    public void Hide()
    {
        _defeatPanel.SetActive(false);
    }

    public void SetDefeatReasonMessage(string message)
    {
        _defeatReasonDistance.text = message;
    }
    
}