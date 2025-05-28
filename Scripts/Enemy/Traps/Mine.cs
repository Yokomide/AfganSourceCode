using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour, IDamageable
{
   [SerializeField] private ParticleSystem _explodeVFX;
   [SerializeField] private int _damage;
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         other.GetComponent<IDamageable>().TakeDamage(_damage);
         Instantiate(_explodeVFX, gameObject.transform.position, Quaternion.identity);
         Destroy(gameObject);
      }
   }

   public void TakeDamage(int value)
   {
      Instantiate(_explodeVFX, gameObject.transform.position, Quaternion.identity);
      Destroy(gameObject);
   }
}
