using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.AI;

/// <summary>a
/// Enables the list of objects when VR is enabled (or vice versa)
/// </summary>
public class VREnabled : MonoBehaviour {
	public bool enabledWithVR = true;
	public GameObject[] objects;

	void Update() {
		bool shouldEnable = XRDevice.isPresent;
		if (!enabledWithVR) {
			shouldEnable = !shouldEnable;
		}
		for (int i = 0; i < objects.Length; ++i) {
			if (objects[i].activeSelf != shouldEnable) {
				objects[i].SetActive(shouldEnable);
			}
		}
	}
}
