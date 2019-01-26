using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys this object when it collides with an object.
/// </summary>
public class DieOnCollide : Collide
{
	public float delay;
	public bool enableRagdoll = true;

	private bool _isDestroying;

	protected override void HandleCollision(Collider collider) {
		GameObject other = collider.gameObject;
		if (!_isDestroying) {
			_isDestroying = true;
			if (delay > 0) {
				AIController controller = GetComponent<AIController>();
				if (controller != null) {
					controller.enabled = false;
				}
				RagdollController ragdoll = GetComponent<RagdollController>();
				if (ragdoll != null) {
					ragdoll.standupEnabled = false;
					if (enableRagdoll) {
						ragdoll.SetRagdollEnabled(true);
					}
				}
			}
			Destroy(gameObject, delay);
		}
	}
}
