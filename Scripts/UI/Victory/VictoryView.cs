using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryView : MonoBehaviour, IView
{
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private Button _menuButton;
    [SerializeField] private TextMeshProUGUI _distanceValueInfo;
    [SerializeField] private TextMeshProUGUI _saved;
    [SerializeField] private TextMeshProUGUI _allyLost;
    [SerializeField] private TextMeshProUGUI _unitsLost;
    [SerializeField] private TextMeshProUGUI _enemiesDestroyed;

    [SerializeField] private MissionInfoHandler _missionInfoHandler;


    public void Show()
    {
        _victoryPanel.SetActive(true);
        _missionInfoHandler.SetSuccess(true);
    }

    public void SetInfo(float distanceValue, int saved, int allyLost, int unitsLost, int enemiesDestroyed)
    {
        _distanceValueInfo.text = $"{distanceValue:F2} км";
        _saved.text = saved.ToString();
        _allyLost.text = allyLost.ToString();
        _unitsLost.text = unitsLost.ToString();
        _enemiesDestroyed.text = enemiesDestroyed.ToString();

    }
    public void Hide()
    {
        _victoryPanel.SetActive(false);
    }

    public void SetDefeatDistanceMessage(string message)
    {
        //_totalDistance.text = message;
    }
    
}