using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ScrollResetter : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private bool _resetVertical = true; 
    [SerializeField] private bool _resetHorizontal = false;

    private void OnEnable()
    {
        Observable.NextFrame().Subscribe(_ => ResetScrollPosition()).AddTo(this);
    }

    public void ResetScrollPosition()
    {
        if (_scrollRect == null)
        {
            Debug.LogWarning("ScrollRect не привязан к ScrollResetter", this);
            return;
        }

        if (_resetVertical)
        {
            _scrollRect.verticalNormalizedPosition = 1.0f; 
        }

        if (_resetHorizontal)
        {
            _scrollRect.horizontalNormalizedPosition = 0.0f; 
        }
    }
}