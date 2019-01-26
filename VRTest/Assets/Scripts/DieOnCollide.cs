using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys this object when it collides with an object.
/// </summary>
public class DieOnCollide : MonoBehaviour
{
	public LayerMask requiredLayer;

	void OnCollisionEnter(Collision collision) {
		GameObject other = collision.gameObject;
		if (requiredLayer.value == 0 || (requiredLayer.value & (1 << other.layer)) > 0) {
			Destroy(gameObject);
		}
	}
}
