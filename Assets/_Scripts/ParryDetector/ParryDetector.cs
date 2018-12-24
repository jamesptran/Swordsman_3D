using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores the method for the ParryDetector to stop bullets midair when the player is parrying (attacking)
public class ParryDetector : MonoBehaviour {
	// Use this for initialization

	void Start () {
	}

	void OnTriggerEnter (Collider col) {
		GameObject projectile = col.gameObject;
		if (projectile.tag == "Projectile") {
			if (transform.parent.GetComponent<GeneralStats> ().GetPlayerCanParry) {
				Debug.Log ("Parry success!!");
				Destroy (projectile);
			}
		}
	}
}
