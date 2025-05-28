using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionInfoHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _task;
    [SerializeField] private Toggle _successToggle;
    [SerializeField] private Image _checkboxBorderImage;
    [SerializeField] private Color _success;
    [SerializeField] private Color _fail;
    public void SetInfo(string title, string description)
    {
        _title.text = title;
        _task.text = description;
    }

    public void SetSuccess(bool value)
    {
        if (value)
        {
            _task.color = _success;
            _checkboxBorderImage.color = _success;
            _successToggle.isOn = true;
        }
        else
        {
            _title.color = _fail;
            _task.color = _fail;
            _checkboxBorderImage.color = _fail;

        }
    }
    
}
