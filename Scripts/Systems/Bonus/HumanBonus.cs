using UnityEngine;
using Zenject;

public class HumanBonus : Bonus
{
    protected override void Start()
    {
        base.Start();
        Type = BonusType.Human;
    }

    protected override void Activate()
    {
        _signalBus.Fire(new BonusActivatedSignal(this));
        _canActivate = false;
        _effect.SetActive(false);

        Destroy(gameObject);
    }
}