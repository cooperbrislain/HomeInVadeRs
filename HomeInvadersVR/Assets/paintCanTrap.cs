using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paintCanTrap : MonoBehaviour
{
	public Rigidbody bucket;
	// Use this for initialization
	void Start () {
		
	}
	
	private void OnTriggerEnter(Collider other)
	{
			Debug.Log("entered");
			bucket.isKinematic = false;
	}

	
	// Update is called once per frame
	void Update () {
		
	}
}
