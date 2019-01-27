#pragma warning disable

using UnityEngine;

using AXGeometryTools;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometryTools.CurvePoint>;



namespace AX.Generators
{



	public class Instance2D : AX.Generators.Generator2D, IShape
	{
		
		public override string GeneratorHandlerTypeName { get { return "GeneratorHandler2D"; } }
		//public override string GeneratorHandlerTypeName { get { return "ShapeMergerHandler"; } }


		// POLLED MEMBERS

		public float thickness;
		public float offset;



		//AXClipperLib.JoinType joinType;

		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			// parameters

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));

										
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));


		}




		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			if (parametersHaveBeenPolled)
				return;

			base.pollControlValuesFromParmeters();

			thickness 	= parametricObject.floatValue("Thickness");
			offset 		= parametricObject.floatValue("Offset");

		}



		// SHAPE_OFFSETTER :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{		

			if (P_Input == null ||  inputSrc_p == null )
				return null;

			// PRE_GENERATE
			preGenerate();


			P_Input.polyTree = null;
			AXShape.thickenAndOffset(ref P_Input, inputSrc_p);


				 		
			if (P_Output == null )
				return null;
							     
			   
			P_Output.polyTree = null;

			//Debug.Log("thickness="+thickness+", offset=" + offset);
			AXShape.thickenAndOffset(ref P_Output, P_Input);

			//if (P_Output.polyTree != null)
			//Debug.Log(" P_Output.polyTree 1="+ Clipper.PolyTreeToPaths(P_Output.polyTree).Count);



			if ( P_Output.polyTree != null)
			{
				transformPolyTree(P_Output.polyTree, localMatrix);
			}
			else if (P_Output.paths != null)
			{
				P_Output.paths = transformPaths(P_Output.paths, localMatrix);
				P_Output.transformedControlPaths = P_Output.paths;
			}

			calculateBounds();

			return null;
	
		}

	} // \ShapeOffsetter 

}