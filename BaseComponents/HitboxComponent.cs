using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using TimeRobbers.BaseComponents;

[GlobalClass]
public partial class HitboxComponent : Area2D
{
    #region CLASS_VARIABLES
    [Export]
    public float BaseDamage { get; private set; }
    [Export]
    public float BaseForce { get; private set; }
    [Export]
    public HitboxAttack BaseAttackStats { get; private set; } = new HitboxAttack(0.0f, 0.0f, Vector2.Zero);
    public HitboxAttack CurrentAttack { get; private set; }
    public bool AttackActive { get; private set; } = false;
    private bool _velocityAttackActive = false;
    public HitboxVelocityAttack VelocityAttack { get; private set; }
    public CollisionShape2D CollisionShape { get; private set; }
    public List<HurtboxComponent> HurtboxesInHitbox { get; private set; } = new List<HurtboxComponent>();

    [Signal]
    public delegate void AttackHitEventHandler(HitboxAttack attack);
    [Signal]
    public delegate void HurtboxEnteredEventHandler(HurtboxComponent hurtbox);
    [Signal]
    public delegate void AttackFinishedEventHandler();
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        Monitorable = false;
        Monitoring = false;
        AttackActive = false;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_velocityAttackActive)
        {
            CurrentAttack = new HitboxAttack(
                damage: VelocityAttack.BaseDamage + VelocityAttack.GetBodyVelocity().Length() * VelocityAttack.VelDamageMult,
                force: VelocityAttack.BaseForce + VelocityAttack.GetBodyVelocity().Length() * VelocityAttack.VelForceMult,
                direction: VelocityAttack.GetBodyVelocity().Normalized()
                );
            //GD.Print("current velocity attack - \ndamage: ", CurrentAttack.Damage, 
            //    "\nforce: ", CurrentAttack.Force,
            //    "\ndirection: ", CurrentAttack.Direction);
        }
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    public void StartBaseAttack()
    {
        Monitorable = true;
        Monitoring = true;
        AttackActive = true;
    }
    public void StartBaseAttackWithTimer(float attackTime)
    {
        Monitorable = true;
        Monitoring = true;
        AttackActive = true;
        GetTree().CreateTimer(attackTime).Timeout += StopAttack;
    }
    public void StartNewAttack(float damage, float force, Vector2 direction)
    {
        CurrentAttack = new HitboxAttack(damage, force, direction);
        Monitorable = true;
        Monitoring = true;
        AttackActive = true;
    }
    public void StartNewAttackWithTimer(float damage, float force, Vector2 direction, float attackTime)
    {
        CurrentAttack = new HitboxAttack(damage, force, direction);
        Monitorable = true;
        Monitoring = true;
        AttackActive = true;
        GetTree().CreateTimer(attackTime).Timeout += StopAttack;
    }
    public void StartVelocityAttack(PhysicsBody2D velBody, float baseDamage, float baseForce, float velDamageMult, float velForceMult)
    {
        VelocityAttack = new HitboxVelocityAttack(velBody, baseDamage, baseForce, velDamageMult, velForceMult);
        Monitorable = true;
        Monitoring = true;
        AttackActive = true;
        _velocityAttackActive = true;
    }
    public void StopAttack()
    {
        SetDeferred(PropertyName.Monitorable, false);
        SetDeferred(PropertyName.Monitoring, false);
        SetDeferred(PropertyName.AttackActive, false);
        SetDeferred(PropertyName._velocityAttackActive, false);

        CallDeferred(MethodName.EmitSignal, SignalName.AttackFinished);
        //do something with current attack?
    }
    #endregion

    #region SIGNAL_LISTENERS
    private void OnAreaEntered(Area2D area)
    {
        if (area is HurtboxComponent hurtboxComponent)
        {
            HurtboxesInHitbox.Add(hurtboxComponent);
            EmitSignal(SignalName.AttackHit, CurrentAttack);
            EmitSignal(SignalName.HurtboxEntered, hurtboxComponent);
        }
    }
    private void OnAreaExited(Area2D area)
    {
        if (area is HurtboxComponent hurtboxComponent)
        {
            HurtboxesInHitbox.Remove(hurtboxComponent);
        }
    }
    private void OnBodyEntered(Node2D body)
    {
        //throw new NotImplementedException();
    }

    private void OnBodyExited(Node2D body)
    {
        //throw new NotImplementedException();
    }
    #endregion

    #region HELPER_CLASSES
    public partial class HitboxVelocityAttack : Resource
    {
        public PhysicsBody2D VelocityBody { get; set; }
        public float BaseDamage { get; private set; }
        public float BaseForce { get; private set; }
        public float VelDamageMult { get; private set; }
        public float VelForceMult { get; private set; }
        public HitboxVelocityAttack()
        {
            VelocityBody = null;
            BaseDamage = 0.0f;
            BaseForce = 0.0f;
            VelDamageMult = 0.0f;
            VelForceMult = 0.0f;
        }
        public HitboxVelocityAttack(PhysicsBody2D velBody, float damage, float force, float velDamageMult, float velForceMult)//, Array<AttackEffect>? attackEffects = null)
        {
            VelocityBody = velBody;
            BaseDamage = damage;
            BaseForce = force;
            VelDamageMult = velDamageMult;
            VelForceMult = velForceMult;
        }
        public Vector2 GetBodyVelocity()
        {
            switch (VelocityBody)
            {
                case Node2D n when n is CharacterBody2D charBody:
                    return charBody.Velocity;
                case Node2D n when n is StaticBody2D staticBody:
                    return staticBody.ConstantLinearVelocity;
                case Node2D n when n is RigidBody2D rigidBody:
                    return rigidBody.LinearVelocity;
                default:
                    GD.PrintErr("Velocity Body isn't one of the three main physics bodies??");
                    return Vector2.Zero;
            }
        }
    }
}
public enum AttackEffect
{
    Poison,
    Stun
}
#endregion