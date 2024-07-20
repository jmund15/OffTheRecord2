using Godot;
using Godot.Collections;
using System;
using TimeRobbers.BaseComponents;

public partial class Player : CharacterBody2D, IDirectionComponent
{
    #region CLASS_VARIALBLES
    public enum LimbHealthState
    {
        Full,
        Oof,
        Uhoh,
        Joever
    }
    public int LimbCount { get; private set; } = 4;
    public float LimbHealthAmt { get; private set; }
    public float CurrLimbHealth { get; private set; }
    public float LimbHealthStateAmt { get; private set; }
    public LimbHealthState CurrLimbHealthState { get; private set; } = LimbHealthState.Full;
    public Dictionary<LimbHealthState, string> LimbHealthAnimString = new Dictionary<LimbHealthState, string>()
    {
        { LimbHealthState.Full, "Full" },
        { LimbHealthState.Oof, "Oof" },
        { LimbHealthState.Uhoh, "Uhoh" },
        { LimbHealthState.Joever, "Joever" }
    };
    private PlayerStateMachine _stateMachine;
    private Dictionary<State, bool> _parallelStateMachines = new Dictionary<State, bool>();
    public State PrimaryState { get; private set; }
    public Dictionary<State, bool> ParallelStates { get; private set; }

    public AnimatedSprite2D AnimSprite { get; private set; }
    public MovementDirection FaceDirection { get; private set; }
    public Sprite2D Shadow { get; private set; }
    public Vector2 BaseShadowPos { get; private set; } = new Vector2();

    public HealthComponent HealthComponent { get; private set; }
    public HurtboxComponent HurtboxComponent { get; private set; }
    public HitboxComponent HitboxComponent { get; private set; }

    [Export]
    public float AfflictionRate { get; private set; } = 25f;

	public const float WalkSpeed = 2500f;

	public const float MovementTransitionBufferTime = 0.1f;

    public int CuresHeld = 1;
    [Export]
    public float CureSpeed { get; private set; } = 50f;
    public AnimatedSprite2D CureFlare { get; private set; }
    public AnimatedSprite2D CureFlareMask { get; private set; }
    //public const float WalkMinInput = 0.1f;
    public string LeftInput { get; private set; } = "Left";
    public string RightInput { get; private set; } = "Right";
    public string UpInput { get; private set; } = "Up";
    public string DownInput { get; private set; } = "Down";
    public string InjectInput { get; private set; } = "Inject";
    public string InteractInput { get; private set; } = "Interact";

    [Signal]
    public delegate void LoseLimbEventHandler(int newLimbCount);
    [Signal]
    public delegate void LimbHealthStateChangeEventHandler(LimbHealthState newLimbHealthState);
    #endregion

    #region BASE_OVERRIDDEN_GODOT_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        AnimSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        HealthComponent = GetNode<HealthComponent>("HealthComponent");
        HurtboxComponent = GetNode<HurtboxComponent>("HurtboxComponent");
        //_hurtboxCapsuleShape = HurtboxComponent.CollisionShape.Shape as CapsuleShape2D;
        HitboxComponent = GetNode<HitboxComponent>("HitboxComponent");
        //_bagHitboxCapsuleShape = HitboxComponent.CollisionShape.Shape as CapsuleShape2D;

        CureFlare = GetNode<AnimatedSprite2D>("CureFlare");
        CureFlareMask = CureFlare.GetNode<AnimatedSprite2D>("CureFlareMask");
        CureFlare.Hide();
        CureFlareMask.Hide();

        HealthComponent.HealthChanged += OnHealthChanged;
        LimbHealthAmt = HealthComponent.MaxHealth / LimbCount;
        CurrLimbHealth = LimbHealthAmt;
        LimbHealthStateAmt = LimbHealthAmt / 4;
        CurrLimbHealthState = LimbHealthState.Full;

        _stateMachine = GetNode<PlayerStateMachine>("StateMachine");
        _stateMachine.Init(this, AnimSprite);
        PrimaryState = _stateMachine.InitialSubState;
        ParallelStates = _stateMachine.ParallelSubStates;
        _stateMachine.TransitionedState += OnTransitionedState;
        _stateMachine.Enter(_parallelStateMachines);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        HealthComponent.Damage(AfflictionRate * (float)delta);
        if (GetDesiredDirection().X < 0)
        {
            AnimSprite.FlipH = true;
        }
        else
        {
            AnimSprite.FlipH = false;
        }

        _stateMachine.ProcessFrame((float)delta);
    }
    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
        _stateMachine.ProcessPhysics((float)delta);
	}
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        _stateMachine.HandleInput(@event);
    }
    #endregion
    #region SIGNAL_LISTENERS

    private void OnHealthChanged(HealthUpdate healthUpdate)
    {
        CurrLimbHealth = LimbHealthAmt - (healthUpdate.MaxHealth - healthUpdate.NewHealth);
        if (CurrLimbHealth <= 0)
        {
            GD.Print("total health: ", healthUpdate.NewHealth, "\nlimb health: ", CurrLimbHealth);
            //PlayerLoseLimb();
            LimbCount--;
            if (LimbCount == 0)
            {
                GameOver();
            }
            //GD.Print("before set max health");
            //HealthComponent.SetHea(healthUpdate.MaxHealth - LimbHealthAmt);
            HealthComponent.SetMaxHealth(healthUpdate.MaxHealth - LimbHealthAmt);
            //GD.Print("set max health");
            EmitSignal(SignalName.LoseLimb, LimbCount);
        }
        else
        {
            switch (CurrLimbHealth)
            {
                case float n when n >= LimbHealthStateAmt * 3:
                    if (CurrLimbHealthState == LimbHealthState.Full) { return; }
                    CurrLimbHealthState = LimbHealthState.Full;
                    break;
                case float n when n >= LimbHealthStateAmt * 2:
                    if (CurrLimbHealthState == LimbHealthState.Oof) { return; }
                    CurrLimbHealthState = LimbHealthState.Oof;

                    break;
                case float n when n >= LimbHealthStateAmt:
                    if (CurrLimbHealthState == LimbHealthState.Uhoh) { return; }
                    CurrLimbHealthState = LimbHealthState.Uhoh;
                    break;
                default: // lowest limb health state
                    if (CurrLimbHealthState == LimbHealthState.Joever) { return; }
                    CurrLimbHealthState = LimbHealthState.Joever;
                    break;
            }
            FlashRed();
            EmitSignal(SignalName.LimbHealthStateChange, Variant.From(CurrLimbHealthState));

        }
    }
    private void OnTransitionedState(State oldState, State newState)
    {
        //throw new NotImplementedException();
    }
    #endregion
    #region HELPER_FUNCTIONS
    private void PlayerLoseLimb()
    {

    }
    private void FlashRed()
    {

    }
    public float CalcMovementSpeed()
    {
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
    public void GameOver()
    {

    }
    public void UpdateShadowPos()
    {
        //switch (FaceDirection)
        //{
        //    case MovementDirection.RIGHT:
        //        Shadow.Position = BaseShadowPos + new Vector2(7.5f, 0); break;
        //    case MovementDirection.DOWNRIGHT:
        //        Shadow.Position = BaseShadowPos + new Vector2(5f, 0); break;
        //    case MovementDirection.DOWN:
        //        Shadow.Position = BaseShadowPos + new Vector2(0, 0); break;
        //    case MovementDirection.DOWNLEFT:
        //        Shadow.Position = BaseShadowPos + new Vector2(-5f, 0); break;
        //    case MovementDirection.LEFT:
        //        Shadow.Position = BaseShadowPos + new Vector2(-7.5f, 0); break;
        //    case MovementDirection.UPLEFT:
        //        Shadow.Position = BaseShadowPos + new Vector2(-5f, -2.5f); break;
        //    case MovementDirection.UP:
        //        Shadow.Position = BaseShadowPos + new Vector2(0f, -5f); break;
        //    case MovementDirection.UPRIGHT:
        //        Shadow.Position = BaseShadowPos + new Vector2(5f, -2.55f); break;
        //    default:
        //        GD.PrintErr("ERROR || non-standard current y framecoord: " + Sprite.FrameCoords.Y); break;
        //}
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
    public Vector2 GetDesiredDirection()
    {
        //GD.Print(Input.GetVector(LeftInput, RightInput, UpInput, DownInput));
        return Input.GetVector(LeftInput, RightInput, UpInput, DownInput);
    }
    public Vector2 GetDesiredDirectionNormalized()
    {
        return GetDesiredDirection().Normalized();
    }
    #endregion
}
