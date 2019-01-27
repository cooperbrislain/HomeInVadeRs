using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>a
/// Loads game after any input
/// </summary>
public class StartGameOnInput : MonoBehaviour {
	public string sceneToLoad = "TeleportOnNavMesh";

	void Update() {
		if (Input.anyKeyDown) {
			SceneManager.LoadScene(sceneToLoad);
            SceneManager.LoadScene("Test", LoadSceneMode.Additive);
            SceneManager.LoadScene("TestRoom", LoadSceneMode.Additive);

        }
	}
}
