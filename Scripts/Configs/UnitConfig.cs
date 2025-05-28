using UnityEngine;

[CreateAssetMenu(fileName = "UnitConfig", menuName = "Configs/UnitConfig", order = 1)]
public class UnitConfig : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private int _id;
    [SerializeField] private int _baseHealth;
    [SerializeField] private int _baseSpeed;
    [SerializeField] private int _baseDamage;
    [SerializeField] private float _baseFireRate = 1f;
    [SerializeField] private int _crew;
    [SerializeField] private UnitType _type;
    [SerializeField] private UnitClass _class;

    public UnitModel CreateUnitModel()
    {
        return new UnitModel(_name, _id, _crew, _baseHealth,1, _baseSpeed, _baseDamage, _type, _class, _baseFireRate);
    }
    
    public string Name => _name;
    public int Id => _id;
    public int Crew => _crew;
    public int BaseHealth => _baseHealth;
    public int BaseSpeed => _baseSpeed;
    public int BaseDamage => _baseDamage;
    
    public float BaseFireRate => _baseFireRate;
    public UnitType Type => _type;
    public UnitClass Class => _class;

}