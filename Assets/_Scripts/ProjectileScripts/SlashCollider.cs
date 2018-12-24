using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashCollider : MonoBehaviour {
	public void ApplyDamage() {
		Debug.Log ("Gets called");
		gameObject.SendMessageUpwards ("ApplyDamage");
	}
}
