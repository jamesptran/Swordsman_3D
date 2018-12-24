using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AttackingAIBehaviour : GenericAIBehaviour {
	[SerializeField] GameObject linePrefab;								// linePrefab stores the line object to instantiate
	LineRenderer laserLine; 											// laserLine is the red line signalling attacking direction

	[SerializeField] Transform bulletSpawnPos;							// bulletSpawnPos is where bullets are spawned
	[SerializeField] Transform aimPos;									// aimPos is where the laserLine originates

	private WeaponStats weaponStats;
	private AIStats aiStats;

	// Use this for initialization
	void Start () {
		localTag = AITag.Attacking;
		AIManager.SubscribeBehaviour (this);

		weaponStats = AIManager.GetStats.GetPrimaryWStats;
		aiStats = AIManager.GetStats.GetAIStats;

		bulletSpawnPos = AIManager.GetStats.GetWeaponObj.transform;
		aimPos = AIManager.GetStats.GetAimObj.transform;

		GameObject newLaser = Instantiate<GameObject> (linePrefab);
		laserLine = newLaser.GetComponent<LineRenderer> ();

		laserLine.startWidth = 0.005f;
		laserLine.endWidth = 0.005f;

		laserLine.positionCount = 2;
		laserLine.SetPosition (0, bulletSpawnPos.position);
		laserLine.SetPosition (1, bulletSpawnPos.position);
		laserLine.startColor = new Color (0, 0, 0, 0);
		laserLine.endColor = new Color (0, 0, 0, 0);
	}

	// Perform checks to register behaviour and setup AttkTarget and MoveDest.
	// Here, when AI is in patrol state, check for enemies within detection range
	void Update () {
		if (AIManager.GetAITag == AITag.Patrolling) {
			if (aiStats.CheckShouldDetectEnemies ((GameObject.FindGameObjectWithTag ("Player").transform.position), transform.position)) {
				// Detect enemy
				// Set target then register current (attack) behaviour
				AIManager.GetCurrentAttkTarget = GameObject.FindGameObjectWithTag ("Player");
				AIManager.GetCurrentMoveDest = AIManager.GetCurrentAttkTarget.transform.position;

				AIManager.RegisterBehaviour (behaviourCode);
			}
		}
	}

	public override AIStats.Command SetCommand (GeneralStats stats) {
		GameObject attackingTarget = AIManager.GetCurrentAttkTarget;
		Vector3 moveDestination = AIManager.GetCurrentMoveDest;

		if (attackingTarget != null) {
			moveDestination = attackingTarget.transform.position;

			// Set back to default behaviour if target is too far from self
			if (aiStats.CheckTargetTooFar (transform.position, moveDestination)) {
				AIManager.UnregisterBehaviour (behaviourCode);
				return (AIStats.Command.Idle);
			}

			// Set back to default behaviour if chasing target too far
			if (aiStats.CheckShouldBackToPatrol(transform.position)) {
				AIManager.UnregisterBehaviour (behaviourCode);
				return (AIStats.Command.Idle);
			}

			return (AIStats.Command.AttackingDest);
		} else {
			// If no target is found, set state to patrolling
			Debug.Log("Target is destroyed");
			AIManager.UnregisterBehaviour (behaviourCode);

			return (AIStats.Command.Idle);
		}
	}
		

	// LocalUpdate is what will be run on update when the behaviour is active, current
	public override void LocalUpdate () {
		Vector3 playerPos = GameObject.FindGameObjectWithTag ("Player").transform.position;
		Rigidbody playerRig = GameObject.FindGameObjectWithTag ("Player").GetComponent<Rigidbody> ();

		// Set target aim towards where the player is going
		Vector3 aimtarget = new Vector3 (playerPos.x, bulletSpawnPos.position.y, playerPos.z) + playerRig.velocity / 10;

		// Set target bullet spawn towards where the player is going, but further away based on sqrt distance to player
		float distanceToPlayer = Mathf.Sqrt(Vector3.Distance(transform.position, playerPos));
		Vector3 target = new Vector3 (playerPos.x, bulletSpawnPos.position.y, playerPos.z) + playerRig.velocity / (5 / distanceToPlayer);
		bulletSpawnPos.LookAt (target);
		aimPos.LookAt (aimtarget);

		// Set the laser endPos using raycast on aimPos
		RaycastHit hit;
		Vector3 endPos;
		if (Physics.Raycast (aimPos.position, aimPos.forward, out hit, weaponStats.GetRange)) {
			endPos = hit.point;
		} else {
			endPos = aimPos.position + aimPos.forward * weaponStats.GetRange;
		}

		laserLine.SetPosition (0, aimPos.position);
		laserLine.SetPosition(1, endPos);
	}

	private IEnumerator SpawnBullet(int count) {
		// Spawn bullet using bulletSpawnPos rotation and position
		if (count > 0) {
			GameObject bullet = Instantiate (weaponStats.WeaponProjectile, bulletSpawnPos.position, AIManager.GetStats.GetWeaponObj.transform.rotation);
			bullet.GetComponent<StandardProjectile> ().weapon = weaponStats;

			yield return new WaitForSeconds (0.2f);
			StartCoroutine (SpawnBullet (count - 1));
		} else {
			laserLine.startColor = new Color (0, 0, 0, 0);
			laserLine.endColor = new Color (0, 0, 0, 0);
		}
	}

	// These events are called from the animator during specific frames of the animation
	// Attack event for primary weapon using raycast
	public void AttackEvent() {
		StartCoroutine (SpawnBullet (weaponStats.GetHitCount));

	}
		
	// Signal players that attack is going to connect soon
	// Only send signal when player is in range and is not blocked
	// Signal ends when attack connects
	public void AttackBegins() {
	}
		
	public void ShouldStartParrying() {
		laserLine.startColor = new Color (1, 0, 0);
		laserLine.endColor = new Color (1, 0, 0);

		Time.timeScale = 1.0f;
	}

	public void ParryIsTooLate() {
		Time.timeScale = 1.0f;
	}

	// Wraps up whatever attack starts
	public void AttackIsInterrupted() {
		
		Time.timeScale = 1.0f;
	}

}
