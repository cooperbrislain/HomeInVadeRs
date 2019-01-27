
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometryTools;
 

namespace AX.Generators
{
	
	

	
	// EXTRUDE GENERATOR
	public class WinWall : Generator3D, IMeshGenerator, ICustomNode
	{

		public List<AXParameter> inputs;

		//public override string GeneratorHandlerTypeName { get { return "ExtrudeHandler"; } }

		// INPUTS
		public AXParameter 			P_Plan;
		public AXParameter 			planSrc_p;
		public AXParametricObject 	planSrc_po;

		public AXParameter P_Height;


		// POLLED MEMBERS

		public float 	height 		= 3;

		public bool planIsClosed = false;

		public List<AXMesh> ax_meshes = null;



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();


			// PLAN SHAPE
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Plan"));
			

			parametricObject.useSplineInputs = true;
			parametricObject.splineInputs = new List<AXParameter>();

			P_Height = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "Height", 2f, .01f, 1000f);
			P_Height.sizeBindingAxis = Axis.Y;


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));

		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Plan 			= parametricObject.getParameter("Plan");


			P_Height 		= parametricObject.getParameter("Height");

		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			
			base.pollControlValuesFromParmeters();

			planSrc_p		= P_Plan.DependsOn; //getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;
			 
		}





		// GENERATE WINWALL
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			if (parametricObject == null || ! parametricObject.isActive)
				return null;


			//Debug.Log("PlanSweep::generate()");

			// RESULTING MESHES
			ax_meshes = new List<AXMesh>();


			preGenerate();


			
			// PLAN
			// The plan may have multiple paths. Each may generate a separate GO.
			
			if (P_Plan == null)
				return null;


			planSrc_p		= getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;

			if (planSrc_p == null || ! planSrc_p.parametricObject.isActive)
				return null;


			planIsClosed 		= (P_Plan.hasThickness || P_Plan.shapeState == ShapeState.Closed) ? true : false;

			
			P_Plan.polyTree = null;

			Paths planPaths = planSrc_p.getPaths();

			Path planPath = planPaths[0];

			Spline planSpline 		= new Spline(planPath, planIsClosed, P_Plan.breakGeom, P_Plan.breakNorm);



			Paths offsetPaths = Pather.wallOffsets(planSpline, .5f,.5f);

			Pather.printPaths(offsetPaths);


			// each path, step through and mak a rectangle 
			// segment wide and height and then subtract windows.

			//Then make poly and add to combiner

			Path window = AXTurtle.Rectangle(1, 1, false);
			//window.Reverse();
			Pather.shiftPath(window, new IntPoint(10000,5000));

			Debug.Log("==========");
			Pather.printPath(window);




			Pather rightPather = new Pather(offsetPaths[0]);

			int[] rightLengths = rightPather.segment_lengths;

			for (int i=0; i<rightLengths.Length; i++)
			{
				Debug.Log(rightLengths[i]);

				int next_i = (i == rightLengths.Length-1) ? 0 : i+1;

				Path rect = AXTurtle.Rectangle(rightLengths[next_i]/10000f, 3, false);

				 


				Clipper c 		= new Clipper();
				c.AddPath(rect, PolyType.ptSubject, true);

				// fenestration
				c.AddPath(window, PolyType.ptClip, true);

				AXClipperLib.PolyTree polytree = new AXClipperLib.PolyTree();
				c.Execute(ClipType.ctDifference, 	polytree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);

				Paths pathResult = Clipper.PolyTreeToPaths(polytree);


				Pather.printPaths(pathResult);

				Mesh mesh = AXPolygon.triangulate(polytree, new AXTexCoords());


				Matrix4x4 wallm	= Matrix4x4.TRS(new Vector3(offsetPaths[0][i].X/10000f, 0, offsetPaths[0][i].Y/10000f), Quaternion.Euler(-90, planSpline.edgeRotations[i], 0), Vector3.one);

				ax_meshes.Add(new AXMesh(mesh,wallm));


			}

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);


			// FINISH BOUNDING

			setBoundaryFromAXMeshes(ax_meshes);


			if (makeGameObjects)
				return parametricObject.makeGameObjectsFromAXMeshes (ax_meshes);


			return null;
		}






	}
}
