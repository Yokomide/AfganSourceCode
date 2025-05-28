using System;
using UniRx;
using UnityEngine;

[System.Serializable]
public class UnitModel
{
    public string Name { get; }
    public int Id { get; }
    public int Crew { get; }
    public ReactiveProperty<int> Health { get; } = new();
    public int MaxHealth { get; private set; }
    public ReactiveProperty<float> Durability { get; } = new(1f);
    public float Speed { get; }
    public int Damage { get; }
    
    public float FireRate { get; } 
    
    public UnitType Type { get; }
    public UnitClass Class { get; }
    
    
    public UnitModel(string name, int id,int crew, int maxHealth, float durability, float speed, int damage, UnitType type, UnitClass unitClass, float fireRate = 1f)
    {
        Name = name;
        Id = id;
        Crew = crew;
        MaxHealth = maxHealth;
        Durability.Value = durability;
        Speed = speed;
        Damage = damage;
        Type = type;
        Class = unitClass;
        FireRate = fireRate;

        UpdateHealth();
    }

    public void UpdateHealth()
    {
        Health.Value = (int) (MaxHealth * Durability.Value);

    }
    public void TakeDamage(int damage)
    {
        Health.Value = Mathf.Max(0, Health.Value - damage);
        Durability.Value = Health.Value > 0 ? Health.Value/ (float)MaxHealth  : 0f;
    }
}
