using UnityEngine;
using System;
using System.Collections;

namespace AXGeometryTools
{
	[Serializable]
	public class AXTexCoords
	{
		public Vector2 	scale;
		public bool 	scaleIsUnified;
		public Vector2 	shift;
		public bool 	runningU;
		public bool     rotateSidesTex;
		public bool     rotateCapsTex;

		// Let's grow the AXTex to be more of a game material
		// in that properties such as density, hardness, etc. may be stored here.


		public AXTexCoords()
		{
			shift 			= Vector2.zero;
			scale 			= new Vector2(5f, 5f);
			scaleIsUnified 	= true;

			runningU 		= true;
			rotateSidesTex 	= false; 
			rotateCapsTex 	= false; 
		}   
		public AXTexCoords(AXTexCoords tex)
		{
			shift 			= new Vector2(tex.shift.x, tex.shift.y);
			scale 			= new Vector2(tex.scale.x, tex.scale.y);
			scaleIsUnified 	= tex.scaleIsUnified;

			runningU 		= tex.runningU;
			rotateSidesTex 	= tex.rotateSidesTex; 
			rotateCapsTex 	= tex.rotateCapsTex; 

		}   


		public void printOut()
		{
			Debug.Log("scale="+scale + ", shift="+shift);

		}

	}
}
