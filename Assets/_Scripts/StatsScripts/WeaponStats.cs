using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class WeaponStats {
	// Weapon stats is only applicable to enemies
	float range = 30.0f;

	// attackSpeed determines how long the delay till next hitCount
	// the hitCount delay is default to be 0.4s
	// 100 attackSpeed means 1 attack per second
	int hitCount = 3;
	float attackSpeed = 100f;
	float damagePerHit = 20f;
	//float projectileSpeed = 0f;
	// reloadTime default to 1 second
	float reloadTime = 1f;

	GameObject projectile;


	// ------------------------------------------------------------------------------------------
	// Variable executive functions - functions that uses locally defined variables for something
	// ------------------------------------------------------------------------------------------
	// Only applicable to ranged weapon
	// Return time in second
//	public float CalculateTimeToTarget(Vector3 object1, Vector3 object2) {
//		float distance = Vector3.Distance (object1, object2);
//		if (projectileSpeed == 0) {
//			return 0;
//		} else {
//			return distance / projectileSpeed;
//		}
//	}


	// ------------------------------------------------------------------------------------------
	// Variable access functions - functions that provide access to locally defined variables
	// ------------------------------------------------------------------------------------------
	public float GetAttackDelay { get { return 1.0f / (attackSpeed / 100.0f); } }
	public float GetDamage { get { return damagePerHit; } }
	public float GetRange { get { return range; } }
	public float GetReloadTime { get { return reloadTime; } }
	public int GetHitCount { get { return hitCount; } }
	public GameObject WeaponProjectile {get { return projectile; } set {projectile = value;} }

	// projectile speed only applies to ranged
	// projectile speed of 0 means its a hitscan weapon
	// public float GetProjectileSpeed { get { return projectileSpeed; } }
}
