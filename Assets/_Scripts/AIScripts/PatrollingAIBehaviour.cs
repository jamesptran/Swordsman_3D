using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingAIBehaviour: GenericAIBehaviour {
	private CharacterStats charStats;
	private AIStats aiStats;

	// Use this for initialization
	void Start () {
		localTag = AITag.Patrolling;
		AIManager.SubscribeBehaviour (this);
		AIManager.RegisterDefaultBehaviour (behaviourCode);


		// Set up 3 different stats
		charStats = AIManager.GetStats.GetCharacterStats;
		aiStats = AIManager.GetStats.GetAIStats;

		// Initiate patrol path, calling Initialize and EndPatrol at start and end
		aiStats.InitializePatrolList (new Vector3(transform.position.x, transform.position.y, transform.position.z));
		aiStats.AddNextPatrolDest(new Vector3(transform.position.x + 15.0f, transform.position.y, transform.position.z));
		aiStats.AddNextPatrolDest(new Vector3(transform.position.x + 15.0f, transform.position.y, transform.position.z + 15.0f));
		aiStats.AddNextPatrolDest(new Vector3(transform.position.x, transform.position.y, transform.position.z + 15.0f));
		aiStats.AddEndPatrolSegment ();
	}
	
	// Perform checks to register behaviour and setup AttkTarget and MoveDest.
	// Here, patrol is the default state so it doesn't do anything
	void Update () {
	}
		
	public override AIStats.Command SetCommand (GeneralStats stats) {
		GameObject attackingTarget = AIManager.GetCurrentAttkTarget;
		Vector3 moveDestination = AIManager.GetCurrentMoveDest;

		// If get to patrol point then move to next patrol

		if (AIManager.GetAgent.remainingDistance < 0.5f) {
			Debug.Log ("Move to next patrol point!");
			aiStats.MoveToNextPatrolPoint ();
			AIManager.GetCurrentMoveDest = aiStats.GetCurrentPatrolPoint;
			Debug.Log (aiStats.GetCurrentPatrolPoint);
		}
			
		// Set run or walk
		return (aiStats.GetCommandToPatrolDest(transform.position));
	}

	public override void LocalUpdate () {
		
	}
}
