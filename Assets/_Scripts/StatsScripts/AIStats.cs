using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStats {
	// Stores states and commands

	//public enum State {Finding, Attacking, Patrolling, Retreating, Wandering}
	public enum Command {RunningToDest, WalkingToDest, AttackingDest, Idle, SprintingToDest}

	Command currentCommand = Command.RunningToDest;
	int patrolIndex = 0;

	// List of patrol points
	[SerializeField] List<Vector3> patrolPoints = new List<Vector3> ();

	// Further than this distance, back to patrol
	float maxDistanceFromPatrol = 40.0f;

	// Start walking when 10.0f away from patrol destination else Run to destination
	// To make this look better, patrol points need to be 
	// at most [walkDistanceFromPatrol] away from each other
	float walkDistanceFromPatrol = 10.0f;
	float sprintDistanceFromPatrol = 20.0f;

	// Detect range is the range in which AI will detect enemies and run to them to attack
	// If enemy goes over 2x detectRange they will go back to patrol
	float detectRange = 15.0f;


	private float turnspeed = 120.0f;					// turnspeed for fixing on the moveDest when moving
	private float aimspeed = 500.0f;					// turnspeed for fixing on the player when attacking

	// ------------------------------------------------------------------------------------------
	// Variable executive functions - functions that uses locally defined variables for something
	// ------------------------------------------------------------------------------------------
	public Command GetCommandToPatrolDest(Vector3 currentPosition) {
		if (Vector3.Distance (currentPosition, GetCurrentPatrolPoint) > sprintDistanceFromPatrol) {
			return Command.SprintingToDest;
		} else if (Vector3.Distance (currentPosition, GetCurrentPatrolPoint) > walkDistanceFromPatrol) {
			return Command.RunningToDest;
		} else {
			return Command.WalkingToDest;
		}
	}

	public void InitializePatrolList(Vector3 firstPatrolPoint) {
		patrolPoints.Clear ();
		patrolPoints.Add (firstPatrolPoint);
	}

	// Divide next patrol dest to several patrol points segment so that they are within walk distance
	// These segments will be around 3.0f away from each other
	public void AddNextPatrolDest(Vector3 nextPatrolDest) {
		int lastIndex = patrolPoints.Count - 1;
		Vector3 lastToNext = nextPatrolDest - patrolPoints [lastIndex];
		Vector3 eachVect = lastToNext / (lastToNext.magnitude / 3.0f);
		int max = Mathf.FloorToInt (lastToNext.magnitude / 3.0f);
		for (int i = 0; i < max; i++) {
			patrolPoints.Add (patrolPoints [lastIndex] + eachVect * i);
		}

		if (Vector3.Distance (patrolPoints [patrolPoints.Count - 1], nextPatrolDest) > 0.5f) {
			patrolPoints.Add (nextPatrolDest);
		}
	}

	// This is to connect the last patrol point to the first dividing it to segments
	public void AddEndPatrolSegment() {
		AddNextPatrolDest (patrolPoints [0]);
		// Remove the last point since it is the same as the first point
		patrolPoints.RemoveAt (patrolPoints.Count - 1);
	}

	public bool CheckShouldBackToPatrol(Vector3 currentPosition) {
		return Vector3.Distance (currentPosition, GetCurrentPatrolPoint) > maxDistanceFromPatrol;
	}

	public bool CheckTargetTooFar(Vector3 enemyPosition, Vector3 currentPosition) {
		return Vector3.Distance (enemyPosition, currentPosition) > (detectRange * 2.0f);
	}

	public bool CheckShouldDetectEnemies(Vector3 enemyPosition, Vector3 currentPosition) {
		bool detected = Vector3.Distance (enemyPosition, currentPosition) < detectRange;
		bool closeEnoughFromPatrol = Vector3.Distance (currentPosition, GetCurrentPatrolPoint) < walkDistanceFromPatrol;

		return (detected && closeEnoughFromPatrol);
	}


	// ------------------------------------------------------------------------------------------
	// Variable access functions - functions that provide access to locally defined variables
	// ------------------------------------------------------------------------------------------

	public Command GetCommand { get { return currentCommand; } set { currentCommand = value; } }
	public float GetTurnspeed { get { return turnspeed; } }
	public float GetAimspeed { get { return aimspeed; } }
			
	public Vector3 GetCurrentPatrolPoint {
		get {
			if (patrolPoints.Count == 0) {
				return new Vector3 (0, 0, 0);
			} else {
				return (patrolPoints [patrolIndex]);
			}
		}
	}

	public void MoveToNextPatrolPoint() {
		if (patrolPoints.Count == 0) {
			return;
		}
		patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
	}


}
