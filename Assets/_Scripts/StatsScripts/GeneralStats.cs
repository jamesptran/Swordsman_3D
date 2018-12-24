using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// General Stats store the different stats for characters
// Type is MonoBehavior to attach it to players and characters
public class GeneralStats : MonoBehaviour {
	CharacterStats charStats = new CharacterStats();
	AIStats aiStats = new AIStats();
	WeaponStats primaryWStats = new WeaponStats();

	// projectilePrefab stores the projectile for enemies
	[SerializeField] GameObject projectilePrefab;

	Animator _anim;
	int takeDamageTrigger;

	bool canParry = false;
	bool canStagger = true;
	float staggerCooldown = 2.0f;
	float staggerTimer = 0f;

	// These two below may be migrated to AttackAIBehaviour soon, after being modulized
	// Weapon object fires bullet
	[SerializeField] GameObject weaponObject;
	// Aim object aims the laser
	[SerializeField] GameObject aimObject;

	void Start() {
		if (tag != "Player")
			primaryWStats.WeaponProjectile = projectilePrefab;
		_anim = GetComponent<Animator> ();
		takeDamageTrigger = Animator.StringToHash ("TakeDamage");
	}

	void Update() {
		// Set canStagger to true when done
		if (!canStagger) {
			staggerTimer += Time.deltaTime;
		}

		if (staggerTimer > staggerCooldown) {
			staggerTimer = 0f;
			canStagger = true;
		}
	}

	void Die() {
		_anim.SetBool ("Dead", true);
	}
		
	// Depends on the enemy design that the weapon is different

	public CharacterStats GetCharacterStats { get { return charStats; } }

	public AIStats GetAIStats { get { return aiStats; } }

	public WeaponStats GetPrimaryWStats { get { return primaryWStats; } }

	public GameObject GetWeaponObj { get { return weaponObject; } }

	public GameObject GetAimObj { get { return aimObject; } }

	public bool GetPlayerCanParry { get { return canParry; } set { canParry = value; } }

	// ------------------------------------------------------------------------------------------
	// Self functions - functions that affect self by changing states
	// Allow the call of these functions from other objects such as an attack or open parry window
	// ------------------------------------------------------------------------------------------
	public void ApplyDamage(float damage) {
		if (canStagger) {
			_anim.SetTrigger (takeDamageTrigger);
			canStagger = false;
		} else {
			Debug.Log ("Stagger blocked during invul time");
		}

		charStats.TakeDamage (damage);
	}
}
