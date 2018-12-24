using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The basicAIBehaviour that all enemies will have. Is used to translate command to actions and animations
// These only translate command to animations and the Navmesh, 
// thus the extra functionalities need to be implemented and activated through the use of subscribe and register
public class BasicAIBehaviour : GenericAIBehaviour {
	[SerializeField] private int currentBehaviour;                   		// Reference to the current AI behaviour.
	private int defaultBehaviour;                        					// The default behaviour of the AI when any other is not active.
	private Animator anim;                                					// Reference to the Animator component.
	private UnityEngine.AI.NavMeshAgent agent;								// Reference to the NavmeshAgent.
	private GeneralStats stats;												// The general stats of the AI
	private AIStats.Command currentCommand;									// Current command of the AIStats
	private int speedFloat;                      							// Speed parameter on the Animator.
	private int attackTrigger;												// Attack trigger on the Animator.
	private AITag currentTag = AITag.Patrolling;
	private bool isLocked;								  					// If isLocked the currentBehaviour won't be activated

	[SerializeField] private List<GenericAIBehaviour> behaviours;  			// Stores AIBehaviours that are currently active

	private GameObject currentAttkTarget; 									// Current attack target for AI
	private Vector3 currentMoveDest; 										// Current moveDestination for AI

	private float attackCounter;											// Counter to count when the next attack comes

	void Awake() {
		behaviours = new List<GenericAIBehaviour> ();
		anim = GetComponent<Animator> ();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent> ();

		stats = GetComponent<GeneralStats> ();
		currentCommand = stats.GetAIStats.GetCommand;

		speedFloat = Animator.StringToHash("Speed");
		attackTrigger = Animator.StringToHash ("Attack");
	}
	
	void Update () {
		foreach (var activeBehaviour in behaviours) {
			if (activeBehaviour.GetBehaviourCode() == currentBehaviour) {
				// Pass the AIStats to the active, current behaviour
				currentCommand = activeBehaviour.SetCommand (stats);
				// call LocalUpdate for the active, current behaviour
				activeBehaviour.LocalUpdate ();
				// Set the AITag to the active, current behaviour
				currentTag = activeBehaviour.localTag;
			}
		}

		PerformCommand (currentCommand, currentAttkTarget, currentMoveDest);
	}

	// ------------------------------------------------------------------------------------------
	// Access functions - functions that allow access to certain variables
	// ------------------------------------------------------------------------------------------
	public GeneralStats GetStats { get { return stats; } }

	public GameObject GetCurrentAttkTarget { get { return currentAttkTarget; } set { currentAttkTarget = value; } } 							
	public Vector3 GetCurrentMoveDest { get { return currentMoveDest; } set { currentMoveDest = value; } } 
	public AITag GetAITag { get { return currentTag; } }
	public UnityEngine.AI.NavMeshAgent GetAgent { get { return agent; } }


	public void SetCurrentBehaviour(int behaviourCode) {
		currentBehaviour = behaviourCode;
	}
		

	// ------------------------------------------------------------------------------------------
	// AI commands are low level commands that the BasicAIBehaviour can translate to animation
	// The basic AI behaviour will call this every update
	// Other behaviours must modify AIStats.Command based on their behaviour and attack + move dest
	// ------------------------------------------------------------------------------------------
	void PerformCommand(AIStats.Command command, GameObject attackingTarget, Vector3 moveDest) {
		// To use command Run and Sprint, set moveDestination to the position.
		if (currentCommand == AIStats.Command.SprintingToDest) {
			agent.destination = moveDest;
			anim.SetFloat (speedFloat, stats.GetCharacterStats.GetSprintingSpeed);
		}

		if (currentCommand == AIStats.Command.RunningToDest) {
			agent.destination = moveDest;
			anim.SetFloat (speedFloat, stats.GetCharacterStats.GetRunningSpeed);
		}

		if (stats.GetAIStats.GetCommand == AIStats.Command.WalkingToDest) {
			agent.destination = moveDest;
			anim.SetFloat (speedFloat, stats.GetCharacterStats.GetWalkingSpeed);
		}

		// Idle command sits the AI in place.
		if (stats.GetAIStats.GetCommand == AIStats.Command.Idle) {
			agent.destination = moveDest;
			anim.SetFloat (speedFloat, 0.0f);
		}


		// To use command AttackingDest, set attackingTarget to the target.
		// The AttackAIBehaviour will take care of extra work such as laserLine and animation functions
		if (currentCommand == AIStats.Command.AttackingDest) {
			if (attackingTarget == null) {
				Debug.Log ("Target for attack is null");
				return;
			}

			// When attacking, run faster than normal
			if (agent.remainingDistance <= stats.GetPrimaryWStats.GetRange) {
				anim.SetFloat (speedFloat, 0f);
			} else {
				anim.SetFloat (speedFloat, stats.GetCharacterStats.GetAttackMoveSpeed);
			}

			if (attackCounter > stats.GetPrimaryWStats.GetAttackDelay && (agent.remainingDistance <= stats.GetPrimaryWStats.GetRange)) {
				anim.SetTrigger (attackTrigger);

				attackCounter = 0f;
			}
			// Turn the AI ourselves using transform.RotateTowards or LookAt
			agent.angularSpeed = 0.0f;
			transform.LookAt (new Vector3 (attackingTarget.transform.position.x, transform.position.y, attackingTarget.transform.position.z));
			Quaternion direction = Quaternion.LookRotation (attackingTarget.transform.position - transform.position);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, direction, stats.GetAIStats.GetAimspeed);

			moveDest = attackingTarget.transform.position;
		} else {
			agent.angularSpeed = stats.GetAIStats.GetTurnspeed;
		}

		// Attack cooldown ticks regardless of action
		attackCounter += Time.deltaTime;
	}

	// This is to make navMesh use the same velocity as root motion for moving characters around
	void OnAnimatorMove() {
		agent.velocity = (anim.deltaPosition / Time.deltaTime).magnitude * transform.forward;
	}


	// Put a new behaviour on the behaviours watch list.
	public void SubscribeBehaviour(GenericAIBehaviour behaviour) {
		behaviours.Add (behaviour);
	}

	// Set the default AI behaviour.
	public void RegisterDefaultBehaviour(int behaviourCode) {
		if (!isLocked) {
			defaultBehaviour = behaviourCode;
			currentBehaviour = behaviourCode;
		}
	}

	// Attempt to set a custom behaviour as the active one.
	// Always changes from default behaviour to the passed one.
	public void RegisterBehaviour(int behaviourCode) {
		if (!isLocked)
			if (currentBehaviour == defaultBehaviour) {
				currentBehaviour = behaviourCode;
			}
	}

	// Attempt to set a custom behaviour as the active one and lock it.
	// Always changes from default behaviour to the passed one.
	public void RegisterLockBehaviour(int behaviourCode) {
		if (currentBehaviour == defaultBehaviour) {
			isLocked = true;
			currentBehaviour = behaviourCode;
		}
	}

	// Attempt to deactivate a player behaviour and return to the default one.
	// If the behaviour is locked, unlock it
	public void UnregisterBehaviour(int behaviourCode) {
		if (currentBehaviour == behaviourCode) {
			isLocked = false;
			currentBehaviour = defaultBehaviour;
		}
	}
}


// This is the GenericAIBehaviour, all AIBehavior must inherit this
public abstract class GenericAIBehaviour : MonoBehaviour {
	protected BasicAIBehaviour AIManager;     		// Reference to the basic AI behaviour manager.
	protected int behaviourCode;                   // The code that identifies a behaviour.
	public enum AITag { Finding, Attacking, Patrolling, Retreating, Wandering }
	public AITag localTag;

	void Awake() {
		AIManager = GetComponent<BasicAIBehaviour> ();

		// Set the behaviour code based on the inheriting class.
		behaviourCode = this.GetType().GetHashCode();
	}

	// This function is implemented when behaviours need extra work done in Update().
	public virtual void LocalUpdate() { }
	// This function is implemented should the behaviour wants to change the command.
	public virtual AIStats.Command SetCommand (GeneralStats stats) { return AIStats.Command.Idle; }
		
	// Get the behaviour code.
	public int GetBehaviourCode() {
		return behaviourCode;
	}

}