using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LevelSelectionView : MonoBehaviour, IView
{
   [SerializeField] private GameObject _panel;
   [SerializeField] private Button _backButton;

    private LevelSelectionPresenter _presenter;
    public void SetPresenter(LevelSelectionPresenter presenter)
    {
        _presenter = presenter;
    }
    void Start()
    {
        _presenter.Initialize();
        Hide();
        _backButton.OnClickAsObservable()
            .Subscribe(_ => Hide())
            .AddTo(this);
    }

    public void Show()
    {
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    void OnDestroy()
    {
        _presenter.Dispose();
    }
}