using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class BreakableComponent : Node2D
{
    #region CLASS_VARIABLES
    public const string ComponentGroupName = "BreakableComponent";

    protected Global Global;
    protected Events SignalBus;

    [ExportGroup("Necessary Components")]
    [Export]
    private HealthComponent _healthComponent;
    [Export]
    private HurtboxComponent _hurtboxComponent;
    [Export]
    private Array<Node2D> _nodesToShakeOnDamage;
    [Export]
    private CollisionShape2D _bodyCollisionShape;
    [Export]
    private PackedScene _brokenPieceScene;
    [ExportGroup("Sfx")]
    [Export]
    public AudioStream OnDamageSfx { get; private set; }
    [Export]
    public AudioStream OnBreakSfx { get; private set; }
    private SfxComponent _sfxComponent;
    private long _damageSfxStreamNum = -1;
    [ExportGroup("Breakable Variables")]
    [Export]
    public float BrokenPieceFullTime { get; private set; } = 1f;
    [Export]
    public float BrokenPieceFadeTime { get; private set; } = 2f;
    public List<RigidBody2D> _brokenPieces { get; set; } = new List<RigidBody2D>();

    //[Export(PropertyHint.Range, "0.0,1.0")]
    //private float _cameraShake = 0.0f;
    [Export]
    public Vector2I BrokenPiecesAmtOnBreakRange { get; private set; } = new Vector2I(0,0);
    [Export]
    public Vector2 PieceForceRange { get; private set; } = new Vector2(1000, 1500);

    
    [Signal]
    public delegate void DamagedEventHandler(float damage, Vector2 hitDirection);
    [Signal]
    public delegate void BreakingEventHandler();
    [Signal]
    public delegate void FullyBrokenEventHandler();
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        Global = GetNode<Global>("/root/Global");
        SignalBus = GetNode<Events>("/root/Events");
        _sfxComponent = GetNode<SfxComponent>("SfxComponent");
        AddToGroup(ComponentGroupName);

        _hurtboxComponent.Attacked += OnHurtboxAttacked;
        //_hurtboxComponent.HitboxExited += OnInteractAreaExited;
        _healthComponent.Destroyed += OnDestroyed;
    }

    public override void _Process(double delta)
    {
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    public virtual void InitBrokenPieces(List<Vector2I> brokenPieceCoords)
    {
        foreach (var bpCoords in brokenPieceCoords)
        {
            var bpBody = _brokenPieceScene.Instantiate() as RigidBody2D;
            bpBody.Position += Global.GetRandomDirection().Normalized() * 5; // seperate broken pieces;
            bpBody.GetNode<Sprite2D>("Sprite2D").FrameCoords = bpCoords;
            bpBody.Hide();
            bpBody.SetDeferred(RigidBody2D.PropertyName.Freeze, true);
            CallDeferred(Node.MethodName.AddChild, bpBody);
            _brokenPieces.Add(bpBody);
        }
    }
    public virtual void OnDamagedBrokenPieces(List<Vector2I> brokenPieceCoords, float force, Vector2? hitDirection = null)
    {
        var bps = InstantiatePieces(brokenPieceCoords);
        PiecesFlyOut(bps, force, hitDirection);
    }
    public virtual void Break(float normForce = 0.25f, Vector2? hitDirection = null)
    {
        EmitSignal(SignalName.Breaking);

        var polyPlayback = _sfxComponent.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
        polyPlayback.PlayStream(OnBreakSfx);
        if (_damageSfxStreamNum != 1 && polyPlayback.IsStreamPlaying(_damageSfxStreamNum))
        {
            polyPlayback.StopStream(_damageSfxStreamNum);
        }

        _bodyCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        foreach (var breakNode in _nodesToShakeOnDamage)
        {
            breakNode.Hide();
        }
        _hurtboxComponent.SetDeferred(Area2D.PropertyName.Monitorable, false);
        _hurtboxComponent.SetDeferred(Area2D.PropertyName.Monitoring, false);
        //CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        //InteractArea.SetDeferred(Area2D.PropertyName.Monitorable, false);
        //InteractArea.SetDeferred(Area2D.PropertyName.Monitoring, false);
        //Shadow.Hide();

        //Global.FreezeEffect(0.05f); 
        //Global.AddCameraTrauma(Mathf.Max(0.3f, (normForce + 0.5f) / 2));
        PiecesFlyOut(_brokenPieces, normForce, hitDirection);

        var endTween = CreateTween();
        //endTween.TweenCallback(Callable.From(() => Global.DropEgyptCoins(MoneyAmt, Position, normForce)));//.SetDelay(0.1f);
        foreach (var bp in _brokenPieces)
        {
            endTween.Parallel().TweenProperty(bp.GetNode<Sprite2D>("Sprite2D"), "modulate:a", 0, BrokenPieceFadeTime).SetEase(Tween.EaseType.In).SetDelay(BrokenPieceFullTime);
        }
        endTween.TweenCallback(Callable.From(QueueFree));//.SetDelay(0.5f);

    }
    public virtual void Damage(float damage, Vector2 hitDirection)
    {
        if (_healthComponent.Health != 0)
        {
            var polyPlayback = _sfxComponent.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
            _damageSfxStreamNum = polyPlayback.PlayStream(OnDamageSfx);
        }
        var scaleMult = damage * 0.5f;
        var posMult = ((damage + 0.2f) / 2f) * 10f;

        var scaleShift = scaleMult * hitDirection;
        var posShift = posMult * hitDirection;
        //WIGGLE
        foreach (var shakeNode in _nodesToShakeOnDamage)
        {
            var scaleTween = CreateTween();
            scaleTween.TweenProperty(shakeNode, "scale", shakeNode.Scale - scaleShift, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
            scaleTween.TweenProperty(shakeNode, "scale", shakeNode.Scale + (scaleShift / 2), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
            scaleTween.TweenProperty(shakeNode, "scale", shakeNode.Scale, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        }

        EmitSignal(SignalName.Damaged, damage, hitDirection);
        //    var posTween = CreateTween();
        //    posTween.TweenProperty(Sprite, "position", Sprite.Position + posShift, 0.075f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
        //    posTween.TweenProperty(Sprite, "position", Sprite.Position - posShift, 0.075f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
        //    posTween.TweenProperty(Sprite, "position", Sprite.Position + posShift, 0.075f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
        //    posTween.TweenProperty(Sprite, "position", Sprite.Position - posShift, 0.075f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
        //    posTween.TweenProperty(Sprite, "position", Sprite.Position, 0.075f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
    }
    private List<RigidBody2D> InstantiatePieces(List<Vector2I> pieceList)
    {
        List<RigidBody2D> pieces = new List<RigidBody2D>();
        foreach (var pieceCoords in pieceList)
        {
            var bpBody = _brokenPieceScene.Instantiate() as RigidBody2D;
            bpBody.Position += Global.GetRandomDirection().Normalized() * 5; // seperate broken pieces;
            bpBody.GetNode<Sprite2D>("Sprite2D").FrameCoords = pieceCoords;
            bpBody.Hide();
            bpBody.SetDeferred(RigidBody2D.PropertyName.Freeze, true);
            CallDeferred(MethodName.AddChild, bpBody);
            pieces.Add(bpBody);
        }
        return pieces;
    }
    public void PiecesFlyOut(List<RigidBody2D> pieces, float force, Vector2? hitDirection = null)
    {
        var pieceTween = CreateTween();

        foreach (var piece in pieces)
        {
            piece.Show();
            var dropDir = hitDirection.HasValue ? (Global.GetRandomDirection().Normalized() + hitDirection.Value).Normalized() : Global.GetRandomDirection().Normalized();
            var distD = Global.Rnd.NextDouble();
            var dropStrength = (float)(distD * (PieceForceRange.Y - PieceForceRange.X) + PieceForceRange.X); //Generates double within a range
            var dropImpulse = force * dropDir * dropStrength;
            piece.SetDeferred(RigidBody2D.PropertyName.Freeze, false);
            piece.CallDeferred(RigidBody2D.MethodName.ApplyCentralImpulse, dropImpulse);
            pieceTween.Parallel().TweenProperty(piece.GetNode<Sprite2D>("Sprite2D"), "modulate:a", 0, BrokenPieceFadeTime).SetEase(Tween.EaseType.In).SetDelay(BrokenPieceFullTime);
            pieceTween.Parallel().TweenCallback(Callable.From(piece.QueueFree)).SetDelay(2.5f);

            //var collTween = CreateTween();
            //if (BreakableHealth > 0)
            //{
            //    piece.SetCollisionMaskValue(4, false);
            //}
            //collTween.TweenCallback(Callable.From(() => piece.SetCollisionMaskValue(4, true))).SetDelay(1.0f);
        }
    }
    #endregion

    #region SIGNAL_LISTENERS
    protected virtual void OnHurtboxAttacked(HitboxAttack attack)
    {
        //if (!(area.GetParent() is Player)) //should always be true but check anyway
        //{
        //    GD.Print("area groups: ", area.GetParent().GetGroups());
        //    //GD.PrintErr(nameof(OnAreaEntered) + " ERROR || Player bag wasn't what hit pot?");
        //    return;
        //}
        //var robber = area.GetParent() as Player;

        //Vector2 hitDirection;
        //if (robber.Velocity.Length() <= robber.IdleVelocity)
        //{ hitDirection = Player.GetVectorFromDirection(robber.FaceDirection) * 0.5f; }
        //else
        //{ hitDirection = robber.Velocity.Normalized(); }
        //GD.Print("robber direction on hit breakable: ", robber.Direction, "\nhitDirection: ", hitDirection, "\nFaceDirection: ", robber.FaceDirection);


        Damage(attack.Damage, attack.Direction);
    }
    protected virtual void OnInteractAreaExited(Area2D area)
    {
    }

    private void OnDestroyed(HealthUpdate destroyUpdate)
    {
        GD.Print("breakable destroyed");
        Break(destroyUpdate.NormalizedForce, destroyUpdate.Direction);
    }
    #endregion

    #region HELPER_CLASSES
    #endregion
}
