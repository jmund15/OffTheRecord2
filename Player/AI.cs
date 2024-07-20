using Godot;
using Godot.Collections;
using System;



public partial class AI : Player
{
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
        WAIT_FOR_PLAYER, //If you make it to the player, wait until he is ready
		YEET //Do the funny jumpscare
	}

	enum AI_SUB_GUARD_STATE
	{
		MOVE_TO_GOAL, //Move towards player goal
		PATROL_GOAL_AREA, //Move near player goal
	}
    #endregion
    #endregion

    #region AI_STATE_FUNCS

    //Current AI State
    private AI_MAIN_BEHAVIOR_STATE currentMainState = AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY; //Current AI State, start as do nothing 
	private int currentSubState = 0; //Current AI Substate, start as do nothing. This is an int so I can cheat and use different enum types. ALWAYS CAST SUB STATE TO INT WHEN CHANGING OR REFERENCING IT
    
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

    //Nav Agent for AI. REMEMBER TO INITIALIZE THIS IN _ready()!!!
    private NavigationAgent2D AI_navAgent;
    private Player protagRef;
    public bool canMove = false; //Can be changed by the outside world to lock movement
    public bool newMove = false;
    private readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());

    //Waypoint of interest
    private int currentWaypointID = -1;


    //Distance to charge from
    private const double CHARGE_DIST = 0.1; //TODO: Find distance to charge from.


    #region AI_SUB_FUNC
    private void AI_chaseState()
	{
		switch (currentSubState) 
		{
			case (int)AI_SUB_CHASE_STATE.LOCATE:
				//NO INPUT HERE
                if (idleThreshold<0.0) {
                    idleThreshold = randInRange(2.0, 5.0); //set idle time
                }
                timeIdle += lastDeltaTime;
                if (timeIdle>=idleThreshold)
                {
                    changeSubState((int)AI_SUB_CHASE_STATE.PURSUE); //Start to chase
                }
				break;

            case (int)AI_SUB_CHASE_STATE.PURSUE:
                //Nav to player point directly using nav agent
                if (movingThreshold < 0.0)
                {
                    movingThreshold = randInRange(10, 15.0); //set movemax time
                }

                AI_navAgent.TargetPosition= protagRef.Position; //Nav to Player
                //TODO:MOVEMENT
                timeMoving += lastDeltaTime;
                if (timeMoving >= movingThreshold) //Change to locate if not found
                {
                    changeSubState((int)AI_SUB_CHASE_STATE.LOCATE);
                }

                if (this.Position.DistanceTo(protagRef.Position)< CHARGE_DIST)
                {
                    changeSubState((int)AI_SUB_CHASE_STATE.CHARGE); //Swap to charge behavior when close
                }
                break;

            case (int)AI_SUB_CHASE_STATE.CHARGE:
                if (this.Position.DistanceTo(protagRef.Position)<CHARGE_DIST){ //TODO FIND THE DIST NUMBER TO LUNGE
                    //TODO TRIGGER LUNGE
                }
                //Lunge at player's current location
                //Handle success and failure and move to juked
                break;

            case (int)AI_SUB_CHASE_STATE.BLOCKED:
                //AI Can't Nav to player point directly
                //???
                break;

            case (int)AI_SUB_CHASE_STATE.JUKED:
                //Did not catch with charge, act confused for a moment, then go
                if (this.Velocity.Abs().Length() <= float.Epsilon) //Find when stopped
                {
                    if (Rnd.NextDouble() <= 0.5) //Flip Coin
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
        switch (currentSubState)
        {
            case (int)AI_SUB_GUARD_STATE.MOVE_TO_GOAL:
                //Nav to Goal
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

                if (this.Position.DistanceTo(protagRef.Position) < 20)//TODO: FIND SEARCH DISTANCE
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
                //Nav to general area of player
                AI_makePath(protagRef.Position);
                if (this.Position.DistanceTo(protagRef.Position)<=10) //TODO: Find Impede distance
                {
                    changeSubState((int)AI_SUB_IMPEDE_STATE.ADJUST);
                }

                break;


            case (int)AI_SUB_IMPEDE_STATE.ADJUST:
                //Move in mirror of player for a bit. 

                if (movingThreshold < 0)
                {
                    movingThreshold=randInRange(3.0, 10.0); // Be in way for 3-10 seconds
                }

                //protag ref input = AI input

                timeMoving += lastDeltaTime;

                if (timeMoving> movingThreshold)
                {
                    if (Rnd.NextDouble() < 0.40)
                    {
                        if (Rnd.NextDouble() < 0.4) {
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
                //Stand there out of frame until player is out of healing state
                AI_makePath(protagRef.Position);

                if(!(this.Position.DistanceTo(protagRef.Position)<= 30)) //TODO: FIND OUT OF VIEW DISTANCE FOR PLAYER
                {
                    //MOVE TOWARDS HEALING PLAYER
                }
                if(true/*protag healing state here*/)
                {
                    changeSubState((int)AI_SUB_POUNCE_STATE.YEET);
                }

                break;

            case (int)AI_SUB_POUNCE_STATE.YEET:
                //Jump the Player

                //Do the Lunge Thing
                break;
        }
    }

    private void AI_standThereMenicinglyState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_STAND_THERE_MENICINGLY_STATE.BE_SPOOKY:
                //Stand there for a bit, reference timeIdle
                if (idleThreshold < 0)
                {
                    idleThreshold = randInRange(5.0, 15.0);
                }

                timeIdle += lastDeltaTime;

                if (timeIdle > idleThreshold)
                {
                    rollAIState();
                }

                break;

        }
    }

    private void AI_toyWithState()
    {
        bool toyDone = false;
        switch (currentSubState)
        {
            case (int)AI_SUB_TOY_WITH_STATE.BE_IN_FRONT_OF:
                //Teleport to point infront of player offscreen
                toyDone = true; //Set ToyDone since effect is good
                break;

            case (int)AI_SUB_TOY_WITH_STATE.SHAPE_FLICKER:
                //Change Sprite for a frame or two while on screen
                toyDone = true; //Set ToyDone since effect is good
                break;

            case (int)AI_SUB_TOY_WITH_STATE.MAKE_SOUND:
                //Play Audio clip
                toyDone = true; //Set ToyDone since effect is good
                break;
        }
        if (toyDone) 
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
                //Nav to Closest valid point not in the view port

                if (idleThreshold < 0)
                {
                    idleThreshold = randInRange(10.0, 15.0);
                    Vector2 posIndex = this.Position;
                    while (AI_isVisible(posIndex))
                    { //Generate a random, not visible position
                        posIndex += new Vector2((float)randInRange(1.0, 50.0) * (float)Rnd.Next(-1, 2), (float)randInRange(1.0, 50.0) * (float)Rnd.Next(-1, 2));
                    }
                    AI_makeClosePath(posIndex);
                }
                
                timeIdle += lastDeltaTime;

                if (timeIdle > idleThreshold)
                {
                    rollAIState();
                }

                break;


        }
    }

    private void handleMainAIState() //Run every frame to handle the main state and handle the current input of the AI Player
	{
        //Condition for toy with timer
        if (!canMove)
        {
            return; //Don't do things if the no move flag is on
        }
        else
        {
            if (newMove)
            {
                rollAIState(); //Roll the AI when coming out of a no move state
            }
        }

        if (timeInMainBehavior >= 25.0) //Max time in any one phase without re-rolling
        {
            rollAIState();
        }

        //TODO: Add Pounce Roll
        if(false/*Player entering heal state here*/)
        {
            if (Rnd.NextDouble() < 0.1)
            {
                changeMainState(AI_MAIN_BEHAVIOR_STATE.POUNCE, (int)AI_SUB_POUNCE_STATE.WAIT_FOR_PLAYER);
            }
        }


        //Toy with Code
        if (!toy)
        {
            toyWithTimer += lastDeltaTime;
        }

        if (nextToyWithTime < toyWithTimer)
        {
            //Store States
            nextToyWithTime = randInRange(20.0, 30.0); ; //make next timer
            toyWithTimer = 0.0;// reset timer

            storeMainState = currentMainState;
            storeSubState = currentSubState;

            //Change to a toy state
            changeMainState(AI_MAIN_BEHAVIOR_STATE.TOY_WITH, (int)Rnd.Next(0, Enum.GetNames(typeof(AI_SUB_TOY_WITH_STATE)).Length));

        }
            switch (currentMainState)
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

                default:
                    //DO AN ERROR THING
                    break;

            }

        //Tick times and states
        timeInMainBehavior += lastDeltaTime;
        timeInSubBehavior += lastDeltaTime;
        lastMainState = currentMainState;
        lastSubState = (int)currentSubState;
        //}
    }
    #endregion
    #endregion

    private void rollAIState()
    {
        AI_MAIN_BEHAVIOR_STATE main = (AI_MAIN_BEHAVIOR_STATE)Rnd.Next(0, 6);
        int subState = (int)main == 2 ? Rnd.Next(0, 3) : 0; //Randomize if toy with
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

        currentMainState = newState;
        timeInMainBehavior = 0.0;
        currentSubState = newSubState;
        timeInSubBehavior = 0.0;

        toy = (currentMainState == AI_MAIN_BEHAVIOR_STATE.TOY_WITH); //Set toy if this is a swap to toy_with state
        

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
                //Do Teleport if point is on a nav mesh
                return;
            }
        }
    }

    //Handle Pathfinding, make sure to check if location is in a valid nav mesh
    private void AI_makePath(Vector2 position)
    {
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
        
    }

    private void AI_makeClosePath(Vector2 position)
    {
        //Check if point is in a nav mesh
        Array<Rid> rIDs = NavigationServer2D.GetMaps();
        foreach (Rid rID in rIDs)
        {
            bool isNavMesh = NavigationServer2D.MapGetClosestPoint(rID, position).DistanceTo(position) <= 5.0f; //if point is in a nav region, its distance should be less than 5.0
            if (isNavMesh)
            {
                AI_navAgent.TargetPosition = NavigationServer2D.MapGetClosestPoint(rID, position); //Allow for path to be set if it is on a nav mesh
                return;
            }
        }

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
        //REMEMBER TO DEFINE NAV AGENT AND VIEWPORT REF!!!
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        lastDeltaTime=delta;
        handleMainAIState();
	}
}
