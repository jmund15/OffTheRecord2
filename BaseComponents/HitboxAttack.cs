using Godot;
using Godot.Collections;
using System;
using TimeRobbers.BaseComponents;

[GlobalClass]
public partial class HitboxAttack : Resource
{
    [Export]
    public float Damage { get; private set; }
    [Export]
    public float Force { get; private set; }
    //public float NormForce { get; private set; }
    [Export]
    public Vector2 Direction { get; private set; }
    [Export]
    public MovementDirection FaceDirection { get; private set; }
    //Dictionary<AttackEffect, float> AttackEffects
    //[Export]
    //public Array<AttackEffect> AttackEffects { get; private set; }
    public HitboxAttack()
    {
        Damage = 0.0f;
        Force = 0.0f;
        Direction = Vector2.Zero;
        FaceDirection = MovementDirection.DOWN;
        //AttackEffects = new Array<AttackEffect>();
    }
    public HitboxAttack(float damage, float force, Vector2 direction)//, Array<AttackEffect>? attackEffects = null)
    {
        Damage = damage;
        Force = force;
        Direction = direction;
        FaceDirection = IDirectionComponent.GetDirectionFromVector(direction);
        //AttackEffects = attackEffects ?? new Array<AttackEffect>();
    }
    //public void SetNormForce(float minForce, float maxForce)
    //{
    //    NormForce = Global.NormalizeNumber(Force, minForce, maxForce);
    //}
}
