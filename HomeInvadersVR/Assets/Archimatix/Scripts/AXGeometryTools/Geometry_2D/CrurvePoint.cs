using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AX;

/* CURVE_POINT
 *
 * For controlling a Bezier curve. Has Tangents.
 */
 
namespace AXGeometryTools
{


	/// <summary>
	/// Curve point.

	// A CurvePoint is a point that has extra data associated with it.
	/// The common use is for a point on a Bezier curcve with handles.
	/// </summary>
	[System.Serializable]
	public class CurvePoint  {

		public string Guid;

		public CurvePointType curvePointType;

		public Vector2 position;

		public string xExpression;
		public string yExpression;

		public Vector2 localHandleA;
		public Vector2 localHandleB;


		public float lastConvertTime;


		
		public CurvePoint (float _x, float _y)
		{
			position.x = _x;
			position.y = _y;

		}
		public CurvePoint (Vector2 p)
		{
			position = p;
			curvePointType = CurvePointType.Point;
			
		}
		public CurvePoint (Vector2 p, Vector2 a)
		{
			position = p;

			localHandleA = a - position;

			curvePointType = CurvePointType.BezierMirrored;
			localHandleB = -localHandleA;
		}
		public CurvePoint (Vector2 p, Vector2 a, Vector2 b)
		{
			position = p;

			localHandleA = a - position;
			localHandleB = b - position;

		}

		public bool isPoint()
		{
			if (curvePointType == CurvePointType.Point)
				return true;
			return false;
		}
		public bool isBezierPoint()
		{
			if (curvePointType == CurvePointType.BezierBroken || curvePointType == CurvePointType.BezierMirrored || curvePointType == CurvePointType.BezierUnified)
				return true;
			return false;
		}


		public void cycleConvertPoint(CurvePoint prev = null, CurvePoint next = null, float sinceLastClick = .5f )
		{
			
			if ((Time.time-lastConvertTime) > sinceLastClick)
			{
				
				switch(curvePointType)
				{
					case CurvePointType.Point:
						convertToBezier(prev, next);
						break;
					case CurvePointType.BezierMirrored:
						convertToBezierBroken(prev, next);;
						break;

					default:
						convertToPoint();
						break;

				}
			}

			lastConvertTime = Time.time;
		}

		public void convertToPoint()
		{
			curvePointType = CurvePointType.Point;
			localHandleA = Vector2.zero;
			localHandleB = Vector2.zero;

		}

		public void convertToBezier(CurvePoint prev = null, CurvePoint next = null)
		{
			
			curvePointType = CurvePointType.BezierMirrored;
			if (prev != null && next != null)
			{
				Vector2 tangent = (next.position-prev.position) * .25f;
				setHandleA(position-tangent);
				setHandleB(position+tangent);
			}

		}
		public void convertToBezierBroken(CurvePoint prev = null, CurvePoint next = null)
		{
			
			//curvePointType = CurvePointType.BezierMirrored;

			if (curvePointType != CurvePointType.BezierMirrored && prev != null && next != null)
			{
				Vector2 tangent = (next.position-prev.position) * .25f;
				setHandleA(position-tangent);
				setHandleB(position+tangent);
			}
			curvePointType = CurvePointType.BezierBroken;

		}




		public void setHandleA(Vector2 gloabalA)
		{
			localHandleA =  gloabalA - position;

			if (curvePointType == CurvePointType.BezierMirrored)
				localHandleB = -localHandleA; 
				
		}
		public void setHandleB(Vector2 gloabalB)
		{
			localHandleB = gloabalB - position ;

			if (curvePointType == CurvePointType.BezierMirrored)
				localHandleA = -localHandleB; 
				
		}




		public Vector3 asVec3()
		{
			return new Vector3(position.x, 0, position.y);
		}



		
		
	}
}
