using Unity.Mathematics;
using UnityEngine;
using Zenject;

public class ProjectileFactory
{
    private readonly DiContainer _container;

    public ProjectileFactory(DiContainer container)
    {
        _container = container;
    }

    public Projectile Create(ProjectileType type, Vector3 direction, int damage)
    {
        var prefab = Resources.Load<GameObject>(GetPrefabPath(type));
        var instance = _container.InstantiatePrefab(prefab);
        var projectile = instance.GetComponent<Projectile>();
        projectile.Initialize(direction, damage);
        projectile.transform.rotation = Quaternion.LookRotation(direction);
        return projectile;
    }

    private string GetPrefabPath(ProjectileType type) => $"Projectiles/{type}Bullet";
}