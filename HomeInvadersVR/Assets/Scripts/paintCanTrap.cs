using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paintCanTrap : MonoBehaviour
{
	public Rigidbody  bucket;
    public GameObject placementLocation;

	private void OnTriggerEnter(Collider other)
	{
        if (other.GetComponentInParent<EnemyAgent>() != null) {
            if (bucket != null)
            {
                bucket.isKinematic = false;
                Destroy(bucket.transform.parent.gameObject, 5);
            }
        }
	}
}
