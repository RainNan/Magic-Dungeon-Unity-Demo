using System;

public class AttributeManager
{
    private static float _defaultHP = 100;
    private static float _defaultMaxHp = 100;
    private static float _defaultMP = 100;
    private static float _defaultMaxMp = 100;

    private static float _defaultStamina = 100;
    private static float _defaultMaxStamina = 100;

    private static float _defaultATK = 20;
    private static float _defaultATKRange = 1.5f;
    private static float _defaultATKSpeed = 1f;
    private static float _defaultDEF = 10f;
    private static float _defaultSpeed = 7f;

    private static Attribute _attribute;
    public Attribute Attribute => _attribute;

    private static AttributeManager _instance;
    public static AttributeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AttributeManager();
                _attribute = new Attribute();
                Init();
            }

            return _instance;
        }
    }

    private static void Init()
    {
        ResetAllAttributesToDefault();

        EventBus.Instance.Subscribe(EventNames.AddHp, (o) =>
        {
            if (o is float addhp)
            {
                Instance.SetHp(Math.Min(Instance.GetHp() + addhp, Instance.GetMaxHp()));
            }
        });

        EventBus.Instance.Subscribe(EventNames.OnEquipmentChanged,
            (o) =>
            {
                RecalculateAttributesFromEquipment();
            });
    }

    private static void ResetAllAttributesToDefault()
    {
        _attribute.HP = _defaultHP;
        _attribute.MaxHP = _defaultMaxHp;

        _attribute.MP = _defaultMP;
        _attribute.MaxMP = _defaultMaxMp;

        _attribute.Stamina = _defaultStamina;
        _attribute.MaxStamina = _defaultMaxStamina;

        _attribute.ATK = _defaultATK;
        _attribute.ATKRange = _defaultATKRange;
        _attribute.ATKSpeed = _defaultATKSpeed; // 这里是AttackCookDown
        _attribute.DEF = _defaultDEF;
        _attribute.Speed = _defaultSpeed;
    }

    private static void RecalculateAttributesFromEquipment()
    {
        float hpPercent = _attribute.MaxHP <= 0f ? 1f : _attribute.HP / _attribute.MaxHP;
        float mpPercent = _attribute.MaxMP <= 0f ? 1f : _attribute.MP / _attribute.MaxMP;
        float staminaPercent = _attribute.MaxStamina <= 0f ? 1f : _attribute.Stamina / _attribute.MaxStamina;

        _attribute.MaxHP = _defaultMaxHp;
        _attribute.MaxMP = _defaultMaxMp;
        _attribute.MaxStamina = _defaultMaxStamina;
        _attribute.ATK = _defaultATK;
        _attribute.ATKRange = _defaultATKRange;
        _attribute.ATKSpeed = _defaultATKSpeed;
        _attribute.DEF = _defaultDEF;
        _attribute.Speed = _defaultSpeed;

        var equipped = PackageManager.Instance.EquipmentEquipped;
        foreach (var keyValuePair in equipped)
        {
            var equipmentConfig = keyValuePair.Value.equipmentConfig;
            _attribute.ATK += equipmentConfig.damage;
            _attribute.ATKRange += equipmentConfig.attackRange;
        }

        _attribute.HP = Math.Min(_attribute.MaxHP, _attribute.MaxHP * hpPercent);
        _attribute.MP = Math.Min(_attribute.MaxMP, _attribute.MaxMP * mpPercent);
        _attribute.Stamina = Math.Min(_attribute.MaxStamina, _attribute.MaxStamina * staminaPercent);

        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public float GetMaxHp() => _attribute.MaxHP;
    public float GetHp() => _attribute.HP;

    public float GetMp() => _attribute.MP;
    public float GetMaxMp() => _attribute.MaxMP;

    public float GetStamina() => _attribute.Stamina;
    public float GetMaxStamina() => _attribute.MaxStamina;

    public float GetAtk() => _attribute.ATK;
    public float GetAtkRange() => _attribute.ATKRange;
    public float GetAtkSpeed() => _attribute.ATKSpeed;
    public float GetDef() => _attribute.DEF;
    public float GetSpeed() => _attribute.Speed;

    public void SetMaxHp(float value)
    {
        _attribute.MaxHP = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetHp(float value)
    {
        _attribute.HP = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetMp(float value)
    {
        _attribute.MP = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetMaxMp(float value)
    {
        _attribute.MaxMP = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetStamina(float value)
    {
        _attribute.Stamina = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetMaxStamina(float value)
    {
        _attribute.MaxStamina = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetAtk(float value)
    {
        _attribute.ATK = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetAtkRange(float value)
    {
        _attribute.ATKRange = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetAtkSpeed(float value)
    {
        _attribute.ATKSpeed = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetDef(float value)
    {
        _attribute.DEF = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }

    public void SetSpeed(float value)
    {
        _attribute.Speed = value;
        EventBus.Instance.Publish(EventNames.AttributeChanged, _attribute);
    }
}
