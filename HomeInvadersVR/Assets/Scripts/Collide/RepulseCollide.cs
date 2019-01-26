using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies a repulsive force (away) on collision, transformed by object rotation
/// </summary>
public class RepulseCollide : Collide {

	public float force;
	public float upForce;

	protected override void HandleCollision(Collision collision) {
		Transform other = collision.transform;
		Rigidbody body = other.GetComponent<Rigidbody>();
		if (body == null) {
			return;
		}
		Vector3 forceVec = (other.position - transform.position).normalized * force;
		forceVec.y += upForce;
		body.AddForce(forceVec);
	}
}
