using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using System;
using Zenject;

[RequireComponent(typeof(Button))]
public class SceneSwitchButton : MonoBehaviour
{
    [SerializeField] private SceneTransitionOptions _transitionOptions = new SceneTransitionOptions();
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [Inject] private UnitPark _unitPark;
    private Button _button;
    private CompositeDisposable _disposables = new CompositeDisposable();

    [Serializable]
    private class SceneTransitionOptions
    {
        public string SceneName = "";
        public int SceneIndex = -1;
        public bool UseSceneIndex = false;
        public bool ReloadCurrentScene = false;
        public float FadeDuration = 0.5f;
        public Ease FadeEase = Ease.InOutQuad;

        public bool HasValidTarget => ReloadCurrentScene || 
                                     (UseSceneIndex && SceneIndex >= 0) || 
                                     !string.IsNullOrEmpty(SceneName);
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.OnClickAsObservable()
            .Subscribe(_ => StartTransition())
            .AddTo(_disposables);
    }

    public void SetLevelSettings(LevelConfig levelData)
    {
        _transitionOptions.SceneName = $"Level {levelData.LevelId}";
    }
    
    private void OnDestroy()
    {
        _disposables.Dispose(); // Очищаем подписки
    }

    private void StartTransition()
    {
        if (!_transitionOptions.HasValidTarget)
        {
            Debug.LogWarning("Нет цели для перехода", this);
            return;
        }

        // Цепочка перехода
        FadeTransition()
            .DoOnCompleted(LoadTargetScene)
            .Subscribe()
            .AddTo(_disposables);
    }

    private IObservable<Unit> FadeTransition()
    {
        if (_fadeCanvasGroup != null)
        {
            _fadeCanvasGroup.alpha = 0f; // Начальное значение
            return Observable.Create<Unit>(observer =>
            {
                _fadeCanvasGroup.DOFade(1f, _transitionOptions.FadeDuration)
                    .SetEase(_transitionOptions.FadeEase)
                    .OnComplete(() =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    });

                return Disposable.Create(() => DOTween.Kill(_fadeCanvasGroup));
            });
        }
        else
        {
            return Observable.Timer(TimeSpan.FromSeconds(_transitionOptions.FadeDuration))
                .Select(_ => Unit.Default);
        }
    }

    private void LoadTargetScene()
    {
        //_unitPark.ReloadUnits();
        if (_transitionOptions.ReloadCurrentScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (_transitionOptions.UseSceneIndex)
        {
            SceneManager.LoadScene(_transitionOptions.SceneIndex);
        }
        else
        {
            SceneManager.LoadScene(_transitionOptions.SceneName);
        }
    }

    // Публичный метод для внешнего вызова
    public IObservable<Unit> TriggerTransition()
    {
        return Observable.Create<Unit>(observer =>
        {
            StartTransition();
            observer.OnNext(Unit.Default);
            observer.OnCompleted();
            return Disposable.Empty;
        });
    }
}