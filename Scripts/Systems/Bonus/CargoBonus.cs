using UnityEngine;
using Zenject;

public class CargoBonus : Bonus
{
    protected override void Start()
    {
        base.Start();
        Type = BonusType.Cargo;
    }

    protected override void Activate()
    {
        _signalBus.Fire(new BonusActivatedSignal(this));
        _canActivate = false;
        _effect.SetActive(false);

        Destroy(gameObject);
    }
}