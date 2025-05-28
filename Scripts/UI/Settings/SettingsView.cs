using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsView : MonoBehaviour, IView
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _backButton;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField]
    private AudioClip _sfxSliderSound;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _musicSlider;  
    
    private SettingsPresenter _presenter;
    public void SetPresenter(SettingsPresenter presenter)
    {
        _presenter = presenter;
    }
    void Start()
    {
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        
        _sfxSlider.value = savedSFXVolume;
        _musicSlider.value = savedMusicVolume;
        
        SetSFXVolume(savedSFXVolume);
        SetMusicVolume(savedMusicVolume);
        
        _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        
        _presenter.Initialize();
        Hide();
        _backButton.OnClickAsObservable()
            .Subscribe(_ => Hide())
            .AddTo(this);
    }
    
    public void Show()
    {
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }

    public void SetSFXVolume(float volume)
    {
        
        if (volume <= 0f)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
    
    public void SetMusicVolume(float volume)
    {
        if (volume <= 0f)
        {
            audioMixer.SetFloat("MusicVolume", -80f);
        }
        else
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
}

