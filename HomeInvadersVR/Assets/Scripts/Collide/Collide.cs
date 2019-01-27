using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for collisions.
/// </summary>
public class Collide : MonoBehaviour {

	public LayerMask requiredLayer;

	protected virtual void OnCollisionEnter(Collision collision) {
		GameObject other = collision.gameObject;
		if (requiredLayer.value == 0 || (requiredLayer.value & (1 << other.layer)) > 0) {
			HandleCollision(collision);
			HandleCollision(collision.collider);
		}
	}

	protected virtual void OnTriggerEnter(Collider collider) {
		GameObject other = collider.gameObject;
		if (requiredLayer.value == 0 || (requiredLayer.value & (1 << other.layer)) > 0) {
			HandleCollision(collider);
		}
	}

	protected virtual void HandleCollision(Collision collision) {
	}

	protected virtual void HandleCollision(Collider collider) {
	}

	protected Rigidbody GetRigidbody(Transform other) {
		var forward = other.GetComponent<ForwardCollisions>();
		if (forward != null) {
			other = forward.target.transform;
		}
		Rigidbody body = RagdollController.GetRigidbody(other.gameObject);
		return body;
	}
}
