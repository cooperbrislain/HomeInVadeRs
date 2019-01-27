#pragma warning disable

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace AX
{

	public class AXRuntimeHandleBehavior : MonoBehaviour
	{

		public Color normalColor = Color.gray;
		public Color highlightColor = Color.white;
		public Color selectedColor = Color.red;


		public AXParameter P_H;
		public AXParameter P_V;

		public Plane drawingSurface;

		bool mouseIsDown;
		Vector3 mouseDownDiff;

		Vector3 mouseDownPosition;
		Vector3 lookV;

		Matrix4x4 context = Matrix4x4.identity;


		// Handle
		public string handleGuid;

		[System.NonSerialized]
		public AXHandle handle;

		float han_x;
		float han_y;
		float han_z;

		// Use this for initialization
		void Start ()
		{
			
		}
		
		// Update is called once per frame
		void Update ()
		{


			if (!Application.isPlaying)
				return;


			bool byPlane = true;


			if (handle == null)
				return;

			AXParametricObject parametricObject = handle.parametricObject;



			// GET POSITION

			if (parametricObject.is2D ()) {
				context = parametricObject.model.transform.localToWorldMatrix * parametricObject.worldDisplayMatrix;

				if (parametricObject.generator.hasOutputsConnected () || parametricObject.is2D ())
					context *= parametricObject.generator.localMatrix.inverse;
				else
					context *= parametricObject.getAxisRotationMatrix ().inverse * parametricObject.generator.localMatrix.inverse * parametricObject.getAxisRotationMatrix ();

			} else {
				// GROUPER MATRIX NOT WORKING....
				context = parametricObject.model.transform.localToWorldMatrix * parametricObject.generator.parametricObject.worldDisplayMatrix * (parametricObject.getAxisRotationMatrix () * parametricObject.generator.getLocalAlignMatrix ()).inverse * parametricObject.generator.localMatrix.inverse;

			}


			// position handle by parmeters.
			positionHandleGameObject ();



			double h_diff = 0;
			double v_diff = 0;


			if (mouseIsDown) {
		
				if (Input.touchCount == 1) {
					// touch input - works better with deltaPosition
					var touch = Input.GetTouch (0);
					//var dx = touch.deltaPosition.x;
					h_diff = (100.0 / Screen.width) * touch.deltaPosition.x;
					v_diff = (100.0 / Screen.width) * touch.deltaPosition.y;		
				} else {
					// 0 touches: must be mouse input
					h_diff = (5000 / Screen.width) * Input.GetAxis ("Mouse X");
					v_diff = (5000 / Screen.width) * Input.GetAxis ("Mouse Y");		
				}
				h_diff /= 5;
				v_diff /= 5;
				//Debug.Log(h_diff +", " + v_diff);

		


				//using plane
				if (byPlane) {
					establishDrawingSurface ();
					//Vector3 prevPosition = transform.position;
				}



				// BASED ON PLANE
				Vector3 world_pos = transform.position;

				if (byPlane) {
					Vector3 hit_position3D = sampleHitPoint ();
					world_pos = hit_position3D - mouseDownDiff;
					transform.position = world_pos;
				}



				Vector3 localPosition = context.inverse.MultiplyPoint3x4 (world_pos);



				//transform.position = world_pos;

				//if (transform.position != prevPosition)
				if (h_diff != 0 || v_diff != 0) {
					//Debug.Log("moved");





					// Determine the orientation of the camera


					//The normal OperatingSystemFamily the handle plane
					//Debug.Log (lookV);

					float threshold = .707f;

					double diffX = -v_diff;
					double diffZ = h_diff;

					// This logic works with a normal vector

					if (!byPlane) {

						if (lookV.x > 0 && lookV.z < 0) {
							diffX = (1 - lookV.x) * h_diff - lookV.x * v_diff;
							diffZ = (1 + lookV.z) * h_diff - lookV.z * v_diff;
						} else if (lookV.x > 0 && lookV.z > 0) {
							diffX = -((lookV.x)) * h_diff - lookV.x * v_diff;
							diffZ = (1 + lookV.z) * h_diff - lookV.z * v_diff;
						} else if (lookV.x < 0 && lookV.z > 0) {
							diffX = (1 + lookV.x) * h_diff + lookV.x * v_diff;
							diffZ = (1 - lookV.z) * h_diff + lookV.z * v_diff;


						} else if (lookV.x > 0 && lookV.z > 0) {
							diffX = -((lookV.x)) * h_diff - (lookV.x) * v_diff;
							diffZ = (1 + lookV.z) * h_diff - lookV.z * v_diff;//(-lookV.x + lookV.z) * v_diff;
						} else if (lookV.z > threshold) { // pointing forwards
							diffX = -h_diff;
							diffZ = -v_diff;
						} else if (lookV.z < -threshold) {
							// pointing backwards
							diffX = h_diff;
							diffZ = v_diff;

						} else if (lookV.x > 0) {
							// pointing right
							diffX = -h_diff;
							diffZ = v_diff;

						} else {
							// pointing left
							diffX = v_diff;
							diffZ = -h_diff;

						}
					}






					AXHandle han = handle;


					string hanString = "han_y";
					float posV = localPosition.y;

					if (parametricObject.is3D ()) {
						hanString = "han_z";
						posV = localPosition.z;
					}

					if (byPlane) {
						parametricObject.setVar ("han_x", (localPosition.x));
						if (parametricObject.is3D ()) 
							parametricObject.setVar ("han_z", world_pos.z);
						else
							parametricObject.setVar ("han_y", localPosition.y);

					} else {
						// Relative slide of cursor
						parametricObject.setVar ("han_x", (localPosition.x + (float)diffX));
						parametricObject.setVar (hanString, (posV + (float)diffZ));

					}

					// From plane
//					parametricObject.setVar("han_x", localPosition.x);
//					parametricObject.setVar("han_y", localPosition.y);
//					parametricObject.setVar("han_z", localPosition.z);


					// EACH EXPRESSION
					for (int i = 0; i < han.expressions.Count; i++) {
						if (han.expressions [i] == "")
							continue;

						string expression = Regex.Replace (han.expressions [i], @"\s+", "");

						string paramName = expression.Substring (0, expression.IndexOf ("="));
						string definition = expression.Substring (expression.IndexOf ("=") + 1);
						//Debug.Log (param + " --- " + definition);

						try {	
							if (parametricObject.getParameter (paramName).Type == AXParameter.DataType.Int)
								parametricObject.initiateRipple_setIntValueFromGUIChange (paramName, Mathf.RoundToInt ((float)parametricObject.parseMath (definition)));
							else
								parametricObject.initiateRipple_setFloatValueFromGUIChange (paramName, (float)parametricObject.parseMath (definition));

						} catch (System.Exception e) {
							parametricObject.codeWarning = "10. Handle error: Please check syntax of: \"" + definition + "\" " + e.Message;
						}

					}
				
					parametricObject.model.isAltered ();
					//parametricObject.model.autobuild();


				}



			}

			


		}

		public void positionHandleGameObject ()
		{
			Vector3 pos = handle.getPosition ();


			pos = context.MultiplyPoint3x4 (pos);

//			if (handle.parametricObject.is3D())
//				pos.z /= 2;

			transform.position = pos;//handle.getPosition();

			Matrix4x4 axisRoter = (handle.parametricObject.is2D ()) ? Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (-90, 0, 0), Vector3.one) : Matrix4x4.identity;

			transform.rotation = AXUtilities.QuaternionFromMatrix (context * axisRoter);
		}


		public Vector3 sampleHitPoint ()
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			float rayDistance = 0;

			// Point on Plane
			if (drawingSurface.Raycast (ray, out rayDistance))
				return ray.GetPoint (rayDistance);

			return Vector3.zero;


		}

		public void establishDrawingSurface ()
		{
			Matrix4x4 m = context;

			if (handle.parametricObject.is3D ())
			{
				m *= Matrix4x4.TRS(Vector3.zero,Quaternion.Euler(-90, 0 , 0), Vector2.one);
			}


			Vector3 v1 = m.MultiplyPoint3x4 (Vector3.zero);
			Vector3 v2 = m.MultiplyPoint3x4 (new Vector3 (100, 0, 0));
			Vector3 v3 = m.MultiplyPoint3x4 (new Vector3 (0, 100, 0));
			
			drawingSurface = new Plane (v1, v2, v3);
			//Matrix4x4 = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;// * generator.localMatrix.inverse;

		}

		void OnMouseDown ()
		{
			AXRuntimeControllerBase.runtimeHandleIsDown = true;

			lookV = Camera.main.transform.position - transform.position;

			lookV = Vector3.ProjectOnPlane (lookV, Vector3.up);


			lookV = lookV.normalized;


			//Debug.Log ("DOWN");
			establishDrawingSurface ();
			//Debug.Log(sampleHitPoint () + " -- " + transform.position);
			mouseDownDiff = sampleHitPoint () - transform.position;

			GetComponent<Renderer> ().material.color = selectedColor;

			mouseIsDown = true;
		}

		void OnMouseUp ()
		{
			AXRuntimeControllerBase.runtimeHandleIsDown = false;

			mouseIsDown = false;
			GetComponent<Renderer> ().material.color = normalColor;
			handle.parametricObject.model.autobuild ();
		}

		void OnMouseEnter ()
		{
			if (!mouseIsDown)
				GetComponent<Renderer> ().material.color = highlightColor;
		}

		void OnMouseExit ()
		{
			if (!mouseIsDown)
				GetComponent<Renderer> ().material.color = normalColor;
		}



	}

}
