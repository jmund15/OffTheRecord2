using Godot;
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
		POUNCE, //Wait outside of vision and leap at player
		GUARD //Actively guard area
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
		PASS_BY_VIEW, //Remain slightly out of view port
		STAND_NEAR_GOAL, //Stand in range of an objective on the map
		NAV_TO_RANDOM_WAYPOINT, //Go towards random waypoint
		WALK_NEAR_WAYPOINT //Move around a bit when at waypoint
	}

	enum AI_SUB_TOY_WITH_STATE
	{
		BE_IN_FRONT_OF, //Teleport to a Waypoint Player is moving towards
		SHAPE_FLICKER, //Change sprite frame to nightmare for a frame or two flicker in in view port
		MAKE_SOUND //Make a noise
	}

	enum AI_SUB_STAND_THERE_MENICINGLY_STATE
	{
		BE_SPOOKY, //Stand menicingly
        MOVE_TO_SPOT //Nav to stand spot
    }

	enum AI_SUB_IMPEDE_STATE
	{
		GET_CLOSE, //Nav to near player
		BLOCK_PATH, //Stand at next node in direction of player movement
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
		CATCH //Move to catch caught player
	}
    #endregion
    #endregion

    #region AI_STATE_FUNCS

    //Current AI State
    private AI_MAIN_BEHAVIOR_STATE currentMainState = AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY; //Current AI State, start as do nothing 
	private int currentSubState = 0; //Current AI Substate, start as do nothing. This is an int so I can cheat and use different enum types. ALWAYS CAST SUB STATE TO INT WHEN CHANGING OR REFERENCING IT

    //Stored AI state for toy_with states to switch to and from to
    private AI_MAIN_BEHAVIOR_STATE storeMainState = AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY;
    private int storeSubState = 0;

    //Time since last toy with occured
    private double toyWithTimer = 0.0; 

    //Timers for behavior logic
	private double timeIdle = 0.0;
    private double timeMoving = 0.0;

    private double timeInMainBehavior = 0.0;
    private double timeInSubBehavior = 0.0;

    //Waypoint of interest
    private int currentWaypointID = -1;
    


    private void AI_chaseState()
	{
		switch (currentSubState) 
		{
			case (int)AI_SUB_CHASE_STATE.LOCATE:
				//Stand there for a bit, reference timeIdle
				break;

            case (int)AI_SUB_CHASE_STATE.PURSUE:
                //Nav to player point directly using nav agent
                break;

            case (int)AI_SUB_CHASE_STATE.CHARGE:
                //Lunge at player's current location
                break;

            case (int)AI_SUB_CHASE_STATE.BLOCKED:
                //AI Can't Nav to player point directly
                break;

            case (int)AI_SUB_CHASE_STATE.JUKED:
                //Did not catch with charge, act confused for a moment, then go
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
                break;

            case (int)AI_SUB_GUARD_STATE.CATCH:
                //Lunge at player's current location, like chase.charge
                break;

        }
    }

    private void AI_impedeState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_IMPEDE_STATE.GET_CLOSE:
                //Nav to general area of player
                break;

            case (int)AI_SUB_IMPEDE_STATE.BLOCK_PATH:
                //Nav to general point in front player
                break;

            case (int)AI_SUB_IMPEDE_STATE.ADJUST:
                //Move in opposite direction of player for a bit. If you want to catch, change to chase.charge
                break;


        }
    }

    private void AI_pounceState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_POUNCE_STATE.ASSUME_POSITION:
                //Stand there out of frame until player is out of healing state
                break;

            case (int)AI_SUB_POUNCE_STATE.YEET:
                //Jump the Player
                break;
        }
    }

    private void AI_standThereMenicinglyState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_STAND_THERE_MENICINGLY_STATE.BE_SPOOKY:
                //Stand there for a bit, reference timeIdle
                break;

            case (int)AI_SUB_STAND_THERE_MENICINGLY_STATE.MOVE_TO_SPOT:
                //Nav to waypoint to stand menicingly
                break;

        }
    }

    private void AI_toyWithState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_TOY_WITH_STATE.BE_IN_FRONT_OF:
                //Teleport to point infront of player offscreen
                break;

            case (int)AI_SUB_TOY_WITH_STATE.SHAPE_FLICKER:
                //Change Sprite for a frame or two while on screen
                break;

            case (int)AI_SUB_TOY_WITH_STATE.MAKE_SOUND:
                //Play Audio clip
                break;


        }
    }

    private void AI_wanderState()
    {
        switch (currentSubState)
        {
            case (int)AI_SUB_WANDER_STATE.STAY_OUT_OF_VIEW:
                //Nav to Closest valid point not in the view port
                break;

            case (int)AI_SUB_WANDER_STATE.PASS_BY_VIEW:
                //Nav to point on edge of player view port
                break;

            case (int)AI_SUB_WANDER_STATE.STAND_NEAR_GOAL:
                //Nav near a goal waypoint and stand near it
                break;

            case (int)AI_SUB_WANDER_STATE.NAV_TO_RANDOM_WAYPOINT:
                //Go to a random way point
                break;

            case (int)AI_SUB_WANDER_STATE.WALK_NEAR_WAYPOINT:
                //Walk in area a distance away from the waypoint
                break;

        }
    }

    private void handleMainAIState() //Run every frame to handle the main state and handle the current input of the AI Player
	{
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
                break;

            case AI_MAIN_BEHAVIOR_STATE.WANDER:
				AI_wanderState();
                break;

			default:
				//DO AN ERROR THING
				break;

        }
    }
    #endregion

    //Teleport Function
	private void AI_teleportToLocation()
	{
		//Just blink the AI to target location
	}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
