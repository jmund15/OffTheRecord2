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
		BE_IN_FRONT_OF, //Move to a Waypoint Player is moving towards
		SHAPE_FLICKER, //Change sprite frame to nightmare for a frame or two flicker
		//??
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

	private AI_MAIN_BEHAVIOR_STATE currentMainState = AI_MAIN_BEHAVIOR_STATE.STAND_THERE_MENICINGLY; //Current AI State, start as do nothing 
	private int currentSubState = 0; //Current AI Substate, start as do nothing. This is an int so I can cheat and use different enum types. ALWAYS CAST SUB STATE TO INT WHEN CHANGING IT

	//Teleport Function

	private void AI_chaseState()
	{
		//TODO, Handle State
	}

    private void AI_guardState()
    {
        //TODO, Handle State
    }

    private void AI_impedeState()
    {
        //TODO, Handle State
    }

    private void AI_pounceState()
    {
        //TODO, Handle State
    }

    private void AI_standThereMenicinglyState()
    {
        //TODO, Handle State
    }

    private void AI_toyWithState()
    {
        //TODO, Handle State
    }

    private void AI_wanderState()
    {
        //TODO, Handle State
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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
