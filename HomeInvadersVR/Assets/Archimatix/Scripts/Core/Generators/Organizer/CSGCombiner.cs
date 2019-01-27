using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;



using AXGeometryTools;


namespace AX.Generators
{



	/*	CSG Combiner
	 * 
	 * The Grouper has a list of ParametricObjects under its management.
	 * 
	 * A List of ParamtericObjects
	 */
	public class CSGCombiner : Organizer, IOrganizer, ILogic
	{

		public override string GeneratorHandlerTypeName { get { return "CSGCombinerHandler"; } }

		public List<AXParameter> inputs;

		public AXParameter P_Channel;

		public int channel = 1;


		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject ()
		{
			base.init_parametricObject ();

			parametricObject.useMeshInputs = true;
			parametricObject.meshInputs = new List<AXParameter> ();

			// PLAN SHAPE
			AXParameter p;

			parametricObject.addParameter (new AXParameter (AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
			p = parametricObject.addParameter (new AXParameter (AXParameter.DataType.CustomOption, "Channel"));
			p.optionLabels = new List<string> ();
			p.optionLabels.Add ("Item 1");
			p.optionLabels.Add ("Item 2");
			p.optionLabels.Add ("Item 3");
		

			parametricObject.addParameter (new AXParameter (AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));
		}



		public override void pollInputParmetersAndSetUpLocalReferences ()
		{
			base.pollInputParmetersAndSetUpLocalReferences ();

			P_Channel = parametricObject.getParameter ("Channel");
		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters ()
		{

			base.pollControlValuesFromParmeters ();


			channel = (P_Channel != null) ? P_Channel.intval : 0;


			inputs = parametricObject.getAllInputMeshParameters ();
			P_Channel.optionLabels = new List<string> ();

			for (int i = 0; i < inputs.Count; i++) {
				AXParameter p = inputs [i];
				if (p.DependsOn != null)
					P_Channel.optionLabels.Add (p.DependsOn.parametricObject.Name);

			}
		}

		// GROUPER::GENERATE
		public override GameObject generate (bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");


			if (!parametricObject.isActive)
				return null;

			

			preGenerate ();


			//GameObject go = null;

//			if (makeGameObjects && !parametricObject.combineMeshes)
//				go = ArchimatixUtils.createAXGameObject (parametricObject.Name, parametricObject);


			List<AXMesh> ax_meshes = new List<AXMesh> ();

			Net3dBool.BooleanModeller modeler;
			Net3dBool.Solid resSolid = null;


			// BOUNDING

			List<AXMesh> boundingMeshes = new List<AXMesh> ();


//			List<Net3dBool.Solid> solids = new List<Net3dBool.Solid> ();
//			List<Net3dBool.Solid> voids = new List<Net3dBool.Solid> ();


			AXParameter 		src_p = null;
			AXParametricObject src_po = null;

			if (inputs != null && inputs.Count > 0) {

				Debug.Log ("inputs.Count = " + inputs.Count);

				Mesh tmpMesh1 = new Mesh();
				Mesh tmpMesh2 = new Mesh();

				src_p = inputs [0].DependsOn;
				if (src_p != null) 
				{
					src_po = src_p.parametricObject;


					CombineInstance[] combinator = new CombineInstance[src_po.Output.meshes.Count];
					for (int bb = 0; bb < combinator.Length; bb++) {
						combinator [bb].mesh 		= src_po.Output.meshes [bb].mesh;
						combinator [bb].transform 	= src_po.Output.meshes [bb].transMatrix;
					}
					tmpMesh1 = new Mesh();
					tmpMesh1.CombineMeshes(combinator);
					tmpMesh1.RecalculateNormals();

//					Debug.Log("//////////////////");
//					for (int i = 0; i < tmpMesh1.vertices.Length; i++) {
//						Vector3 vert = tmpMesh1.vertices [i];
//						Debug.Log ("["+i+"] "+vert);
//
//					}
//					Debug.Log("//////////////////");

				}

				if (inputs.Count > 1 && inputs [1] != null)
				{
					src_p = inputs [1].DependsOn;

					if (src_p != null)
					{
						src_po = src_p.parametricObject;

						if (src_po != null)
						{


							CombineInstance[] combinator = new CombineInstance[src_po.Output.meshes.Count];
							for (int bb = 0; bb < combinator.Length; bb++) {
								combinator [bb].mesh 		= src_po.Output.meshes [bb].mesh;
								combinator [bb].transform 	= src_po.Output.meshes [bb].transMatrix;
							}
							tmpMesh2 = new Mesh();
							tmpMesh2.CombineMeshes(combinator);
							tmpMesh2.RecalculateNormals();






							// convert to csg meshes
							int len1 = tmpMesh1.vertices.Length;
							Net3dBool.Point3d[] pverts1 = new Net3dBool.Point3d[len1];
							Vector3 vert1;
							for (int i = 0; i < len1; i++) {
								vert1 = tmpMesh1.vertices [i];

								pverts1 [i] = new Net3dBool.Point3d (vert1.x, vert1.y, vert1.z);
							}
							Net3dBool.Solid a = new Net3dBool.Solid (pverts1,
								tmpMesh1.triangles, 
								getColorArray (len1, Color.red));



							// convert to csg meshes
							int len2 = tmpMesh2.vertices.Length;
							Net3dBool.Point3d[] pverts2 = new Net3dBool.Point3d[len2];
							Vector3 vert2;
							for (int i = 0; i < len2; i++) {
								vert2 = tmpMesh2.vertices [i];

								pverts2[i] = new Net3dBool.Point3d (vert2.x, vert2.y, vert2.z);
							}
							Net3dBool.Solid b = new Net3dBool.Solid (pverts2,
								tmpMesh2.triangles, 
								getColorArray (len2, Color.red));






							modeler = new Net3dBool.BooleanModeller (a, b);
							resSolid = modeler.getDifference ();



								Mesh tmesh = new Mesh ();
							int mlen = resSolid.getVertices ().Length;
							Net3dBool.Point3d[] bverts = resSolid.getVertices ();

							Vector3[] vertices = new Vector3[mlen];

							for (int i = 0; i < mlen; i++) {
								Net3dBool.Point3d p = bverts [i];
								vertices [i] = new Vector3 ((float)p.x, (float)p.y, (float)p.z);
							}
							tmesh.vertices = vertices;

							tmesh.triangles = resSolid.getIndices ();
								
							tmesh.RecalculateNormals ();





							ax_meshes.Add (new AXMesh (tmesh));


							parametricObject.finishMultiAXMeshAndOutput (ax_meshes, isReplica);
						}
					}
					return null;

				}













//
//				for (int ii = 0; ii < inputs.Count; ii++) {
//					src_p = inputs [ii].DependsOn;
//
//					if (src_p == null)
//						continue;
//
//					Debug.Log ("ii=" + ii + ": " + src_p.Name);
//
//					if (src_p != null) {
//						src_po = src_p.parametricObject;
//					
//
//						if (src_po.is3D ()) {
//							if (src_po.Output != null && src_po.Output.meshes != null) {
//								for (int j = 0; j < src_po.Output.meshes.Count; j++) {
//
//									int len = src_po.Output.meshes [j].mesh.vertices.Length;
//
//									// convert to csg meshes
//									Net3dBool.Point3d[] pverts = new Net3dBool.Point3d[len];
//									Vector3 vert;
//									for (int i = 0; i < len; i++) {
//										vert = src_po.Output.meshes [j].mesh.vertices [i];
//
//										pverts [i] = new Net3dBool.Point3d (vert.x, vert.y, vert.z);
//									}
//
//									if (ii == 0) {
//										solids.Add (new Net3dBool.Solid (pverts,
//											src_po.Output.meshes [j].mesh.triangles, 
//											getColorArray (len, Color.red)));
//									} else {
//										voids.Add (new Net3dBool.Solid (pverts,
//											src_po.Output.meshes [j].mesh.triangles, 
//											getColorArray (len, Color.red)));
//
//									}
//
//									Mesh tmesh = new Mesh ();
//									int mlen = resSolid.getVertices ().Length;
//									Net3dBool.Point3d[] bverts = resSolid.getVertices ();
//
//									Vector3[] vertices = new Vector3[mlen];
//
//									for (int i = 0; i < mlen; i++) {
//										Net3dBool.Point3d p = bverts [i];
//										vertices [i] = new Vector3 ((float)p.x, (float)p.y, (float)p.z);
//									}
//									tmesh.vertices = vertices;
//
//									tmesh.triangles = resSolid.getIndices ();
//										
//									tmesh.RecalculateNormals ();
//
//
//									AXMesh dep_amesh = src_po.Output.meshes [j];
//									ax_meshes.Add (dep_amesh.Clone (dep_amesh.transMatrix));
//								}
//							}
//
//
//						}
//					}
//
//				}




				//now have all our solids and voids

//				Debug.Log (solids.Count + " -- " + voids.Count);
//
//				if (solids.Count == 0 || voids.Count == 0)
//					return null;
//
//
//				for (int ss=2; ss<solids.Count; ss++)
//					{
//						modeler = new Net3dBool.BooleanModeller (resSolid, solids [1]);
//						resSolid = modeler.getUnion();
//					}
//
//				if (solids.Count == 1) {
//					modeler = new Net3dBool.BooleanModeller (solids [0], voids [0]);
//					resSolid = modeler.getDifference ();
//
//					if (voids.Count > 1) {
//						for (int v = 1; v < voids.Count; v++) {
//							modeler = new Net3dBool.BooleanModeller (resSolid, voids [v]);
//							resSolid = modeler.getDifference ();
//						}
//					}
//				}
//				else
//				{
//					modeler = new Net3dBool.BooleanModeller (solids [0], solids [1]);
//					resSolid = modeler.getUnion();					for (int ss=2; ss<solids.Count; ss++)
//					{
//						modeler = new Net3dBool.BooleanModeller (resSolid, solids [1]);
//						resSolid = modeler.getUnion();
//					}
//
//					for (int ss=2; ss<solids.Count; ss++)
//					{
//						modeler = new Net3dBool.BooleanModeller (resSolid, solids [ss]);
//						resSolid = modeler.getUnion();
//					}
//					for (int vv=0; vv<voids.Count; vv++)
//					{
//						modeler = new Net3dBool.BooleanModeller (resSolid, voids[vv]);
//						resSolid = modeler.getDifference();
//					}
//
//				}
//
//				if (resSolid == null)
//					return null;
//
//				Mesh tmesh = new Mesh ();
//				int mlen = resSolid.getVertices ().Length;
//				Net3dBool.Point3d[] bverts = resSolid.getVertices ();
//
//				Vector3[] vertices = new Vector3[mlen];
//
//				for (int i = 0; i < mlen; i++) {
//					Net3dBool.Point3d p = bverts [i];
//					vertices [i] = new Vector3 ((float)p.x, (float)p.y, (float)p.z);
//				}
//				tmesh.vertices = vertices;
//
//				tmesh.triangles = resSolid.getIndices ();
//					
//				tmesh.RecalculateNormals ();
//




				// BOUNDING MESHES
				//boundsCombinator[i].mesh 		= input_p.DependsOn.parametricObject.boundsMesh;
				//boundsCombinator[i].transform 	= input_p.DependsOn.parametricObject.generator.localMatrixWithAxisRotationAndAlignment;
//				if (src_po.boundsMesh != null)
//					boundingMeshes.Add (new AXMesh (src_po.boundsMesh, src_po.generator.localMatrixWithAxisRotationAndAlignment));


				// GAME_OBJECTS

//						if (makeGameObjects && !parametricObject.combineMeshes) {
//
//							GameObject plugGO = src_po.generator.generate (true, initiator_po, isReplica);
//							if (plugGO != null)
//								plugGO.transform.parent = go.transform;
//						}

				//ax_meshes.Add (new AXMesh (tmesh));


				//P_Output.meshes = src_p.meshes;





				// FINISH AX_MESHES


				//Debug.Log("ORG: " + ax_meshes.Count);
				parametricObject.finishMultiAXMeshAndOutput (ax_meshes, isReplica);



				// FINISH BOUNDS

				CombineInstance[] boundsCombinator = new CombineInstance[boundingMeshes.Count];
				for (int bb = 0; bb < boundsCombinator.Length; bb++) {
					boundsCombinator [bb].mesh = boundingMeshes [bb].mesh;
					boundsCombinator [bb].transform = boundingMeshes [bb].transMatrix;
				}
				setBoundsWithCombinator (boundsCombinator);


				if (P_BoundsX != null && !P_BoundsX.hasRelations () &&	!P_BoundsX.hasExpressions ())
					P_BoundsX.FloatVal = parametricObject.bounds.size.x;

				if (P_BoundsY != null && !P_BoundsY.hasRelations () &&	!P_BoundsY.hasExpressions ())
					P_BoundsY.FloatVal = parametricObject.bounds.size.y;

				if (P_BoundsZ != null && !P_BoundsZ.hasRelations () &&	!P_BoundsZ.hasExpressions ())
					P_BoundsZ.FloatVal = parametricObject.bounds.size.z;

			}



			// FINISH GAME_OBJECTS

//			if (makeGameObjects) {
//				if (parametricObject.combineMeshes) {
//					go = parametricObject.makeGameObjectsFromAXMeshes (ax_meshes, true, false);
//
//
//					// COMBINE ALL THE MESHES
//					CombineInstance[] combine = new CombineInstance[ax_meshes.Count];
//
//					int combineCt = 0;
//					for (int i = 0; i < ax_meshes.Count; i++) {
//						AXMesh _amesh = ax_meshes [i];
//						combine [combineCt].mesh = _amesh.mesh;
//						combine [combineCt].transform = _amesh.transMatrix;
//						combineCt++;
//					}
//
//					Mesh combinedMesh = new Mesh ();
//					combinedMesh.CombineMeshes (combine);
//
//					// If combine, use combined mesh as invisible collider
//					MeshFilter mf = (MeshFilter)go.GetComponent (typeof(MeshFilter));
//
//					if (mf == null)
//						mf = (MeshFilter)go.AddComponent (typeof(MeshFilter));
//
//					if (mf != null) {
//						mf.sharedMesh = combinedMesh;
//						parametricObject.addCollider (go);
//					}
//				} else {
//					Matrix4x4 tmx = parametricObject.getLocalMatrix ();
//
//					go.transform.rotation = AXUtilities.QuaternionFromMatrix (tmx);
//					go.transform.position = AXUtilities.GetPosition (tmx);
//					go.transform.localScale = parametricObject.getLocalScaleAxisRotated ();
//				}
//				return go;
//			}




			return null;

		}

		public Net3dBool.Color3f[] getColorArray (int length, Color c)
		{
			Net3dBool.Color3f[] ar = new Net3dBool.Color3f[length];
			for (var i = 0; i < length; i++)
				ar [i] = new Net3dBool.Color3f (c.r, c.g, c.b);
			return ar;
		}


	}
}