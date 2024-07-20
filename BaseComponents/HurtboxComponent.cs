using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class HurtboxComponent : Area2D
{
    #region CLASS_VARIABLES
    [Export]
    public HealthComponent HealthComponent { get; private set; }
    public bool HasHealthComponent { get; private set; }
    public CollisionShape2D CollisionShape { get; private set; }
    public HitboxAttack LatestAttack { get; set; }
    public Node2D LatestAttacker { get; set; }
    public List<HitboxComponent> HitboxesInHurtbox { get; private set; } = new List<HitboxComponent>();
    public List<DamageBodyComponent> DamageBodiesInHurtbox { get; private set; } = new List<DamageBodyComponent>();
    //public HurtboxState State { get; set; }
    public bool Invulnerable { get; set; } = false;
    public bool Defending { get; set; } = false;
    [Signal]
    public delegate void HitboxEnteredEventHandler(HitboxComponent hitbox);
    [Signal]
    public delegate void DamageBodyEnteredEventHandler(DamageBodyComponent damageBody);
    [Signal]
    public delegate void AttackedEventHandler(HitboxAttack attack);
    #endregion
    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        base._Ready();
        CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        //HasHealthComponent = (_healthComponent == null);
	}
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        base._Process(delta);
	}
    #endregion

    #region COMPONENT_FUNCTIONS
    private void HurtImpactEffect(HitboxComponent hitbox)
    {

    }
    #endregion
    #region SIGNAL_LISTENERS
    private void OnAreaEntered(Area2D area)
    {
        if (area is HitboxComponent hitboxComponent) 
        { 
            HitboxesInHurtbox.Add(hitboxComponent);
            HealthComponent?.DamageWithAttack(hitboxComponent.CurrentAttack);
            LatestAttack = hitboxComponent.CurrentAttack;
            LatestAttacker = hitboxComponent.GetOwner<Node2D>();
            EmitSignal(SignalName.Attacked, hitboxComponent.CurrentAttack);
            EmitSignal(SignalName.HitboxEntered, hitboxComponent);
        }
    }
    private void OnAreaExited(Area2D area)
    {
        if (area is HitboxComponent hitboxComponent)
        {
            HitboxesInHurtbox.Remove(hitboxComponent);
        }
    }
    private void OnBodyEntered(Node2D body)
    {
        //// OLD AND SLOW
        //if (body is PhysicsBody2D physicsBody)
        //{
        //    if (physicsBody.GetNode<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentSceneName) is DamageBodyComponent damageBody)
        //    {

        //    }
        //}
        if (body.GetNodeOrNull<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentSceneName) is not null &&
            body.GetNode<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentSceneName) is DamageBodyComponent damageBody)
        {
            DamageBodiesInHurtbox.Add(damageBody);
            HealthComponent?.DamageWithAttack(damageBody.CurrentAttack);
            LatestAttack = damageBody.CurrentAttack;
            //HurtImpactEffect(hitboxComponent);
            EmitSignal(SignalName.DamageBodyEntered, damageBody);
        }
    }
    private void OnBodyExited(Node2D body)
    {
        if (body.GetNodeOrNull<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentSceneName) is not null &&
            body.GetNode<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentSceneName) is DamageBodyComponent damageBody)
        {
            DamageBodiesInHurtbox.Remove(damageBody);
        }
    }

    public void DeactivateHurtbox()
    {
        SetDeferred(PropertyName.Monitorable, false);
        SetDeferred(PropertyName.Monitoring, false);
    }
    public void ReactivateHurtbox()
    {
        SetDeferred(PropertyName.Monitorable, true);
        SetDeferred(PropertyName.Monitoring, true);
    }
    #endregion

    #region HELPER_CLASSES
}
public enum HurtboxState
{
    Vulnerable,
    Defending,
    Invulnerable
}
#endregion




