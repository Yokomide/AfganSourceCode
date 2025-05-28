using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UnitView : MonoBehaviour
{
    [SerializeField] private Slider _hpBar;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private EntityAudioHandler _audioHandler;


    public void InitializeHealthBar(float maxHealth)
    {
        _hpBar.maxValue = maxHealth;
    }
    public void UpdateHealthBar(float value)
    {
        _hpBar.DOValue(value, 0.1f);
    }
    public void PlayShootEffect()
    {
        _muzzleFlash.Play();
        _audioHandler.PlayShootSound();
    }

}
