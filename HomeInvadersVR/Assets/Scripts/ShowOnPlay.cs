using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOnPlay : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponentInChildren<MeshRenderer>().enabled = true;
	}
	
}
