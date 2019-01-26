using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows mouse drag input to drag objects with the given layer.
/// </summary>
public class ObjectDragger : MonoBehaviour
{
	public LayerMask layer;

	public Transform selectedObject;

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, float.MaxValue, layer.value)) {
				selectedObject = hit.transform;
			} else {
				selectedObject = null;
			}
		} else if (Input.GetMouseButton(0) && selectedObject != null) {
			float hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Plane plane = new Plane(Vector3.up, Vector3.zero);

			if (plane.Raycast(ray, out hit)) {
				Vector3 pos = ray.GetPoint(hit);
				selectedObject.position = pos;
			}
		}
	}
}
