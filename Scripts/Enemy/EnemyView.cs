using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [SerializeField] private ParticleSystem _destroyVFX;

    public void PlayDieVFX()
    {
        if (_destroyVFX)
        {
            Instantiate(_destroyVFX, transform.position, quaternion.identity);
        }
    }
    
    
}
