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
		Rigidbody body = GetRigidbody(other);
		if (body == null) {
			return;
		}
		Vector3 forceVec = (other.position - transform.position).normalized * force;
		forceVec.y += upForce;
		StartCoroutine(DelayedAddForce(body, forceVec));
	}

	private IEnumerator DelayedAddForce(Rigidbody body, Vector3 forceVec) {
		yield return null;
		if (body != null) {
			body.AddForce(forceVec);
		}
	}
}
