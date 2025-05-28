using System;
using UniRx;
using UnityEngine;
using Zenject;

public class Projectile : MonoBehaviour
{
    public int Damage { get; private set; }
    private float _speed = 35f;

    [SerializeField]
    private ParticleSystem _explosionEffect;
    
    [SerializeField]
    private float _destructionRadius;
    
    [SerializeField]
    private LayerMask _enemyLayer;

    [SerializeField] 
    private bool _showHitIndicator;
    
    [Inject] private SignalBus _signalBus;
    
    public void Initialize(Vector3 direction, int damage)
    {
        Damage = damage;
        GetComponent<Rigidbody>().velocity = direction * _speed;
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter(Collision other)
    {
        Explode();
        Destroy(gameObject);
    }
    
    void Explode()
    {
        if (_explosionEffect != null)
        {
            Instantiate(_explosionEffect, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, _destructionRadius);
        foreach (Collider collider in colliders)
        {
            if (((1 << collider.gameObject.layer) & _enemyLayer) != 0)
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
                    if(_showHitIndicator)
                        _signalBus.Fire(new HitSignal(screenPosition));
                    damageable.TakeDamage(Damage);
                }
            }
        }
    }
}