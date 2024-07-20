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
		MOVE_TO_SPOT, //Nav to stand spot
		BE_SPOOKY //Stand menicingly
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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
