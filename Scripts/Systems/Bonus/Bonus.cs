using System.Linq;
using UnityEngine;
using Zenject;

public abstract class Bonus : MonoBehaviour
{
    [Inject] protected SignalBus _signalBus;
    [Inject] protected LevelConfig _levelConfig;
    [SerializeField] private int _bonusId;
    [SerializeField] protected GameObject _effect;
    public BonusType Type { get; protected set; }
    public int BonusId => _bonusId;   
    
    protected bool _canActivate;
    
    protected virtual void Start()
    {
        _signalBus.Subscribe<StartMissionSignal>(_=> _canActivate = true);
        CheckBonusState();
    }

    private void OnMouseDown()
    {
        if(_canActivate)
            Activate();
    }

    protected abstract void Activate();
    
    private void CheckBonusState()
    {
        var levelData = GamePersistence.SaveData.Levels.FirstOrDefault(l => l.LevelId == _levelConfig.LevelId);
        
        if (levelData != null && levelData.IsCompleted && levelData.CollectedBonusIds.Contains(_bonusId))
        {
            _canActivate = false;
            gameObject.SetActive(false);
        }
    }
}