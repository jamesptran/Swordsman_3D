using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores the method for the projectile, 
// such as moving forward by itself, colliding to objects and dealing damage
public class StandardProjectile : MonoBehaviour {
	public WeaponStats weapon;
	Vector3 initialPosition;
	// switchProjectile determines when the bullet should start behaving like a projectile
	// (when touched the InstinctSphere)
	public bool switchProjectile = false;

	// Currently all projectiles have fixed speed
	float fixedSpeed = 700;

	// Use this for initialization
	void Start () {
		initialPosition = transform.position;
	}

	void OnTriggerEnter(Collider col) {
		// layer 2 is ignore raycast. Basically everything that ignore raycast can be goes through by the bullet
		if (col.gameObject.layer != 2) {
			// Only apply damage if tag is "Player"
			if (col.gameObject.tag == "Player") {
				col.gameObject.SendMessage ("ApplyDamage", weapon.GetDamage, SendMessageOptions.DontRequireReceiver);
				Destroy (gameObject);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (Vector3.Distance (initialPosition, transform.position) >= weapon.GetRange) {
			Debug.Log ("Out of range");
			Destroy (gameObject);
		}
	}

	void FixedUpdate() {
		GetComponent<Rigidbody> ().velocity = transform.forward * fixedSpeed * Time.fixedDeltaTime;
	}

	public void ApplyDamage() {
		Destroy (gameObject);
	}
}