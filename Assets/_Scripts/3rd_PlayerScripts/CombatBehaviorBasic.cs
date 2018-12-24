using UnityEngine;
using System.Collections;

// AttackBehavior inherits from GenericBehaviour. This class corresponds to attack using melee weapon behaviour.
public class CombatBehaviorBasic : GenericBehaviour {
	public string attackButton = "Fire1";
	public string beginCombatButton = "BeginCombat";
	public string lockButton = "LockTarget";
	public float speedDampTime = 0.1f;				// Default damp time to change the animations based on current speed.

	private int attackTrigger;
	private int attackTrigger2;
	private int engagedBool;
	private int lockedonBool;

	bool isEngaged;
	GeneralStats stats;

	bool isLocked;
	GameObject lockTarget;
	GameObject meleeHitTarget;
	bool shouldTriggerWeapon = false;

	// Start is always called after any Awake functions.
	void Start () {
		// Set up the references.
		attackTrigger = Animator.StringToHash ("Attack1");
		attackTrigger2 = Animator.StringToHash ("Attack2");
		engagedBool = Animator.StringToHash ("Engaged");
		lockedonBool = Animator.StringToHash ("LockedOn");

		stats = GetComponent<GeneralStats> ();
	}

	bool IsPlaying(string animationName) {
		return (behaviourManager.GetAnim.GetNextAnimatorStateInfo (1).IsName (animationName));
	}

	// Update is used to set features regardless the active behaviour.
	void Update () {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		Vector2 inputDir = input.normalized;

		if (Input.GetButtonDown (beginCombatButton)) {
			if (isEngaged) {
				StartCoroutine(ToggleCombatOff());
			} else {
				StartCoroutine(ToggleCombatOn());
			}
		}

		// Can't sprint when in combat mode, sprint turns off combat mode
		canSprint = !isEngaged;

		// Set engaged boolean on the Animator Controller.
		behaviourManager.GetAnim.SetBool (engagedBool, isEngaged);

		if (isEngaged && Input.GetButtonDown (attackButton)) {
			if (IsPlaying("Attack1") || IsPlaying("Attack1Linger"))
				behaviourManager.GetAnim.SetTrigger (attackTrigger2);
			else
				behaviourManager.GetAnim.SetTrigger (attackTrigger);
		}

		// Turn off combat if sprint is pressed
		if (Input.GetButton (behaviourManager.sprintButton)) {
			StartCoroutine(ToggleCombatOff());
		}

		// Lock camera and player towards object
		if (Input.GetButtonDown(lockButton) && isEngaged) {
			if (isLocked) {
				isLocked = false;
			} else {
				// Set lock target as well
				behaviourManager.GetCamScript.SetFocusTarget(GameObject.FindGameObjectWithTag ("Enemy").transform);
				isLocked = true;
			}
		}

		// Update camera internal focus boolean
		behaviourManager.GetCamScript.SetFocus (isLocked);
		behaviourManager.GetAnim.SetBool (lockedonBool, isLocked);

		// Check if attack animation is playing to turn on the weaponTrigger
		if (behaviourManager.GetAnim.GetNextAnimatorStateInfo(1).IsName ("Attack1") 
			|| behaviourManager.GetAnim.GetNextAnimatorStateInfo(1).IsName ("Attack2")) {
			shouldTriggerWeapon = true;
		}
	}

	// Co-rountine to start combat mode with delay.
	private IEnumerator ToggleCombatOn() {
		if (!isEngaged) {
			yield return new WaitForSeconds(0.05f);
			// Combat is not possible.
			if (behaviourManager.GetTempLockStatus(this.behaviourCode) || behaviourManager.IsOverriding(this))
				yield return false;

			// Go in combat.
			else {
				isEngaged = true;
				yield return new WaitForSeconds(0.1f);
				behaviourManager.GetAnim.SetFloat(speedFloat, 0);
				// This state overrides the active one.
				behaviourManager.OverrideWithBehaviour(this);
			}
		}
	}

	// Co-rountine to end combat mode with delay.
	private IEnumerator ToggleCombatOff() {
		isLocked = false;
		if (isEngaged) {
			isEngaged = false;
			yield return new WaitForSeconds(0.1f);
			behaviourManager.RevokeOverridingBehaviour(this);
		}
	}

	// Rotating function taken from MoveBehavior, meant for engaged_free mode
	// Rotate the player to match correct orientation, according to camera and key pressed.
	Vector3 Rotating(float horizontal, float vertical)
	{
		// Get camera forward direction, without vertical component.
		Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);

		// Player is moving on ground, Y component of camera facing is not relevant.
		forward.y = 0.0f;
		forward = forward.normalized;

		// Calculate target direction based on camera forward and direction key.
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		Vector3 targetDirection;
		targetDirection = forward * vertical + right * horizontal;

		// Lerp current direction to calculated target direction.
		if((behaviourManager.IsMoving() && targetDirection != Vector3.zero))
		{
			Quaternion targetRotation = Quaternion.LookRotation (targetDirection);

			Quaternion newRotation = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, targetRotation, behaviourManager.turnSmoothing);
			behaviourManager.GetRigidBody.MoveRotation (newRotation);
			behaviourManager.SetLastDirection(targetDirection);
		}
		// If idle, Ignore current camera facing and consider last moving direction.
		if(!(Mathf.Abs(horizontal) > 0.9 || Mathf.Abs(vertical) > 0.9))
		{
			behaviourManager.Repositioning();
		}

		return targetDirection;
	}

	void MovementManagement(float horizontal, float vertical) {
		if (!isLocked) {
			// Call function that deals with player orientation.
			Rotating (horizontal, vertical);

			// Set proper speed.
			Vector2 dir = new Vector2 (horizontal, vertical);		
			behaviourManager.GetAnim.SetFloat (speedFloat, Vector2.ClampMagnitude (dir, 1f).magnitude, speedDampTime, Time.deltaTime);
		}
	}

	// If locked, rotating is automatic
	public override void LocalFixedUpdate () {
		base.LocalFixedUpdate ();
		MovementManagement (behaviourManager.GetH, behaviourManager.GetV);
	}

	// When weapon enter another's collider
	void OnTriggerEnter(Collider other) {
		if (shouldTriggerWeapon && (other.tag == "Enemy" || other.tag == "DamageCollider")) {
			Debug.Log ("Attack hit enemy");
			// Check if target has already been hit this attack cycle.
			if (other.gameObject != meleeHitTarget) {
				meleeHitTarget = other.gameObject;
				other.gameObject.SendMessage ("ApplyDamage", stats.GetPrimaryWStats.GetDamage, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	// ------------------------------------------------------------------------------------------
	// Animation functions - functions that are called from the animation
	// ------------------------------------------------------------------------------------------

	// Called when attack animation begins
	public void AttackBegins() {
		shouldTriggerWeapon = true;

	}

	// Called when attack animation ends
	public void AttackEnds() {
		shouldTriggerWeapon = false;
	}

	// Same as GeneralStats ApplyDamage function, but this one is purely for player instead of universal
	// Set Parry to false, triggerweapon to false and meleeHitTarget to null.
	// This is to avoid external damage interrupting attack, leading to infinite parry window / weapon damage window
	// ToggleCombatOn as well
	public void ApplyDamage() {
		StartCoroutine (ToggleCombatOn ());
		shouldTriggerWeapon = false;
		meleeHitTarget = null;
	}
}