using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node2D
{
    #region CLASS_VARIABLES
    [Export]
    public float MaxHealth {
        get { return _maxHealth; }
        private set 
        { 
            if (_maxHealth != value)
            {
                if (!_healthInitialized)
                {
                    _maxHealth = value;
                    return;
                }
                if (ChangeHealthOnMaxChange)
                {
                    var change = value - _maxHealth;
                    Health += change;
                }
                _maxHealth = value;
                if (Health > _maxHealth)
                {
                    Health = _maxHealth; // cap health at max
                }
                EmitSignal(SignalName.MaxHealthChanged, _maxHealth);
            }
        }
    }
    public float Health
    {
        get { return _health; }
        private set 
        { 
            if (!_healthInitialized)
            {
                //_health = Mathf.Clamp(value, 0f, MaxHealth);
                _health = value;
                return;
            }
            var prevHealth = _health;
            _health = Mathf.Clamp(value, 0f, MaxHealth);
            var healthUpdate = new HealthUpdate(prevHealth, _health, MaxHealth, _damageForce, _damageDir);
            EmitSignal(SignalName.HealthChanged, healthUpdate);
            if (_health == 0f)
            {
                if (!IsDead)
                {
                    IsDead = true;
                    EmitSignal(SignalName.Destroyed, healthUpdate);
                }
            }
            else
            {
                IsDead = false;
            }
            
        }
    }

    [Export]
    public bool ChangeHealthOnMaxChange { get; private set; } = true;

    public bool IsDead
    {
        get { return _isDead; }
        set
        {
            if (value != _isDead)
            {
                _isDead = value;
            }
        }
    }

    private float _maxHealth;
    private float _health;
    private float _damageForce = 0.0f;
    private Vector2 _damageDir = Vector2.Zero;
    private bool _isDead = false;
    private bool _healthInitialized = false;

    [Signal]
    public delegate void MaxHealthChangedEventHandler(float newMax);
    [Signal]
    public delegate void HealthChangedEventHandler(HealthUpdate healthUpdate);
    [Signal]
    public delegate void DestroyedEventHandler(HealthUpdate destroyUpdate);
    [Signal]
    public delegate void RessurectEventHandler(HealthUpdate ressurectUpdate);
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        CallDeferred(MethodName.InitializeHealth);
        //InitializeHealth();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    public virtual void InitializeHealth()
    {
        Health = MaxHealth;
        IsDead = false;
        _healthInitialized = true;
    }
    public virtual void Damage(float damage, float force = HealthUpdate._defaultNormForce, Vector2? direction = null)
    {
        _damageForce = force;
        _damageDir = direction ?? HealthUpdate._defaultDir;
        Health -= damage;
    }
    public virtual void DamageWithAttack(HitboxAttack attack)
    {
        _damageForce = attack.Force;
        _damageDir = attack.Direction;
        Health -= attack.Damage;
    }
    public virtual void Heal(float healAmt)
    {
        Damage(-healAmt);
    }

    public virtual void SetMaxHealth(float newMax)
    {
        MaxHealth = newMax;
    }
    #endregion

    #region SIGNAL_LISTENERS
    #endregion

    #region HELPER_CLASSES
    
}
public partial class HealthUpdate : Resource
{
    public static readonly Vector2 _defaultDir = new Vector2(0, 0);
    public const float _defaultNormForce = 0.0f;
    public float NewHealth { get; private set; }
    public float PreviousHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float NormalizedForce { get; private set; }
    public Vector2 Direction { get; private set; }
    public HealthUpdate(float newHealth, float previousHealth, float maxHealth, float normForce = _defaultNormForce, Vector2? direction = null)
    {
        NewHealth = newHealth;
        PreviousHealth = previousHealth;
        MaxHealth = maxHealth;
        NormalizedForce = normForce;
        Direction = direction ?? _defaultDir;
    }
}
#endregion
