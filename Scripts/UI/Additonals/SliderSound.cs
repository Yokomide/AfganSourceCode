using UnityEngine;
using UnityEngine.UI;

public class SliderSound : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource; 
    [SerializeField] private AudioClip _soundClip;  
    [SerializeField] private float _step = 0.05f;  

    private float _lastValue;    

    void Start()
    {
        Slider slider = GetComponent<Slider>();
        _lastValue = slider.value; 
        slider.onValueChanged.AddListener(PlaySoundOnStep);
    }

    void PlaySoundOnStep(float newValue)
    {
        if (Mathf.Abs(newValue - _lastValue) >= _step)
        {
            _audioSource.PlayOneShot(_soundClip);
            _lastValue = newValue;
        }
    }
}