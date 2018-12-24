using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats {
	float currentHP;
	float maximumHP;
	bool isDead = false;

	// These 3 speeds are for 3 states, running walking and sprinting.
	float runningSpeed;
	float walkingSpeed;
	float sprintingSpeed;
	// attackingMoveSpeed is movement speed while chasing down someone
	float attackingMoveSpeed;

	// Default constructor - put default numbers here
	public CharacterStats() {
		runningSpeed = 0.6f;
		walkingSpeed = 0.2f;
		sprintingSpeed = 1.2f;
		attackingMoveSpeed = 0.8f;

		maximumHP = 100.0f;
		currentHP = 100.0f;
	}

	// Automatically set the other speed based on provided runningSpeed
	// walkingSpeed lower than 0.1f sets animation to idle, so running is always higher than 0.1f
	public CharacterStats(float maxHP, float runSpeed) {
		if (runSpeed / 3.0f < 0.1f) {
			walkingSpeed = 0.2f;
		} else {
			walkingSpeed = runSpeed / 3.0f;
		}

		runningSpeed = runSpeed;
		sprintingSpeed = 2.0f * runSpeed;
		attackingMoveSpeed = 1.35f * runSpeed;

		// Set max_hp and currentHp
		maximumHP = maxHP;
		currentHP = maxHP;
	}

	// ------------------------------------------------------------------------------------------
	// Variable executive functions - functions that uses locally defined variables for something
	// ------------------------------------------------------------------------------------------
	public void TakeDamage(float damage) {
		currentHP -= damage;
		if (currentHP <= 0) {
			Die ();
		}
	}

	void Die() {
		isDead = true;
		Debug.Log ("Character dies");
	}

	// ------------------------------------------------------------------------------------------
	// Variable access functions - functions that provide access to locally defined variables
	// ------------------------------------------------------------------------------------------
	public float GetSprintingSpeed { get { return sprintingSpeed; } }

	public float GetRunningSpeed { get { return runningSpeed; } }

	public float GetWalkingSpeed { get { return walkingSpeed; } }

	public float GetAttackMoveSpeed { get { return attackingMoveSpeed; } }

	public float GetMaximumHP { get { return maximumHP; } }

	public float GetCurrentHP { get { return currentHP; } }

	public bool IsDead { get { return isDead; } }
}