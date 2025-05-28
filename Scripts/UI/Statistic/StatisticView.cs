using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class StatisticView : MonoBehaviour, IView
{
    [SerializeField] private Button _backButton;
    [SerializeField] private TextMeshProUGUI _statisticsText;

    private StatisticPresenter _statisticPresenter;

    public void SetPresenter(StatisticPresenter presenter)
    {
        _statisticPresenter = presenter;
    }

    private void Start()
    {
        Hide();
        _backButton.OnClickAsObservable()
            .Subscribe(_ => Hide())
            .AddTo(this);
    }

    public void UpdateStatisticsText(float distance, int saved, int livesLost, int unitsLost, int enemiesDestroyed, int enemyVehiclesDestroyed)
    {
        _statisticsText.text =
            $"Уничтожено врагов: <pos=90%>{enemiesDestroyed}" +
            $"\nУничтожено вражеской техники: <pos=90%>{enemyVehiclesDestroyed}" +
            $"\nПотеряно союзников: <pos=90%>{unitsLost}" +
            $"\nПотеряно техники: <pos=90%>{livesLost}" +
            $"\nПройдено пути(км): <pos=90%>{distance:F1}" +
            $"\nСпасено людей: <pos=90%>{saved}";

    }

    public void Show()
    {
        gameObject.SetActive(true);
        _statisticPresenter.ShowStatistic();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}