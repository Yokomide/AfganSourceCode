using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using TMPro;

public class MapHandler : MonoBehaviour
{
    [SerializeField] private SceneSwitchButton _startLevelButton;
    [SerializeField] private MissionInfoHandler _missionInfoHandler;
    [SerializeField] private TextMeshProUGUI _levelDescription;
    [SerializeField] private GameObject _levelPinsHolder;
    [SerializeField] private Toggle _levelCompleteToggle;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _deselectedColor;
    [SerializeField] private AudioSource _pinsAudioSource;
    private Button[] _levelPins;
    private List<Image> _pinImages = new List<Image>();
    private LevelConfig[] _levelConfigs;
    private int _selectedPin = -1;

    void Start()
    {
        _levelPins = _levelPinsHolder.GetComponentsInChildren<Button>();

        _levelConfigs = Resources.LoadAll<LevelConfig>("Levels")
            .OrderBy(l => l.LevelId)
            .ToArray();
        
        for (int i = 0; i < _levelPins.Length; i++)
        {
            int index = i;
            _levelPins[i].OnClickAsObservable()
                .Where(_ => CanSelectPin(index))
                .Subscribe(_ => SelectPin(index))
                .AddTo(this);
            _pinImages.Add(_levelPins[i].GetComponent<Image>());
        }

        LoadLevelData();
        UpdatePinColors();
        SelectLastAvailablePin();
    }

    private void SelectLastAvailablePin()
    {
        for (int i = 0; i < _levelConfigs.Length; i++)
        {
            if (!CanSelectPin(i+1))
            {
                SelectPin(i);
                break;
            }
        }
    }

    private void SelectPin(int index)
    {
        if (_selectedPin == index) return;

        if (_selectedPin != -1)
        {
            _pinImages[_selectedPin].color = _deselectedColor;
            _levelPins[_selectedPin].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }

        _selectedPin = index;
        _pinImages[_selectedPin].color = _selectedColor;
        _levelPins[_selectedPin].GetComponentInChildren<TextMeshProUGUI>().color = _deselectedColor;
        
        var levelConfig = _levelConfigs[index];
        _missionInfoHandler.SetInfo(levelConfig.Title, levelConfig.TaskDescription);
        _levelDescription.text = levelConfig.LevelDiscription;
        var level = GamePersistence.SaveData.Levels.FirstOrDefault(ld => ld.LevelId == index+1);
        if (level != null)
        {
            Debug.Log("Completed: " + level.IsCompleted);
            _levelCompleteToggle.isOn = level.IsCompleted;
        }

        _pinsAudioSource.Play();
        _startLevelButton.SetLevelSettings(levelConfig);
    }
    
    public void LevelCompleted(int levelId)
    {
        var levelData = GamePersistence.SaveData.Levels.FirstOrDefault(ld => ld.LevelId == levelId);
        if (levelData != null)
        {
            levelData.IsCompleted = true;
            GamePersistence.SaveGame();
        }

        UpdatePinColors();
    }
    
    private bool CanSelectPin(int index)
    {
        if (index == 0) return true;

        var previousLevelId = _levelConfigs[index - 1].LevelId;
        var previousLevelData = GamePersistence.SaveData.Levels.FirstOrDefault(ld => ld.LevelId == previousLevelId);

        return previousLevelData != null && previousLevelData.IsCompleted;
    }
    
    private void UpdatePinColors()
    {
        int levelCount = _levelConfigs.Length;
        int pinCount = _levelPins.Length;

        for (int i = 0; i < pinCount; i++)
        {
            if (i >= levelCount)
            {
                Debug.LogWarning($"UpdatePinColors: Отсутствует конфиг для пина {i}");
                _levelPins[i].interactable = false;
                continue;
            }

            bool isCompleted = GamePersistence.SaveData.Levels
                .Any(ld => ld.LevelId == _levelConfigs[i].LevelId && ld.IsCompleted);

            if (CanSelectPin(i))
            {
                _levelPins[i].interactable = true;
                _pinImages[i].color = _deselectedColor;
                _levelPins[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }
            else
            {
                _levelPins[i].interactable = false;
                _pinImages[i].color = _deselectedColor;
                _levelPins[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }
        }
    }


    private void LoadLevelData()
    {
        var savedLevels = GamePersistence.SaveData.Levels.ToDictionary(ld => ld.LevelId);
        for (int i = 1; i < savedLevels.Count; i++)
        {
            Debug.Log($"Level {savedLevels[i].LevelId} complete: {savedLevels[i].IsCompleted}");
        }
        foreach (var levelConfig in _levelConfigs)
        {
            if (!savedLevels.ContainsKey(levelConfig.LevelId))
            {
                GamePersistence.SaveData.Levels.Add(new LevelData(levelConfig.LevelId));
            }
        }

        GamePersistence.SaveGame();
        UpdatePinColors();
    }
}