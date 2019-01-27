using UnityEngine;
using System.Collections;

/* CURVE_POINT_3D
 *
 * For controlling a Bezier curve. Has Tangents.
 */
 
namespace AXGeometryTools
{


	/// <summary>
	/// CurvePoint3D.

	// A CurvePoint is a point that has extra data associated with it.
	/// The common use is for a point on a Bezier curcve with handles.
	/// </summary>
	[System.Serializable]
	public class CurvePoint3D  {


		public CurvePointType curvePointType;

		public Vector3 		position;
		public Quaternion 	rotation;

	}

}
