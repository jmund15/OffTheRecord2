using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using TimeRobbers.BaseComponents;


public partial class Monster : BasePlayer
{
    public bool InitCutscene;

    private Timer _screenExitedTimer;
    [Export]
    private PackedScene _bloodTrail;
    private Timer _bloodDropTimer;
    private Color _bloodColor = new Color("dc00dc");

    private VisibleOnScreenNotifier2D _visibleOnScreenNotifier;
    private float _preExitScreenDist;
    public Vector2 StuckTeleport { get; set; } = Vector2.Zero;
    private bool _setLungeTimer = false;
    private bool _lunged = false;
    private bool _checkedPounce = false;
    [Export]
    public float LungeSpeed = 9000f;
    [Export]
    private float _pounceSpeed = 9000f;
    [Export]
    private float _chaseSpeed = 6000f;
    [Export]
    private float _walkSpeed = 3000f;

    private Timer _pathCalcTimer;
    private bool _calcPath = true;
    private void OnPathTimeout()
    {
        _calcPath = true;
    }
    
    [Signal]
    public delegate void LungeEventHandler();
    [Signal]
    public delegate void DevourLimbEventHandler(int newLimbCount);
    public bool AttackedPlayer { get; set; } = false;
    public List<SeveredLimb> LimbsToDevour { get; set; } = new List<SeveredLimb>();
    public SeveredLimb LimbDevouring { get; set; }

    private AnimatedSprite2D _toyWithMonster;
    private bool _flickerDone = true;
    private bool _toyDone = false;
    #region AI_STATES
    enum AI_MAIN_BEHAVIOR_STATE
	{
		CHASE, //Attempt to Catch Player Directly
		WANDER, //Be spooky
		TOY_WITH, //Actively Dick with player
		STAND_THERE_MENICINGLY, //Exactly what it says on the tin
		IMPEDE, //Get infront of the player and let it run into you
		GUARD, //Actively guard area
        POUNCE, //Wait outside of vision and leap at player
        FIND_LIMB
    }

    #region AI_SUB_STATES

    enum AI_SUB_CHASE_STATE
	{
		LOCATE, //Stand and try to find player
		PURSUE, //Bee-line for Player
		CHARGE, //Accelerate in straight line to catch player
		BLOCKED, //Blocked and cannot nav to player directly
		JUKED //Continue for a moment, then Re-orient after missing a charge
	}

	enum AI_SUB_WANDER_STATE
	{
		STAY_OUT_OF_VIEW, //Avoid being in view port entirely
	}

	enum AI_SUB_TOY_WITH_STATE
	{
		BE_IN_FRONT_OF, //Teleport to a Waypoint Player is moving towards
		SHAPE_FLICKER, //Change sprite frame to nightmare for a frame or two flicker in in view port
		MAKE_SOUND //Make a noise
	}

	enum AI_SUB_STAND_THERE_MENICINGLY_STATE
	{
        BE_SPOOKY //Stand menicingly
        
    }

	enum AI_SUB_IMPEDE_STATE
	{
		GET_CLOSE, //Nav to near player
		ADJUST //Adjust intermediate position to be in player's path

	}

	enum AI_SUB_POUNCE_STATE
	{
		ASSUME_POSITION, //Position near, out of camera view
		YEET //Do the funny jumpscare
	}

	enum AI_SUB_GUARD_STATE
	{
		MOVE_TO_GOAL, //Move towards player goal
		PATROL_GOAL_AREA, //Move near player goal
	}

    enum AI_SUB_FIND_LIMB_STATE
    {
        MOVE_TO_LIMB
    }
    #endregion
    #endregion

    #region AI_STATE_FUNCS

    //Current AI State
    private AI_MAIN_BEHAVIOR_STATE _mainState = AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY; //Current AI State, start as do nothing 
    private AI_MAIN_BEHAVIOR_STATE CurrentMainState
    {
        get => _mainState;
        set
        {
            if (value == _mainState) { return; }
            _mainState = value;
            GD.Print("Main state change into: ", _mainState);
        }
    } 

    private int _subState = 0; //Current AI Substate, start as do nothing. This is an int so I can cheat and use different enum types. ALWAYS CAST SUB STATE TO INT WHEN CHANGING OR REFERENCING IT

    private int currentSubState
    {
        get => _subState;
        set
        {
            if (value == _subState) { return; }
            _subState = value;
            GD.Print("Sub state change into: ", _subState);
        }
    }
    private AI_MAIN_BEHAVIOR_STATE lastMainState = AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY;
    private int lastSubState = 0;

    //Stored AI state for toy_with states to switch to and from to
    private AI_MAIN_BEHAVIOR_STATE storeMainState = AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY;
    private int storeSubState = 0;

    //Time since last toy with occured
    private double toyWithTimer = 0.0;
    private double nextToyWithTime = 30.0;
    private bool toy = false;

    //Timers for behavior logic
	private double timeIdle = 0.0;
    private double idleThreshold = -1.0;
    private double timeMoving = 0.0;
    private double movingThreshold = -1.0;

    private double timeInMainBehavior = 0.0;
    private double timeInSubBehavior = 0.0;

    private double lastDeltaTime = 0.0;

    private NavigationAgent2D AI_navAgent;
    public Player ProtagRef;
    public bool newMove = false;
    private readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());

    //Waypoint of interest
    private int currentWaypointID = -1;

    //Distance to charge from
    private const double CHARGE_DIST = 50; //TODO: Find distance to charge from.

   


    #region AI_SUB_FUNC
    private void AI_chaseState()
	{
		switch (currentSubState) 
		{
			case (int)AI_SUB_CHASE_STATE.LOCATE:
                if (!AI_isVisible(Position)) { changeSubState((int)AI_SUB_CHASE_STATE.PURSUE); break; }
                CurrentSpeed = _walkSpeed;
                //NO INPUT HERE
                if (idleThreshold<0.0) {
                    idleThreshold = randInRange(1, 2.5); //set idle time
                }
                timeIdle += lastDeltaTime;
                if (timeIdle>=idleThreshold)
                {
                    changeSubState((int)AI_SUB_CHASE_STATE.PURSUE); //Start to chase
                }
				break;

            case (int)AI_SUB_CHASE_STATE.PURSUE:
                CurrentSpeed = _walkSpeed;
                //Nav to player point directly using nav agent
                if (movingThreshold < 0.0)
                {
                    movingThreshold = randInRange(2.5, 5); //set movemax time
                }
                AI_makePath(ProtagRef.Position);
                //AI_navAgent.TargetPosition = ProtagRef.Position; //Nav to Player
                
                //TODO:MOVEMENT

                timeMoving += lastDeltaTime;

                //GD.Print("timeMoving: ", timeMoving);

                if (timeMoving >= movingThreshold) //Change to locate if not found
                {
                    changeSubState((int)AI_SUB_CHASE_STATE.LOCATE);
                }

                //GD.Print("Distance to Player: ", this.Position.DistanceTo(ProtagRef.Position));

                if (AI_navAgent.DistanceToTarget() <= CHARGE_DIST * 1.5f)
                {
                    _lunged = false;
                    AI_makePath(ProtagRef.Position);
                    changeSubState((int)AI_SUB_CHASE_STATE.CHARGE); //Swap to charge behavior when close
                }
                break;

            case (int)AI_SUB_CHASE_STATE.CHARGE:
                if (movingThreshold < 0.0)
                {
                    movingThreshold = randInRange(1, 2.5); //set movemax time
                }
                CurrentSpeed = _chaseSpeed;
                if (AI_navAgent.DistanceToTarget() <= CHARGE_DIST && !_lunged) { //TODO FIND THE DIST NUMBER TO LUNGE
                    EmitSignal(SignalName.Lunge);
                    _lunged = true;
                }
                timeMoving += lastDeltaTime;

                //GD.Print("timeMoving: ", timeMoving);

                if (timeMoving >= movingThreshold)
                {
                    changeSubState((int)AI_SUB_CHASE_STATE.JUKED);
                }

                if (Velocity.IsZeroApprox()) //Find when stopped
                {
                    GD.Print("Missed Player");
                    _lunged = false;
                    changeSubState((int)AI_SUB_CHASE_STATE.JUKED);
                }
                 break;

            case (int)AI_SUB_CHASE_STATE.BLOCKED:
                //AI Can't Nav to player point directly
                //???
                break;

            case (int)AI_SUB_CHASE_STATE.JUKED:
                //Did not catch with charge, act confused for a moment, then go
                if (Velocity.IsZeroApprox() || AI_navAgent.DistanceToTarget() <= 25f) //Find when stopped
                {
                    if (Rnd.NextDouble() <= 0.75) //Flip Coin
                    {
                        if (Rnd.NextDouble() <= 0.75) //Flip Coin
                        {
                            changeSubState((int)AI_SUB_CHASE_STATE.LOCATE); // Look around
                        }
                        else {
                            rollAIState(); //re-roll the AI State
                        }
                    }
                    else
                    {
                        changeSubState((int)AI_SUB_CHASE_STATE.PURSUE); //Keep Pursuing
                    }
                }


                break;

        }
    }

    private void AI_guardState()
    {
        rollAIState();
        return;
        switch (currentSubState)
        {
            case (int)AI_SUB_GUARD_STATE.MOVE_TO_GOAL:
                //Nav to Goal
                if (AI_navAgent.DistanceToTarget() <= 50f) { 
                    changeSubState((int)AI_SUB_GUARD_STATE.PATROL_GOAL_AREA);
                }
                break;

            case (int)AI_SUB_GUARD_STATE.PATROL_GOAL_AREA:
                //Move in Goal Area
                if (movingThreshold < 0.0)
                {
                    movingThreshold = randInRange(3.0, 4.0); //set idle time
                    if (idleThreshold < 0.0)
                    {
                        idleThreshold = movingThreshold * 3;
                    }
                    AI_makeClosePath(this.Position + new Vector2((float)randInRange(1.0, 20.0)*(float)Rnd.Next(-1, 2), (float)randInRange(1.0, 20.0) * (float)Rnd.Next(-1, 2))); //Get new random close point within 20 coords????
                }

                //HANDLE MOVE IN PATROL PATH HERE

                timeMoving += lastDeltaTime;
                timeIdle += lastDeltaTime;
                
                if (timeMoving > movingThreshold)
                {
                    movingThreshold = -1.0; //Reset and make a new path on next loop
                }

                if (timeIdle > idleThreshold)
                {
                    rollAIState(); //Roll if guard provides nothing
                }

                if (this.Position.DistanceTo(ProtagRef.Position) < 100f)//TODO: FIND SEARCH DISTANCE
                {
                    changeMainState(AI_MAIN_BEHAVIOR_STATE.CHASE, (int)AI_SUB_CHASE_STATE.PURSUE); //Pursue if found
                }

                break;
        }
    }

    private void AI_impedeState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_IMPEDE_STATE.GET_CLOSE:
                if (movingThreshold < 0.0)
                {
                    movingThreshold = randInRange(5, 7.5); //set movemax time
                }

                timeMoving += lastDeltaTime;

                if (timeMoving >= movingThreshold) //Change to locate if not found
                {
                    rollAIState(); return;
                }
                CurrentSpeed = _chaseSpeed;
                //Nav to general area of player
                AI_makePath(ProtagRef.Position);
                if (AI_navAgent.DistanceToTarget() <= 75) //TODO: Find Impede distance
                {
                    changeSubState((int)AI_SUB_IMPEDE_STATE.ADJUST);
                }

                break;


            case (int)AI_SUB_IMPEDE_STATE.ADJUST:
                if (!AI_isVisible(Position)) { rollAIState(); return; }
                CurrentSpeed = _walkSpeed;
                //Move in mirror of player for a bit. 

                if (movingThreshold < 0)
                {
                    movingThreshold=randInRange(2.5, 5); // Be in way for 3-10 seconds
                }

                AI_navAgent.TargetPosition = Position + (ProtagRef.GetDesiredDirection() * 10);
                //protag ref input = AI input

                timeMoving += lastDeltaTime;

                if (timeMoving> movingThreshold)
                {
                    if (Rnd.NextDouble() <= 0.50)
                    {
                        if (Rnd.NextDouble() <= 0.6f) {
                            changeMainState(AI_MAIN_BEHAVIOR_STATE.CHASE, (int)AI_SUB_CHASE_STATE.PURSUE);
                        }
                        else
                        {
                            changeSubState((int)AI_SUB_IMPEDE_STATE.GET_CLOSE);  // Get Close Again
                        }
                    }
                    else
                    {
                        rollAIState(); //Re-roll AI state when done
                    }
                }


                break;


        }
    }

    private void AI_pounceState() //Pounce will only occur when checking for healing
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_POUNCE_STATE.ASSUME_POSITION:
                if (movingThreshold < 0)
                {
                    movingThreshold = randInRange(5, 10); 
                }
                timeMoving += lastDeltaTime;

                if (timeMoving > movingThreshold)
                {
                    rollAIState(); return;
                }
                CurrentSpeed = _pounceSpeed;
                AI_makePath(ProtagRef.Position);

                //Stand there out of frame until player is out of healing state

                if (AI_navAgent.DistanceToTarget() >= CHARGE_DIST) //TODO: FIND OUT OF VIEW DISTANCE FOR PLAYER
                {
                    //MOVE TOWARDS HEALING PLAYER
                }
                else
                {
                    AI_navAgent.TargetPosition = Position;
                    _lunged = true;
                    changeSubState((int)AI_SUB_POUNCE_STATE.YEET);
                }

                break;

            case (int)AI_SUB_POUNCE_STATE.YEET:
                if (!_setLungeTimer)
                {
                    _setLungeTimer = true;
                    GetTree().CreateTimer(0.5f).Timeout += () =>
                    {
                        _lunged = false;
                    };
                }
                if (!ProtagRef.Healing && !_lunged)
                {
                    AI_navAgent.TargetPosition = ProtagRef.Position;
                    //Do the Lunge Thing
                    EmitSignal(SignalName.Lunge);
                    CurrentSpeed = _walkSpeed;
                    _lunged = true;
                }
                //Did not catch with charge, act confused for a moment, then go
                if (Velocity.IsZeroApprox() && _lunged) //Find when stopped
                {
                    _lunged = false;
                    _setLungeTimer = false;
                    if (Rnd.NextDouble() <= 0.75) //Flip Coin
                    {
                        if (Rnd.NextDouble() <= 0.5) //Flip Coin
                        {
                            changeMainState(AI_MAIN_BEHAVIOR_STATE.CHASE, (int)AI_SUB_CHASE_STATE.LOCATE);
                        }
                        else
                        {
                            changeMainState(AI_MAIN_BEHAVIOR_STATE.CHASE, (int)AI_SUB_CHASE_STATE.PURSUE);
                        }
                    }
                    else
                    {
                        changeMainState(AI_MAIN_BEHAVIOR_STATE.CHASE, (int)AI_SUB_CHASE_STATE.JUKED);
                    }
                }

                break;
        }
    }

    private void AI_standThereMenicinglyState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_STAND_THERE_MENICINGLY_STATE.BE_SPOOKY:
                if (!AI_isVisible(Position)) { rollAIState(); return; }
                AI_navAgent.TargetPosition = Position;
                //Stand there for a bit, reference timeIdle
                if (idleThreshold < 0)
                {
                    idleThreshold = randInRange(1, 3);
                }

                timeIdle += lastDeltaTime;

                if (timeIdle > idleThreshold)
                {
                    rollAIState(); return;
                }

                break;

        }
    }

    private void AI_toyWithState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_TOY_WITH_STATE.BE_IN_FRONT_OF:
                if (!_flickerDone || _toyDone) { break; }
                if (AI_isVisible(Position) && (Global.CandleGroupsCompleted <= 3 && ProtagRef.LimbCount > 1))
                {
                    changeSubState((int)AI_SUB_TOY_WITH_STATE.MAKE_SOUND); return;
                }
                AI_makePath(Position);
                if (ProtagRef.GetDesiredDirection().IsZeroApprox())
                {
                    _toyWithMonster.GlobalPosition = ProtagRef.Position + (ProtagRef.PreZeroInput * (float)randInRange(50, 100));
                }
                else
                {
                    _toyWithMonster.GlobalPosition = ProtagRef.Position + (ProtagRef.GetDesiredDirectionNormalized() * (float)randInRange(50, 80));
                }
                _toyWithMonster.Show();
                ToyWithFront(Rnd.Next(1, 4));
                AI_Audio(Rnd.Next(1, 25));
                GD.Print("TOY IN FRONT OF");
                break;
            case (int)AI_SUB_TOY_WITH_STATE.SHAPE_FLICKER:
                if (!_flickerDone || _toyDone) { break; }
                if (AI_isVisible(Position) && (Global.CandleGroupsCompleted <= 3 && ProtagRef.LimbCount > 1))
                {
                    changeSubState((int)AI_SUB_TOY_WITH_STATE.MAKE_SOUND); return;
                }
                AI_makePath(Position);
                _toyWithMonster.GlobalPosition = ProtagRef.Position + ((Global.GetRandomDirection().Normalized() * (float)randInRange(70, 125)));
                _toyWithMonster.Show();
                ToyWithFlicker(Rnd.Next(1,11)); 
                AI_Audio(Rnd.Next(1, 25));
                GD.Print("TOY FLICKER");
                break;
            case (int)AI_SUB_TOY_WITH_STATE.MAKE_SOUND:
                if (!AI_isVisible(Position))
                {
                    changeSubState(Rnd.Next(0,2)); return;
                }
                //Play Audio clip
                AI_Audio(Rnd.Next(1,25));
                _toyDone = true; //Set ToyDone since effect is good
                break;
            
        }
        if (_toyDone) 
        {
            //Resume last action or re-roll
            if (lastMainState == AI_MAIN_BEHAVIOR_STATE.TOY_WITH)
            {
                rollAIState(); //If it was a dedicated toy with roll
            }
            else
            {
                changeMainState(lastMainState, lastSubState);
            }
            toy = false;
        }
    }

    private void AI_wanderState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_WANDER_STATE.STAY_OUT_OF_VIEW:
                if (!AI_isVisible(Position)) { rollAIState(); return; }

                AI_makePath(Position + Global.GetRandomDirection() * 300);
                CurrentSpeed = _walkSpeed;

                //Nav to Closest valid point not in the view port

                if (idleThreshold < 0)
                {
                    idleThreshold = randInRange(1.5, 5);
                    //Vector2 posIndex = this.GlobalPosition;
                    //Vector2 wanderPos = Position + (Global.GetRandomDirection() * 50);
                    //AI_makeClosePath(wanderPos);
                }
                
                timeIdle += lastDeltaTime;

                if (timeIdle > idleThreshold)
                {
                    rollAIState();
                }

                break;


        }
    }
    private void AI_FindLimbState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_FIND_LIMB_STATE.MOVE_TO_LIMB:
                CurrentSpeed = _pounceSpeed;
                //Stand there for a bit, reference timeIdle
                if (idleThreshold < 0)
                {
                    idleThreshold = 9f;
                }
                AI_makePath(LimbsToDevour[0].GlobalPosition);
                //GD.Print("dist to limb: ", AI_navAgent.DistanceToTarget());
                if (AI_navAgent.DistanceToTarget() <= 25f)
                {
                    LimbDevouring = LimbsToDevour[0];
                    EmitSignal(SignalName.DevourLimb, LimbCount);
                    OnBloodDropTimeout();
                    rollAIState(); return;
                }

                timeIdle += lastDeltaTime;

                if (timeIdle > idleThreshold)
                {
                    AI_teleportToLocation(AI_navAgent.TargetPosition);
                }
                break;
        }
    }
    private void handleMainAIState() //Run every frame to handle the main state and handle the current input of the AI Player
	{
        //GD.Print("State: ", CurrentMainState, currentSubState);
        
        //Condition for toy with timer
        if (!CanMove)
        {
            return; //Don't do things if the no move flag is on
        }
        else
        {
            if (newMove)
            {
                rollAIState(); //Roll the AI when coming out of a no move state
                newMove = false;
            }
        }

        if (timeInMainBehavior >= 10f && timeInSubBehavior >= 10f) //Max time in any one phase without re-rolling
        {
            if (!AI_isVisible(Position)) {
                if (LimbsToDevour.Count > 0)
                {
                    AI_teleportToLocation(LimbsToDevour[0].Position);
                    changeMainState(AI_MAIN_BEHAVIOR_STATE.FIND_LIMB, (int)AI_SUB_FIND_LIMB_STATE.MOVE_TO_LIMB);
                }
                else 
                {
                    int teleportAt = 0;
                    while (Position.DistanceTo(ProtagRef.Position) > 260 && DistanceOfNavToPlayer() > 400)
                    {
                        //AI_teleportToLocation(StuckTeleport);
                        AI_teleportToLocation(ProtagRef.Position + (Global.GetRandomDirection().Normalized() * 250));
                        if (teleportAt > 10)
                        {
                            break;
                        }
                        teleportAt++;
                    }
                    GD.Print("teleported to ", Position, " after ", teleportAt, " attempts");
                    rollAIState();
                }
            }
        }

        if (LimbsToDevour.Count > 0 && (Position - LimbsToDevour[0].Position).Length() > 20f)
        {
            changeMainState(AI_MAIN_BEHAVIOR_STATE.FIND_LIMB, (int)AI_SUB_FIND_LIMB_STATE.MOVE_TO_LIMB);
        }

        if (ProtagRef.Healing)
        {
            if (Rnd.NextDouble() < 0.25 && !_checkedPounce)
            {
                if (!AI_isVisible(Position))
                {
                    //int teleportAt = 0;
                    //while (Position.DistanceTo(ProtagRef.Position) > 260)
                    //{
                    //    //AI_teleportToLocation(StuckTeleport);
                    //    AI_teleportToLocation(ProtagRef.Position + (Global.GetRandomDirection().Normalized() * 250));
                    //    if (teleportAt > 100)
                    //    {
                    //        break;
                    //    }
                    //    teleportAt++;
                    //}
                }
                changeMainState(AI_MAIN_BEHAVIOR_STATE.POUNCE, (int)AI_SUB_POUNCE_STATE.ASSUME_POSITION);
                _checkedPounce = true;
            }
            else if (Rnd.NextDouble() < 0.3f && !_checkedPounce)
            {
                changeMainState(AI_MAIN_BEHAVIOR_STATE.TOY_WITH, (int)AI_SUB_TOY_WITH_STATE.BE_IN_FRONT_OF);
                _checkedPounce = true;
            }
            else
            {
                _checkedPounce = true;
            }
        }
        else { _checkedPounce = false; }


        //Toy with Code
        if (!toy)
        {
            toyWithTimer += lastDeltaTime;
        }

        if (nextToyWithTime < toyWithTimer)
        {
            //Store States
            if ((Global.CandleGroupsCompleted > 3 || ProtagRef.LimbCount == 1))
            {
                nextToyWithTime = randInRange(3, 7); //make next timer
            } else
            {
                nextToyWithTime = randInRange(10, 20); ; //make next timer
            }

            toyWithTimer = 0.0;// reset timer

            storeMainState = CurrentMainState;
            storeSubState = currentSubState;

            //Change to a toy state
            changeMainState(AI_MAIN_BEHAVIOR_STATE.TOY_WITH, Rnd.Next(0,3));
        }
        switch (CurrentMainState)
        {
            case AI_MAIN_BEHAVIOR_STATE.CHASE:
                AI_chaseState();
                break;

            case AI_MAIN_BEHAVIOR_STATE.GUARD:
                AI_guardState();
                break;

            case AI_MAIN_BEHAVIOR_STATE.IMPEDE:
                AI_impedeState();
                break;

            case AI_MAIN_BEHAVIOR_STATE.POUNCE:
                AI_pounceState();
                break;

            case AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY:
                AI_standThereMenicinglyState();
                break;

            case AI_MAIN_BEHAVIOR_STATE.TOY_WITH:
                AI_toyWithState();
                return; //Return instead to not update last states

            case AI_MAIN_BEHAVIOR_STATE.WANDER:
                AI_wanderState();
                break;
            case AI_MAIN_BEHAVIOR_STATE.FIND_LIMB:
                AI_FindLimbState();
                break;
            default:
                //DO AN ERROR THING
                break;

        }

        //Tick times and states
        timeInMainBehavior += lastDeltaTime;
        timeInSubBehavior += lastDeltaTime;
        lastMainState = CurrentMainState;
        lastSubState = (int)currentSubState;
        //}
    }
    #endregion
    #endregion

    private void rollAIState()
    {
        AI_MAIN_BEHAVIOR_STATE main = (AI_MAIN_BEHAVIOR_STATE)Rnd.Next(0, 7);
        if (main == AI_MAIN_BEHAVIOR_STATE.POUNCE) { 
            if (Rnd.NextDouble() < 0.5)
            {
                rollAIState(); // Pounce rare
            }
        }
        int subState = (int)main == 2 ? Rnd.Next(0,3) : 0; //Randomize if toy with
        if (main == AI_MAIN_BEHAVIOR_STATE.TOY_WITH) { lastMainState = main; }
        changeMainState(main, subState);
    }

    private void changeMainState(AI_MAIN_BEHAVIOR_STATE newState, int newSubState)
    {
        //default the idle and moving trackers when changing states
        timeIdle = 0.0;
        idleThreshold = -1.0;
        timeMoving = 0;
        movingThreshold = -1.0;

        CurrentMainState = newState;
        timeInMainBehavior = 0.0;
        currentSubState = newSubState;
        timeInSubBehavior = 0.0;

        toy = (CurrentMainState == AI_MAIN_BEHAVIOR_STATE.TOY_WITH); //Set toy if this is a swap to toy_with state
        if (toy) { _toyDone = false; }
    }

    private void changeSubState(int newState)
    {
        //default the idle and moving trackers when changing states
        timeIdle = 0.0;
        idleThreshold = -1.0;
        timeMoving = 0;
        movingThreshold = -1.0;

        currentSubState = newState;
        timeInSubBehavior = 0.0;
    }

    //Find if Point is visible
    private bool AI_isVisible(Vector2 position)
    {
        return _visibleOnScreenNotifier.IsOnScreen();
        Rect2 view = GetViewportRect();
        return view.HasPoint(position); 
    }

    //Teleport Function
    private void AI_teleportToLocation(Vector2 position)
	{
        //Just blink the AI to target location
        //Check if point is in a nav mesh
        Array<Rid> rIDs = NavigationServer2D.GetMaps();
        foreach (Rid rID in rIDs)
        {
            bool isNavMesh = NavigationServer2D.MapGetClosestPoint(rID, position).DistanceTo(position) <= float.Epsilon; //if point is in a nav region, its distance should be 0.0
            if (isNavMesh)
            {
                Position = position;
                return;
            }
        }
    }

    //Handle Pathfinding, make sure to check if location is in a valid nav mesh
    private void AI_makePath(Vector2 position)
    {
        if (!_calcPath) { return; }
        //Check if point is in a nav mesh
        Array<Rid> rIDs = NavigationServer2D.GetMaps();
        foreach (Rid rID in rIDs)
        {
            bool isNavMesh = NavigationServer2D.MapGetClosestPoint(rID, position).DistanceTo(position) <= float.Epsilon; //if point is in a nav region, its distance should be 0.0
            if (isNavMesh)
            {
                AI_navAgent.TargetPosition = position; //Allow for path to be set if it is on a nav mesh
                return;
            }
        }
        _calcPath = false;
        _pathCalcTimer.Start();
    }

    private void AI_makeClosePath(Vector2 position)
    {
        if (!_calcPath) { return; }
        //Check if point is in a nav mesh
        Array<Rid> rIDs = NavigationServer2D.GetMaps();
        foreach (Rid rID in rIDs)
        {
            bool isNavMesh = NavigationServer2D.MapGetClosestPoint(rID, position).DistanceTo(position) <= 25.0f; //if point is in a nav region, its distance should be less than 5.0
            if (isNavMesh)
            {
                AI_navAgent.TargetPosition = NavigationServer2D.MapGetClosestPoint(rID, position); //Allow for path to be set if it is on a nav mesh
                return;
            }
        }
        _calcPath = false;
        _pathCalcTimer.Start();
    }

    private void AI_Audio(int audioID)
    {
        GD.Print("Trying to play audio");
        AudioStreamPlayer player = GetParent().GetNode("Sound").GetNode("SFXMonster").GetNode<AudioStreamPlayer>("sfx"+audioID);
        GD.Print(player.Name);
        player.Play();
    }

    private double randInRange(double min, double max)
    {
        if (max < min)
        {
            //Do an Error thing
        }
        return min+((max-min)*Rnd.NextDouble());
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        base._Ready();
        LimbCount = 0;
        ProtagRef = Global.Player;
        ProtagRef.HealingStateChange += OnHealingStateChange;
        AI_navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");

        _visibleOnScreenNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        _visibleOnScreenNotifier.ScreenExited += OnScreenExited;
        _visibleOnScreenNotifier.ScreenEntered += OnScreenEntered;
        _screenExitedTimer = GetNode<Timer>("ScreenExitedTimer");
        _screenExitedTimer.Timeout += PostScreenExitCheck;

        _pathCalcTimer = GetNode<Timer>("PathCalcTimer");
        _pathCalcTimer.Timeout += OnPathTimeout;
        HitboxComponent.HurtboxEntered += OnHurtboxEntered;
        ProtagRef.LimbDetached += OnLimbDetached;

        _bloodDropTimer = GetNode<Timer>("BloodDropTimer");
        _bloodDropTimer.Timeout += OnBloodDropTimeout;

        _toyWithMonster = GetNode<AnimatedSprite2D>("ToyWithMonster");
        _toyWithMonster.Hide();

        InitStateMachine(); 
        
        //changeMainState(AI_MAIN_BEHAVIOR_STATE.IMPEDE, (int)AI_SUB_IMPEDE_STATE.GET_CLOSE);

        changeMainState(AI_MAIN_BEHAVIOR_STATE.CHASE, (int)AI_SUB_CHASE_STATE.PURSUE);
        //REMEMBER TO DEFINE NAV AGENT AND VIEWPORT REF!!!
    }
    public void SetPlayerInfo()
    {
        ProtagRef = Global.Player;
        ProtagRef.HealingStateChange += OnHealingStateChange;
        ProtagRef.LimbDetached += OnLimbDetached;
    }
    public void OnBloodDropTimeout()
    {
        var bloodTrail = _bloodTrail.Instantiate<BloodTrail>();
        bloodTrail.GlobalPosition = GlobalPosition;
        bloodTrail.Modulate = _bloodColor;
        bloodTrail.BloodColor = _bloodColor;
        Global.MainScene.CallDeferred(MainScene.MethodName.AddChild, bloodTrail);

        float newBloodTime = 0.0f;
        var bloodD = Global.Rnd.NextDouble();
        switch (LimbCount)
        {
            case 3:
                newBloodTime = ((float)bloodD * (5f - 2f) + 2f); //Generates double within a range
                break;
            case 2:
                newBloodTime = ((float)bloodD * (4f - 1.5f) + 1.5f); //Generates double within a range
                break;
            case 1:
                newBloodTime = ((float)bloodD * (3f - 1f) + 1f); //Generates double within a range
                break;
            default: return;
        }
        _bloodDropTimer.Start(newBloodTime);
    }
    private void OnScreenEntered()
    {
        CurrentSpeed = _walkSpeed;
        _screenExitedTimer.Stop();
    }

    private void OnScreenExited()
    {
        if (_screenExitedTimer.TimeLeft > 0) { return; }
        _preExitScreenDist = DistanceOfNavToPlayer();
        _screenExitedTimer.Start(10.0f);
    }
    private void PostScreenExitCheck()
    {
        if (!_visibleOnScreenNotifier.IsOnScreen())
        {
            var currDistToPlayer = DistanceOfNavToPlayer();
            if (LimbsToDevour.Count > 0)
            {
                AI_teleportToLocation(LimbsToDevour[0].Position);
                changeMainState(AI_MAIN_BEHAVIOR_STATE.FIND_LIMB, (int)AI_SUB_FIND_LIMB_STATE.MOVE_TO_LIMB);
                GD.Print("teleporting to limb, taking too long...");
            }
            else if (currDistToPlayer < _preExitScreenDist || currDistToPlayer < 350) //getting closer
            {
                GD.Print("new dist of ", currDistToPlayer, " shorter than 350 or prev dist of ", _preExitScreenDist, ", so not teleporting.");
                OnScreenExited();
            }
            else
            {
                GD.Print("new dist of ", currDistToPlayer, " longer than 350 and prev dist of ", _preExitScreenDist, ", so teleporting.");
                int teleportAt = 0;
                while (Position.DistanceTo(ProtagRef.Position) > 260 && DistanceOfNavToPlayer() > 400)
                {
                    //AI_teleportToLocation(StuckTeleport);
                    AI_teleportToLocation(ProtagRef.Position + (Global.GetRandomDirection().Normalized() * 250));
                    if (teleportAt > 15)
                    {
                        break;
                    }
                    teleportAt++;
                }
                //var origPos = Position;
                ////while (Position.DistanceTo(ProtagRef.Position) > 260)
                //while (currDistToPlayer < DistanceOfNavToPlayer())
                //{
                //    //AI_teleportToLocation(StuckTeleport);
                //    AI_teleportToLocation(ProtagRef.Position + (Global.GetRandomDirection().Normalized() * 250));
                //    teleportAt++;
                //    if (teleportAt > 10)
                //    {
                //        break;
                //    }
                //    GD.Print("curr dist to player: ", currDistToPlayer, "\nafter teleport: ", DistanceOfNavToPlayer());
                //}
                GD.Print("teleported to ", Position, " after ", teleportAt, " attempts");
                changeMainState(AI_MAIN_BEHAVIOR_STATE.CHASE, (int)AI_SUB_CHASE_STATE.PURSUE);
            }
            if (!_visibleOnScreenNotifier.IsOnScreen())
            {
                OnScreenExited();
            }
        }
    }

    private void OnHealingStateChange(bool isHealing)
    {
        _checkedPounce = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (CanMove)
        {
            if (GetDesiredDirection().X < -0.1 && AI_navAgent.DistanceToTarget() > 15f)
            {
                AnimSprite.FlipH = true;
            }
            else if (GetDesiredDirection().X > 0.1 && AI_navAgent.DistanceToTarget() > 15f)
            {
                AnimSprite.FlipH = false;
            }
        }
        _stateMachine.ProcessFrame((float)delta);
        //GD.Print("Ai is visible: ", AI_isVisible(Position));
        
        

        //AI_navAgent.TargetPosition = ProtagRef.Position; //Nav to Player
        //AI_makePath(ProtagRef.Position);
        //GD.Print("new target pos: ", AI_navAgent.TargetPosition);
        //if (AI_navAgent.DistanceToTarget() >= 100f)
        //{
        //    CurrentSpeed = 5000f;
        //}
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        lastDeltaTime = delta;
        //GD.Print("current AI state: ", currentMainState, " \ntargetpos: ", AI_navAgent.TargetPosition);
        handleMainAIState();
        if (DistanceOfNavToPlayer() > 350)
        {
            CurrentSpeed = _pounceSpeed;
            //GD.Print("dist ", DistanceOfNavToPlayer(), ", so chasing");
        }
        else if (DistanceOfNavToPlayer() > 200)
        {
            CurrentSpeed = _chaseSpeed;
        }
        else if (Global.CandleGroupsCompleted == 4)
        {
            CurrentSpeed = _chaseSpeed;
        }
    }
    private void OnLimbDetached(SeveredLimb severedLimb)
    {
        if (AttackedPlayer || !CanMove)
        {
            
            if (LimbCount == 0)
            {
                CanMove = true;
                newMove = true;
            }

            LimbDevouring = severedLimb;
            //AI_teleportToLocation(severedLimb.GlobalPosition - new Vector2(25, 0));
            EmitSignal(SignalName.DevourLimb, LimbCount);
            OnBloodDropTimeout();
            AttackedPlayer = false;
        }
        else
        {
            LimbsToDevour.Add(severedLimb);
        }
    }
    private void OnHurtboxEntered(HurtboxComponent hurtbox)
    {
        if (hurtbox == HurtboxComponent || hurtbox.Invulnerable)
        {
            return;
        }
        AttackedPlayer = true;
    }
    public override float CalcMovementSpeed()
    {
        switch (LimbCount)
        {
            case 4:
                return 1f;
            case 3:
                return 1f;
            case 2:
                return 0.85f;
            case 1:
                return 0.7f;
            case 0:
                return 2.0f;
            default:
                throw new Exception("LIMB COUNT ERROR");
        }
    }
    public override Vector2 GetDesiredDirection()
    {
        if (InitCutscene)
        {
            return ProtagRef.Position - Position;
        }
        else if (!CanMove) { return Vector2.Zero; }
        //GD.Print("target Pos: ", AI_navAgent.TargetPosition, "\nnextpath:", ToLocal(AI_navAgent.GetNextPathPosition()));
        return ToLocal(AI_navAgent.GetNextPathPosition());
    }
    public override Vector2 GetDesiredDirectionNormalized()
    {
        return GetDesiredDirection().Normalized();
    }

    private float DistanceOfNavToPlayer()
    {
        var currTargetPos = AI_navAgent.TargetPosition;
        AI_navAgent.TargetPosition = ProtagRef.Position;
        var navBuff = AI_navAgent.GetNextPathPosition();
        var navPath = AI_navAgent.GetCurrentNavigationPath();
        if (navPath.Length == 0) { return 0f; }
        var currNavPos = navPath[AI_navAgent.GetCurrentNavigationPathIndex()];
        var currNavIdx = 0;
        var dist = 0.0f;

        foreach (var navPoint in navPath)
        {
            if (currNavIdx <= AI_navAgent.GetCurrentNavigationPathIndex())
            {
                currNavIdx++;
                continue;
            }
            dist += currNavPos.DistanceTo(navPoint);
            currNavPos = navPoint;
        }
        AI_navAgent.TargetPosition = currTargetPos;
        return dist;
    }

    private void ToyWithFlicker(int flashes)
    {
        if (!_flickerDone) { return; }
        _flickerDone = false;
        var shapeFlickerTween = CreateTween();
        for (int i =  0; i < flashes; i++)
        {
            shapeFlickerTween.TweenProperty(_toyWithMonster, "visible", false, 0.0f).SetDelay((float)randInRange(0.1, 0.2));
            shapeFlickerTween.TweenProperty(_toyWithMonster, "position",
                ProtagRef.Position + (Global.GetRandomDirection() * (float)randInRange(70, 125)), 0.0f);
            shapeFlickerTween.TweenProperty(_toyWithMonster, "visible", true, 0.0f).SetDelay((float)randInRange(0.1, 0.3));
        }
        shapeFlickerTween.TweenProperty(_toyWithMonster, "visible", false, 0.0f).SetDelay((float)randInRange(0.1, 0.2));
        shapeFlickerTween.TweenProperty(this, PropertyName._flickerDone.ToString(), true, 0.0f);
        shapeFlickerTween.TweenProperty(this, PropertyName._toyDone.ToString(), true, 0.0f);
    }
    private void ToyWithFront(int flashes)
    {
        if (!_flickerDone) { return; }
        
        _flickerDone = false;
        var shapeFlickerTween = CreateTween();
        for (int i = 0; i < flashes; i++)
        {
            shapeFlickerTween.TweenProperty(_toyWithMonster, "visible", false, 0.0f).SetDelay((float)randInRange(0.05, 0.15));
            shapeFlickerTween.TweenProperty(_toyWithMonster, "position",
                ProtagRef.Position + (ProtagRef.PreZeroInput * (float)randInRange(50, 80)), 0.0f);
            shapeFlickerTween.TweenProperty(_toyWithMonster, "visible", true, 0.0f).SetDelay((float)randInRange(0.05, 0.15));
        }
        shapeFlickerTween.TweenProperty(_toyWithMonster, "visible", false, 0.0f).SetDelay((float)randInRange(0.05, 0.15));
        shapeFlickerTween.TweenProperty(this, PropertyName._flickerDone.ToString(), true, 0.0f);
        shapeFlickerTween.TweenProperty(this, PropertyName._toyDone.ToString(), true, 0.0f);
    }

}
