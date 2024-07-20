using Godot;
using System;
using TimeRobbers.BaseComponents;

public partial class Player : CharacterBody2D, IDirectionComponent
{
    #region CLASS_VARIALBLES
    public int LimbCount { get; private set; } = 4;
    public float LimbHealth { get; private set; }
    public Sprite2D Sprite { get; private set; }
    public AnimationPlayer AnimPlayer { get; private set; }
    public MovementDirection FaceDirection { get; private set; }
    public Sprite2D Shadow { get; private set; }
    public Vector2 BaseShadowPos { get; private set; } = new Vector2();

    public HealthComponent HealthComponent { get; private set; }
    [Export]
    public float AfflictionRate { get; private set; } = 10f;

	public const float WalkSpeed = 300.0f;
	public const float JumpVelocity = -400.0f;

	public const float MovementTransitionBufferTime = 0.1f;
    //public const float WalkMinInput = 0.1f;
    public string LeftInput { get; private set; }
    public string RightInput { get; private set; }
    public string UpInput { get; private set; }
    public string DownInput { get; private set; }
    public string InjectInput { get; private set; }
    public string DefendInput { get; private set; }
    public string LeapInput { get; private set; }
    public string ThrowInput { get; private set; }
    public string ItemInput { get; private set; }

    [Signal]
    public delegate void LoseLimbEventHandler(int newLimbCount);
    #endregion

    #region BASE_OVERRIDDEN_GODOT_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        HealthComponent.HealthChanged += OnHealthChanged;
        LimbHealth = HealthComponent.MaxHealth / LimbCount;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        HealthComponent.Damage(AfflictionRate * (float)delta);
    }
    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		
		MoveAndSlide();
	}
    #endregion
    #region SIGNAL_LISTENERS

    private void OnHealthChanged(HealthUpdate healthUpdate)
    {
        if (healthUpdate.MaxHealth - healthUpdate.NewHealth > LimbHealth)
        {
            LoseLimb();
            LimbCount--;
            HealthComponent.SetMaxHealth(healthUpdate.MaxHealth - LimbHealth);
            EmitSignal(SignalName.LoseLimb, LimbCount);
        }
    }
    #endregion
    #region HELPER_FUNCTIONS
    private void LoseLimb()
    {

    }
    public void UpdateShadowPos()
    {
        switch (FaceDirection)
        {
            case MovementDirection.RIGHT:
                Shadow.Position = BaseShadowPos + new Vector2(7.5f, 0); break;
            case MovementDirection.DOWNRIGHT:
                Shadow.Position = BaseShadowPos + new Vector2(5f, 0); break;
            case MovementDirection.DOWN:
                Shadow.Position = BaseShadowPos + new Vector2(0, 0); break;
            case MovementDirection.DOWNLEFT:
                Shadow.Position = BaseShadowPos + new Vector2(-5f, 0); break;
            case MovementDirection.LEFT:
                Shadow.Position = BaseShadowPos + new Vector2(-7.5f, 0); break;
            case MovementDirection.UPLEFT:
                Shadow.Position = BaseShadowPos + new Vector2(-5f, -2.5f); break;
            case MovementDirection.UP:
                Shadow.Position = BaseShadowPos + new Vector2(0f, -5f); break;
            case MovementDirection.UPRIGHT:
                Shadow.Position = BaseShadowPos + new Vector2(5f, -2.55f); break;
            default:
                GD.PrintErr("ERROR || non-standard current y framecoord: " + Sprite.FrameCoords.Y); break;
        }
    }
    public MovementDirection GetMovementDirection()
    {
        switch (Sprite.FrameCoords.Y)
        {
            case 1:
                return MovementDirection.RIGHT;
            case 2:
                return MovementDirection.DOWNRIGHT;
            case 3:
                return MovementDirection.DOWN;
            case 4:
                return MovementDirection.DOWNLEFT;
            case 5:
                return MovementDirection.LEFT;
            case 6:
                return MovementDirection.UPLEFT;
            case 7:
                return MovementDirection.UP;
            case 8:
                return MovementDirection.UPRIGHT;
            default:
                GD.PrintErr("ERROR || non-standard current y framecoord: " + Sprite.FrameCoords.Y);
                throw new Exception("ERROR || non-standard current y framecoord: " + Sprite.FrameCoords.Y);
                //return MovementDirection.NULL;
        }
    }
    //TODO: CHANGE TO RETURN VARIABLE INSTEAD OF CALCULATING EACH TIME FOR OPTIMIZING????
    public Vector2 GetDesiredDirection()
    {
        float dirMult = 0f;
        switch (LimbCount)
        {
            case 4:
                break;
            case 3:
                break;
            case 2:
                break;
            case 1:
                break; 
            default:
                GD.PrintErr("LIMB COUNT ERROR"); break;
        }
        return Input.GetVector(LeftInput, RightInput, UpInput, DownInput) * dirMult;
    }
    public Vector2 GetDesiredDirectionNormalized()
    {
        return GetDesiredDirection().Normalized();
    }
    #endregion
}
