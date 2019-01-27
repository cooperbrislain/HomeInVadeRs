using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple controls
/// </summary>
public class RagdollController : MonoBehaviour {

	public bool standupEnabled = true;
	public float standupForce = 10;
	public float fallForce = 5;
	public Rigidbody head;
	public Rigidbody body;

	public float startY = 2;
	public bool isRagdollEnabled = true;

	private Rigidbody[] _rigidbodies;

	void Awake() {
		if (body == null) {
			body = GetComponent<Rigidbody>();
		}
		if (head == null) {
			head = GetComponent<Rigidbody>();
		}
		startY = body.position.y;
		_rigidbodies = GetComponentsInChildren<Rigidbody>(true);
		if (!isRagdollEnabled) {
			SetRagdollEnabled(false);
		}
	}

	void Update () {
		if (standupEnabled) {
			if (head.position.y < startY) {
				head.AddForce(standupForce * Vector3.up);
			} else {
				head.AddForce(-standupForce * Vector3.up);
			}
		}
	}

	public static Rigidbody GetRigidbody(GameObject obj) {
		var controller = obj.GetComponent<RagdollController>();
		if (controller != null) {
			return controller.body;
		}
		return obj.GetComponent<Rigidbody>();
	}

	public void SetRagdollEnabled(bool enabled) {
		for (int i = 0; i < _rigidbodies.Length; ++i) {
			_rigidbodies[i].isKinematic = !enabled;
		}
		isRagdollEnabled = enabled;
		UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		if (agent != null) {
			agent.enabled = !enabled;
		}
		EnemyAgent enemy = GetComponent<EnemyAgent>();
		if (enemy != null) {
			enemy.enabled = !enabled;
		}
	}
}
