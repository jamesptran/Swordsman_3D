using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Slows time when bullet enters the sphere
// Serves as a helper for parry bullets
public class InstinctSphere : MonoBehaviour {
	float slowTimeLeft = 0f;

	// Use this for initialization
	void Start () {
		
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Projectile") {
			SlowsTime();
		}
	}

	private void SlowsTime() {
		// Add 1 second to slowTimeLeft to slow time for 2 seconds
		slowTimeLeft = 0.3f;
	}
	
	// Update is called once per frame
	void Update () {
		// If there is time in slowTimeLeft then slow time until it runs out
		if (slowTimeLeft > 0) {
			Time.timeScale = 0.3f;
			Time.fixedDeltaTime = 0.02F * Time.timeScale;
			slowTimeLeft -= Time.deltaTime;
		} else {
			slowTimeLeft = 0;
			Time.timeScale = 1.0f;
			Time.fixedDeltaTime = 0.02F * Time.timeScale;
		}
		
	}
}
