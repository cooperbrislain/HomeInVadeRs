using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies a fixed force on collision, transformed by object rotation
/// </summary>
public class ApplyForceCollide : Collide {

	public Vector3 force;

	protected override void HandleCollision(Collision collision) {
		Transform other = collision.transform;
		var forward = other.GetComponent<ForwardCollisions>();
		if (forward != null) {
			other = forward.target.transform;
		}
		Rigidbody body = RagdollController.GetRigidbody(other.gameObject);
		if (body == null) {
			return;
		}
		Vector3 transformedForce = transform.TransformDirection(force);
		body.AddForce(transformedForce);
	}
}
