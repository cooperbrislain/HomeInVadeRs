using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{

	public float ropeLength;

	public GameObject ropeLink;
	// Use this for initialization
	private GameObject prevLink;
	private GameObject newLink;
	
	void Start () {
		for (int i = 0; i < ropeLength * 100; i += 10)
		{
			newLink = Instantiate(ropeLink, new Vector3(0, i * -0.01f, 0), Quaternion.identity, gameObject.transform);
			HingeJoint hinge = newLink.GetComponent(typeof(HingeJoint)) as HingeJoint;
			if (i == 0)
			{
				hinge.connectedBody = gameObject.GetComponent<Rigidbody>();
			} else {	
				hinge.connectedBody = prevLink.GetComponent<Rigidbody>();
			}
			prevLink = newLink;
		}
	
	}
	// Update is called once per frame
	void Update () {
		
	}
}
