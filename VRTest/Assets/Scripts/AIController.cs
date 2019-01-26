using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls an player or enemy that seeks a target.
/// </summary>
public class AIController : MonoBehaviour
{
	public Transform target;
    public Rigidbody body;
	public float seekForce;

    void Start()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
		Vector3 dir = target.position - transform.position;
		dir.Normalize();
		body.AddForce(dir * seekForce);
    }
}
