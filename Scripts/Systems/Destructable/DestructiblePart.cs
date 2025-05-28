using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class DestructiblePart : MonoBehaviour, IDamageable
{
    [SerializeField] private bool _explodeImmediately = false;
    [SerializeField] private bool _isKeyPart = false;
    [SerializeField] private float _destroyDelayAfterFall = 1f;
    [SerializeField] private GameObject _explosionPrefab = null;
    [SerializeField] private List<DestructiblePart> _dependentParts = new List<DestructiblePart>();
    [SerializeField] private float _max_health = 100f;
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private int _explosionDamage = 50;
    [SerializeField] private LayerMask _damageableLayer;
    
    private float _health;
    private bool _isDestroyed = false;
    private Rigidbody _rb;
    private DestructibleObject _destructibleObject;

    public bool IsKeyPart => _isKeyPart;
    public bool IsDestroyed => _isDestroyed;
    public List<DestructiblePart> DependentParts => _dependentParts;

    public void Initialize(DestructibleObject destructibleObject)
    {
        _destructibleObject = destructibleObject;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _health = _max_health;
        
        if (_destructibleObject == null)
        {
            Debug.LogWarning($"No DestructibleObject found in parents of {gameObject.name}!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDestroyed) return;

        _health -= damage;
        if (_health <= 0)
        {
            DestroyPart();
        }
    }

    public void DestroyPart()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;

        transform.SetParent(null);

        if (_explodeImmediately)
        {
            HandleImmediateExplosion();
        }
        else
        {
            HandleFallingPart();
        }

        if (_isKeyPart && _destructibleObject != null)
        {
            _destructibleObject.OnKeyPartDestroyed(this);
        }
    }

    private void HandleImmediateExplosion()
    {
        _rb.isKinematic = false;
        _rb.AddExplosionForce(500f, transform.position, 5f);

        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }

        Deal_explosionDamage();

        Destroy(gameObject);
    }

    private void HandleFallingPart()
    {
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }

        _rb.isKinematic = false;
        StartCoroutine(CheckFallAndDestroy());
    }

    private IEnumerator CheckFallAndDestroy()
    {
        while (true)
        {
            if (_rb.velocity.magnitude >= 0.1f)
            {
                yield return new WaitForSeconds(_destroyDelayAfterFall);
                
                if (_explosionPrefab != null)
                {
                    Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
                }

                Deal_explosionDamage();
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }
    }

    private void Deal_explosionDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius, _damageableLayer);
        foreach (Collider collider in colliders)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null && damageable != this)
            {
                damageable.TakeDamage(_explosionDamage);
            }
        }
    }
}
