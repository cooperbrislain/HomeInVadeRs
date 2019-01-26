using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {

	public bool standupEnabled = true;
	public float standupForce = 10;
	public float fallForce = 5;
	public Rigidbody head;
	public Rigidbody body;

	public float startY = 2;

	void Awake() {
		if (body == null) {
			body = GetComponent<Rigidbody>();
		}
		if (head == null) {
			head = GetComponent<Rigidbody>();
		}
		startY = body.position.y;
	}

	void Update () {
		if (head.position.y < startY) {
			head.AddForce(standupForce * Vector3.up);
		} else {
			head.AddForce(-standupForce * Vector3.up);
		}
	}

	public static Rigidbody GetRigidbody(GameObject obj) {
		var controller = obj.GetComponent<RagdollController>();
		if (controller != null) {
			return controller.body;
		}
		return obj.GetComponent<Rigidbody>();
	}
}
