using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAudioHandler : MonoBehaviour
{
    [SerializeField] private AudioClip _shootSound;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _shootAudioSource;
    [SerializeField] private AudioSource _moveAudioSource;
    
    [Header("Pitch Settings")]
    public float minPitch = 0.8f; 
    public float maxPitch = 1.2f; 
    
    public void PlayShootSound()
    {
        float randomPitch = Random.Range(minPitch, maxPitch);
        _shootAudioSource.pitch = randomPitch;
        _shootAudioSource.PlayOneShot(_shootSound);
    }
}
