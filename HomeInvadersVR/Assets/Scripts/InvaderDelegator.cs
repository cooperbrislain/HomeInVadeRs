using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvaderDelegator : MonoBehaviour {

    Valuable[] _valuables;

    // Use this for initialization
    void Start () {
        _valuables = Object.FindObjectsOfType<Valuable>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
