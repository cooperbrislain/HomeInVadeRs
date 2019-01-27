using UnityEngine;

#if UNITY_EDITOR  
using UnityEditor;
#endif

using System.Collections;

[ExecuteInEditMode]
public class AXSceneViewBillboard : MonoBehaviour {

	void OnRenderObject () {
		#if UNITY_EDITOR  

		if (SceneView.lastActiveSceneView != null)
		{
			Vector3 targetPos = Camera.current.transform.position;

			#if UNITY_EDITOR  
			targetPos = SceneView.lastActiveSceneView.camera.gameObject.transform.position; //sceneCameras[0].transform.position;
			#endif

			if (Application.isPlaying)
				targetPos = Camera.current.transform.position;

			//Vector3 lookVec = transform.position-targetPos;
			//lookVec.y = 0;
			//transform.LookAt (lookVec, Vector3.up);

			// From http://answers.unity3d.com/questions/36255/lookat-to-only-rotate-on-y-axis-how.html
			float distanceToPlane = Vector3.Dot(Vector3.up, targetPos - transform.position);
			Vector3 pointOnPlane = targetPos - (Vector3.up * distanceToPlane);
 			transform.LookAt(pointOnPlane, Vector3.up);
			transform.Rotate(new Vector3(-90, 90, -90));

		}
		#endif
	}

}
