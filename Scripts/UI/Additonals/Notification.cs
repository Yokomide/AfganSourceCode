using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    [SerializeField] private GameObject _notificationPanel;
    [SerializeField] private TextMeshProUGUI _textHolder;
    [SerializeField] private Image _border;
    [SerializeField] private Color _errorColor;
    [SerializeField] private Color _notifyColor;

    private IDisposable _currentTimerSubscription;

    public void ShowNotification(NotificationType type, string info)
    {
        _currentTimerSubscription?.Dispose();

        if (type == NotificationType.Notify)
            _border.color = _notifyColor;
        else
            _border.color = _errorColor;

        SetInfo(info);

        _notificationPanel.SetActive(true);
        _currentTimerSubscription = Observable.Timer(TimeSpan.FromSeconds(3))
            .Subscribe(_ => _notificationPanel.gameObject.SetActive(false))
            .AddTo(this);
    }

    private void SetInfo(string info)
    {
        _textHolder.text = info;
    }

    public enum NotificationType
    {
        Notify,
        Error
    }
}