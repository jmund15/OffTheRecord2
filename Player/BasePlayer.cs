using Godot;
using Godot.Collections;
using System;
using System.Security.Cryptography;
using TimeRobbers.BaseComponents;

public abstract partial class BasePlayer : CharacterBody2D, IDirectionComponent
{
    #region CLASS_VARIALBLES
    protected Global Global;
    public int LimbCount { get; set; }

    protected CompoundState _stateMachine;
    protected Dictionary<State, bool> _parallelStateMachines = new Dictionary<State, bool>();
    public State PrimaryState { get; protected set; }
    public Dictionary<State, bool> ParallelStates { get; protected set; }
    public bool CanMove { get; set; } = true;

    public AnimatedSprite2D AnimSprite { get; protected set; }
    public MovementDirection FaceDirection { get; protected set; }
    public Sprite2D Shadow { get; protected set; }
    public Vector2 BaseShadowPos { get; protected set; } = new Vector2();

    public HurtboxComponent HurtboxComponent { get; protected set; }
    public HitboxComponent HitboxComponent { get; protected set; }

    public float CurrentSpeed { get; protected set; } = 3000f;

    public const float MovementTransitionBufferTime = 0.1f;
    #endregion

    #region BASE_OVERRIDDEN_GODOT_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        Global = GetNode<Global>("/root/Global");

        AnimSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        HurtboxComponent = GetNode<HurtboxComponent>("HurtboxComponent");
        //_hurtboxCapsuleShape = HurtboxComponent.CollisionShape.Shape as CapsuleShape2D;
        HitboxComponent = GetNode<HitboxComponent>("HitboxComponent");
        //_bagHitboxCapsuleShape = HitboxComponent.CollisionShape.Shape as CapsuleShape2D;

        


        //var limb = SeveredLimbScene.Instantiate<SeveredLimb>();
        //AddChild(limb);
        //limb.LimbSprite.Play("arm");
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (CanMove)
        {
            if (GetDesiredDirection().X < -0.1)
            {
                AnimSprite.FlipH = true;
            }
            else if (GetDesiredDirection().X > 0.1)
            {
                AnimSprite.FlipH = false;
            }
        }
        _stateMachine.ProcessFrame((float)delta);
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _stateMachine.ProcessPhysics((float)delta);
    }
    #endregion
    #region SIGNAL_LISTENERS
    public virtual void OnTransitionedState(State oldState, State newState)
    {
        //throw new NotImplementedException();
    }
    #endregion
    #region HELPER_FUNCTIONS
    public virtual void InitStateMachine()
    {
        _stateMachine = GetNode<CompoundState>("StateMachine");
        _stateMachine.Init(this, AnimSprite);
        PrimaryState = _stateMachine.InitialSubState;
        ParallelStates = _stateMachine.ParallelSubStates;
        _stateMachine.TransitionedState += OnTransitionedState;
        _stateMachine.Enter(_parallelStateMachines);
    }
    public virtual float CalcMovementSpeed()
    {
        return 1;
        switch (LimbCount)
        {
            case 4:
                return 1;
            case 3:
                return 0.8f;
            case 2:
                return 0.6f;
            case 1:
                return 0.4f;
            default:
                throw new Exception("LIMB COUNT ERROR");
        }
    }
    public MovementDirection GetMovementDirection()
    {
        if (AnimSprite.FlipH)
        {
            return MovementDirection.LEFT;
        }
        else
        {
            return MovementDirection.RIGHT;
        }
    }
    //TODO: CHANGE TO RETURN VARIABLE INSTEAD OF CALCULATING EACH TIME FOR OPTIMIZING????
    public abstract Vector2 GetDesiredDirection();
    public abstract Vector2 GetDesiredDirectionNormalized();
    #endregion
}
