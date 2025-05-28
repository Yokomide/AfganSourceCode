using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private Image _image;
    [Inject] private SignalBus _signalBus;

    private void Awake()
    {
        _image.enabled = false;
        _signalBus.Subscribe<StartMissionSignal>(() => _image.enabled = true);
    }
}
