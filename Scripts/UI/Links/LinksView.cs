using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LinksView : MonoBehaviour, IView
{
    [Serializable]
    public class LinkButton
    {
        public Button button;
        public string link;
    }
    
    [SerializeField] private GameObject _panel;
    [SerializeField] private LinksPresenter _presenter;
    [SerializeField] private Button _backButton;
    [SerializeField] private List<LinkButton> _linkButtons = new List<LinkButton>();
    
    public void SetPresenter(LinksPresenter presenter)
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
        
        for (int i = 0; i < _linkButtons.Count; i++)
        {
            int index = i;
            _linkButtons[index].button.onClick.AddListener(() => OpenLink(_linkButtons[index].link));
        }
    }

    private void OpenLink(string url)
    {
        Application.OpenURL(url);
    }
    public void Show()
    {
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);

    }
}
