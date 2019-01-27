using UnityEngine;
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

using System.IO;

using AX.SimpleJSON;

 


using AXGeometryTools;

using AX.Generators;

using AXClipperLib;
using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;
 

namespace AX {

	[System.Serializable]
	public class Pather  
	{

		public Path path;
		public int[] segment_lengths;

		public  Pather(Path p)
		{
			path = p;
			segment_lengths = getSegmentLengths();
		}



		public static int getSegLenBasedOnSubdivision(Path path, int subdivision)
		{	
			if (subdivision <= 0)
				return 10000;
		
			Paths paths = new Paths();
			paths.Add(path);
			return getSegLenBasedOnSubdivision(paths, subdivision);

		}

		public static int getSegLenBasedOnSubdivision(Paths paths, int subdivision)
		{
			if (subdivision <= 0)
				return 10000;

			IntRect bounds = AXClipperLib.Clipper.GetBounds(paths);



			long bot 	= bounds.bottom;
			long top 	= bounds.top;
			long left 	= bounds.left;
			long right 	= bounds.right;



			if (left  > right)
			{
				left = bounds.right;
				right =  bounds.left;
			}
			if (bot > top)
			{
				bot = bounds.top;
				top = bounds.bottom;
			}
			int width 	= (int) Math.Abs(right-left);
			int height 	= (int) Math.Abs(top-bot);

			int max = width > height ? width : height;

			return max / subdivision;



		}


		public static Paths cleanPaths(Paths paths, int t = 10)
		{
			Paths retps = new Paths();

			for(int i=0; i<paths.Count; i++)
			{
				retps.Add(cleanPath(paths[i], t));
			}

			return retps;

		}
		public static Path cleanPath(Path path, int t = 10)
		{
			
			Path retp = new Path();

			int t2 = t*t;

			for(int i=0; i<path.Count-1; i++)
			{
				long d2 = DistanceSquared(path[i], path[i+1]);


				if (d2 < t2)
				{
					
					retp.Add(Lerp(path[i], path[i+1], .5f));
					i++;
				}
				else 
				{
					retp.Add(path[i]);

					if (i == path.Count-2)
						retp.Add(path[i+1]);
				}
			}

			return retp;

		}


		public static Paths segmentPaths(Paths paths, long bay=5000, bool isClosed = true)
		{
			Paths retps = new Paths();

			for (int i=0; i<paths.Count; i++)
			{
				retps.Add(segmentPath(paths[i], bay, isClosed));
				//AXGeometryTools.Utilities.printPath(segmentPath(paths[i], bay, isClosed));
			}
			//AXGeometryTools.Utilities.printPaths(retps);
			return retps;

		}

		public static Path segmentPath(Path path, long bay = 5000, bool isClosed = true)
		{
			Path retp = new Path();

			int endIndex = path.Count-1;

			for (int i=0; i<=endIndex; i++)
			{
				retp.Add(path[i]);

				int next = (i==path.Count-1) ? 0 : i+1;

				if (next == 0 && ! isClosed)
				{
					break;
				}
				//Debug.Log("i="+i+", next="+next);
				long d = Distance(path[i], path[next]);

					if (d > bay)
					{
						//a dd interstitial points
						int steps = (int) (d/bay);



						for (int j=1; j<steps; j++)
						{
							//Debug.Log("- " + j);

							float t = ((float)j)/((float)steps);
							retp.Add(Lerp(path[i], path[next], t));
						}
					}
//				


			}

			//retp.Add(path[path.Count-1]);

			return retp;


		}



		// MANPAGE: http://www.archimatix.com/uncategorized/axspline-getinsetcornerspines
		public Paths getInsetCornerSplinesOLD(float inset)
		{
			//There can't be more subsplines then there are vertices...
			// Each of these subslines will have at least 3 points


			Paths returnPaths; 


			// PLANSWEEPS AT CORNERS
			// First, go around and group verts that are closer
			// to  each other than min_sengmentLength
			float min_sengmentLength = 2*inset;



			// FIND FIRST CURSOR VERTEX

			int cursor = 0; // essentially 0

			if (segment_lengths[cursor] < min_sengmentLength )
			{
				cursor = path.Count-1;

				// back up to first long segment you find
				while (segment_lengths[cursor] < min_sengmentLength )
				{
					if (cursor == 0)
					{
						// if we mad it all the way back to 0, then all the segments were too small.
						// just return this AXSpline
						returnPaths = new Paths();
						returnPaths.Add(path);
						return returnPaths;
					}
					cursor--;
				}

			}

			
			// OK: Now we have our starting point: cursor. 
			// Proceed forward from here with the grouping.

			// Use a single array of ints with -88888 as seperators and -99999 as terminator.
			// Cant have more the 2*verCount entries (a seperator between each vert)

			int[] groupedIndicies = new int[2*path.Count*100];

			int index = 0;
			groupedIndicies[index++] = cursor++;


			int countOfSplines = 1;

			// GROUP VERTS THAT DEFINE THE SUBSPLINES
			while ( (cursor % path.Count) != groupedIndicies[0])
			{
				if (segment_lengths[cursor % path.Count] > min_sengmentLength)
				{
					countOfSplines++;
					groupedIndicies[index++] = -88888; // add break code
				}
				
				groupedIndicies[index++] = cursor % path.Count;

				// starting from cursor, add vertices to subspline

				cursor++;
			}

			// done... add terminator
			groupedIndicies[index++] = -99999;

			   

			// Take each group and add a beginning and ending vertex inset by margin.
			returnPaths = new Paths();


			Path subpath = null;

			for(int j=0; j<groupedIndicies.Length; j++)
			{
				if (j==0 || groupedIndicies[j] == -88888 || groupedIndicies[j] == -99999)
				{
					// End a spline
					if (groupedIndicies[j] == -88888 || groupedIndicies[j] == -99999)
					{ 
						// Add end vert
						int nexti = (groupedIndicies[j-1]+1) % path.Count;
						float percentInset = inset/segment_lengths[nexti];
						IntPoint endVert = Lerp ( path[groupedIndicies[j-1]], nextPoint(groupedIndicies[j-1]), percentInset);
			
						subpath.Add(endVert);
						returnPaths.Add(subpath);

						if (groupedIndicies[j] == -99999)
							break;
					}

					// Begin a spline
					if 	(j==0 || groupedIndicies[j] == -88888)
					{
						// skip over -88888
						if 	(groupedIndicies[j] == -88888) 
							j++;
						// start new AXSpline...
						subpath = new Path();

						//int nexti = (groupedIndicies[j-1]+1) % path.Count;
						float percentInset = inset/segment_lengths[groupedIndicies[j]];

						IntPoint begVert = Lerp (previousPoint(groupedIndicies[j]), path[groupedIndicies[j]], 1-percentInset);
						subpath.Add(begVert);
						subpath.Add(path[groupedIndicies[j]]);
					}
				}
				else
				{
					subpath.Add(path[groupedIndicies[j]]);
				}
			}

			/*
			Debug.Log("===========================");
			for(int j=0; j<groupedIndicies.Length; j++)
			{
				Debug.Log(groupedIndicies[j]);
				if (groupedIndicies[j] == -99999)
					break;
			}

			foreach(Path s in returnPaths)
			{
				Debug.Log("----");
				AXGeometryTools.Utilities.printPath(s);
			}
			*/


			return returnPaths;

		}



		public static VertexProperties getVertexProperties(Path path, int i)
		{

			int prev_i = (i==0) 			? path.Count-1 : i-1;
			int next_i = (i==path.Count-1) 	? 			 0 : i+1;

			Vector2 pp = new Vector2(path[prev_i].X, path[prev_i].Y);
			Vector2  p = new Vector2(path[i].X, path[i].Y);
			Vector2 np = new Vector2(path[next_i].X, path[next_i].Y);

			Vector2 v1 	=  p - pp;
			Vector2 v2 	= np - p;

			// NODE ROTATION & TRANSFORM
			Vector2 v1PN = (new Vector2(v1.y, -v1.x)).normalized;
			Vector2 v2PN = (new Vector2(v2.y, -v2.x)).normalized;
			
			// -- BISECTOR: the addition of the normalized perpendicular vectors leads to a bisector
			Vector2 bisector = v1PN + v2PN ;

			float tmp_ang = -Mathf.Atan2(bisector.y, bisector.x)*Mathf.Rad2Deg;
			if (tmp_ang < 0)
				tmp_ang += 360;

			
			// BEVEL ANGLE
			float bevelAng = Vector2.Angle(bisector, v2) - 90;

			return new VertexProperties(bisector, bevelAng);

		}




		/// <summary>
		/// Tese to see if the Path has a convex angle.
		/// </summary>
		/// <returns><c>true</c>, if a concave angle is found, it returns false.
		/// If it makes it to the end with no concave angles, it returns fales.</returns>
		/// <param name="path">Path.</param>
		public static bool isConvex(Path path)
		{
			for (int i=0; i<path.Count; i++)
			{
				VertexProperties vps = getVertexProperties(path, i);

				if (vps.bevelAngle < 0)
					return false;
			}

			return true;
		}




        //public static Paths difference(Path path, Path cutter)
        //{

        //    Clipper c = new Clipper(Clipper.ioPreserveCollinear);
        //    c.AddPath(path, PolyType.ptSubject, true);
        //    c.AddPath(cutter, PolyType.ptClip, true);

        //    PolyTree polyTree = new AXClipperLib.PolyTree();
        //    c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        //    return Clipper.PolyTreeToPaths(polyTree);

        //}

        //public static Paths difference(Paths paths, Path cutter)
        //{

        //    Clipper c = new Clipper(Clipper.ioPreserveCollinear);
        //    c.AddPaths(paths, PolyType.ptSubject, true);
        //    c.AddPath(cutter, PolyType.ptClip, true);

        //    PolyTree polyTree = new AXClipperLib.PolyTree();
        //    c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        //    return Clipper.PolyTreeToPaths(polyTree);

        //}
        public static Paths difference(Paths paths, Paths cutters)
        {

            Clipper c = new Clipper(Clipper.ioPreserveCollinear);
            c.AddPaths(paths, PolyType.ptSubject, true);
            c.AddPaths(cutters, PolyType.ptClip, true);

            PolyTree polyTree = new AXClipperLib.PolyTree();
            c.Execute(ClipType.ctDifference, polyTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return Clipper.PolyTreeToPaths(polyTree);

        }


		/// <summary>
		/// Splits the into convex paths.
		/// If the Path has a concave angle, the bisector is extended to find an intersection
		/// with line sgements further allong. 
		/// Once the intersection is found two paths are generated and returned.
		// This is run recursively.
		/// </summary>
		/// <returns>A set of convex paths.</returns>
		/// <param name="path">Path.</param>
		public static Paths splitIntoConvexPaths(Path path, int gener = 0, int part=0)
		{
			Paths returnPaths = new Paths();

			if (gener > 25)
			{
				returnPaths.Add(path);
				return returnPaths;
			}

//			Debug.Log("["+gener+"]["+part+"] * * * * * * * * * * * * splitIntoConvexPaths * * * * * * * * * * * *");
//			Pather.printPath(path);



			for (int i=0; i<path.Count; i++)
			{

				VertexProperties vps = getVertexProperties(path, i);

				//Debug.Log(bevelAng);

				if (vps.bevelAngle < 0f)
				{
					Vector2 nBisector = -vps.bisector;

					//Debug.Log("CONCAVE FOUND at (" + i + "): " + path[i] +" this is concave! bisector=" + nBisector);

					PointOnPathSegment pops = findSelfIntersection(path, i, nBisector);
					//Debug.Log("Intersection Before: "+pops.index+", (" + pops.point.X+", "+pops.point.Y+")");

					if (pops.index >= 0)
					{
						// split paths at this index
						 
						// A

						Path a = new Path();

						a.Add(path[i]);

						if (pops.t < .2f)
							a.Add(path[prevI(path, pops.index)]);
						else if (pops.t < .9f)
							a.Add(pops.point);

						int j = pops.index;
						while (j != i)
						{
							a.Add(path[j]);
							j = nextI(path, j);
						}


						// B

						Path b = new Path();

						//Debug.Log (gener + ":"+part+ "; t="+pops.t + ",  index="+pops.index);

						if (pops.t < .9f)
						{
							if (pops.t > .2f)
								b.Add(pops.point);
						}
						else 
							b.Add(path[pops.index]);
						

						b.Add(path[i]);

						j = nextI(path, i);
						while (j != pops.index)
						{
							b.Add(path[j]);
							j = nextI(path, j);
						}

						//Debug.Log("["+gener+"] ADD A %%%%");
						returnPaths.AddRange(splitIntoConvexPaths(a, gener+1, 1));
						//Debug.Log("["+gener+"] ADD A %%%% -- END");

						//Debug.Log("["+gener+"] ADD B %%%%");
						returnPaths.AddRange(splitIntoConvexPaths(b, gener+1, 2));
						//Debug.Log("["+gener+"] ADD B %%%% -- END");

						//printPaths(returnPaths);
					}
//					if (gener == 0)
//						printPaths(returnPaths);

					return returnPaths;

				}
			}

			//Debug.Log("!!! ["+gener+"] No concave found..............");
			returnPaths.Add(path);

			return returnPaths;

		}


		public static Paths spltPathAtY(Path p, float y)
		{
			Paths paths = new Paths();

			Path a = new Path();
			Path b = new Path();


			int Y = sup(y);

			bool isAboveY = false;

			for (int i=0; i<p.Count; i++)
			{

				if(isAboveY)
				{
					b.Add(p[i]);
					continue;
				}

				a.Add(p[i]);

				Debug.Log(p[i].Y + " -- " + Y);
				if (p[i].Y > Y)
				{	// just crossed over
					// start b
					Debug.Log("crossed");
					isAboveY = true;
					b.Add(p[i]);
				}
			}

			paths.Add(a);
			paths.Add(b);

			return paths;
		}


		public static  PointOnPathSegment findSelfIntersection(Path path, int fromIndex, Vector2 normal)
		{
			// start at segment after next segment
			int prev_i = nextI(path, fromIndex);
			int 	 i = nextI(path, prev_i);

			//Debug.Log(normal);

			while (i != fromIndex)
			{
				PointOnPathSegment pops = raySegmentIntersection(IP2Vector2(path[prev_i]), IP2Vector2(path[i]), IP2Vector2(path[fromIndex]), normal );
				//Debug.Log(" ---X " + inter);


				if (pops.point.X != -999999 && pops.point.Y != -999999)
				{
					// We have found an intersection point!
					pops.index = i;
					return pops;

				}


				prev_i = i;
				i = nextI(path, i);
			}


			return new PointOnPathSegment(-1, new IntPoint());

		}

	


		//	This is looking for the intersection point between the line segments
		//	AB and CD. We are using the parametric equation A-bt = C - du, where
		//	b is the vector (B-A) and d is the vector (D-C). We want to solve for
		//	t and u - if they are both between 0 and 1, we have a valid
		//	intersection point.

		public static PointOnPathSegment raySegmentIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 n)
		{
			int rayLength 		= 10000000;

			Vector2 D			= (C+(rayLength*n));
			Vector2 b		 	= B-A;
			Vector2 d		 	= D-C;
			Vector2 d_perp	 	= new Vector2(-d.y, d.x);
			float denom 		= Vector2.Dot(d_perp, b);


			//if (denom == 0) { return new Vector2(-888888, -888888); } // parallel: no intersection possible


			Vector2 c		= C-A;
			float numer	 	= Vector2.Dot(d_perp, c);
			float t		 	= numer / denom;
			
			
			if (0 <= t  && t <= 1) {
				//Debug.Log("t = " + t);

				Vector2 b_perp = new Vector2(-b.y, b.x);
				
				numer = Vector2.Dot(b_perp, c);
				//numer = b_perp.x*c.x + b_perp.y*c.y;
				
				float u = numer / denom;
				if (0<= u && u <=1) {
					// sements intersect!
					//Debug.Log(" ===>  u = " + u);// + ", inbound = " + inbound);


					return new PointOnPathSegment(t, Vector2IP(Vector2.Lerp(A, B, t)));
				}
			}
			// no intersection found
			return new PointOnPathSegment(-1, new IntPoint(-999999, -999999));
		}





		public static Paths offset(Paths paths, float offset)
		{
			// Set cleaning precision
			IntRect brect = Clipper.GetBounds(paths);
			int cleanPolygonPrecision = 2;
			if ( (brect.right-brect.left) > 10000)
				cleanPolygonPrecision = 30;


			// Clean...
			AXClipperLib.JoinType jt =  AXClipperLib.JoinType.jtSquare ;
			paths = AXGeometryTools.Utilities.cleanPaths(paths, cleanPolygonPrecision);



			Paths 		resPaths 	= new Paths ();
			AXClipperLib.PolyTree 	resPolytree = null;


			// OFFSETTER
			ClipperOffset 	co  = new ClipperOffset ();
			co.MiterLimit = 2.0f;
			


			foreach (Path path in paths)
			{

				co.Clear();
				resPolytree = null;

				co.AddPath (path, jt, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);

				// this resPolytree has transformed curves in it
				resPolytree = new AXClipperLib.PolyTree();
				co.Execute (ref resPolytree, (double)(offset * AXGeometryTools.Utilities.IntPointPrecision));
				resPaths.AddRange(Clipper.ClosedPathsFromPolyTree(resPolytree));
			}

			return resPaths;

		}





		// UTILITIES

		public static Vector2 IP2Vector2(IntPoint ip)
		{
			return new Vector2(ip.X, ip.Y);
		}

		public static IntPoint Vector2IP(Vector2 v)
		{
			return new IntPoint((int)v.x, (int)v.y);
		}



        public static Vector2 IP2Vector2WithPrecision(IntPoint ip)
        {
            return new Vector2((((float)ip.X) / AXGeometryTools.Utilities.IntPointPrecision), (((float)ip.Y) / AXGeometryTools.Utilities.IntPointPrecision));
        }
        public static IntPoint Vector2IPWithPrecision(Vector2 v)
        {
            return new IntPoint((int)(v.x * AXGeometryTools.Utilities.IntPointPrecision), (int)(v.y * AXGeometryTools.Utilities.IntPointPrecision));
        }



		public static IntPoint Lerp(IntPoint a, IntPoint b, float p)
		{
			return new IntPoint( ((b.X-a.X)*p + a.X), ((b.Y-a.Y)*p +a.Y));
		}


		public static bool Equals(IntPoint a, IntPoint b)
		{
			if (a.X == b.X && a.Y == b.Y)
				return true;

			return false;
			
		}
		public static long DistanceSquared (IntPoint a, IntPoint b)
		{
			if (a.X == b.X && a.Y == b.Y)
				return 0;

						long diffx = Math.Abs(b.X-a.X);
			long diffy = Math.Abs(b.Y-a.Y);

			return  diffx*diffx + diffy*diffy;
		}



		public static long Distance(IntPoint a, IntPoint b)
		{
			if (a.X == b.X && a.Y == b.Y)
				return 0;

			return (long) Mathf.Sqrt( (float) Mathf.Pow((b.X-a.X), 2) + (float) Mathf.Pow((b.Y-a.Y), 2) );
		}

		public static int prevI(Path path, int i)
		{
			return (i==0) 			? path.Count-1 : i-1;
		}
		public static int nextI(Path path, int i)
		{
			return (i==path.Count-1) 	? 			 0 : i+1;
		}




		public static void printPath(Path path)
		{
//			for (int i = 0; i < path.Count; i++) {
//				IntPoint ip = path [i];
//				Debug.Log ("(" + i + ") " + ip.X + ", " + ip.Y);
//			}
			Debug.Log ( pathToString(path) );
		}
		public static string pathToString(Path path) 
		{
			string ret = "";
			for (int i = 0; i < path.Count; i++) {
				IntPoint ip = path [i];
				ret += "  ["+i+"] (" + ip.X + ", " + ip.Y + ")\r";
			}
			return ret;
		}
		public static void printPaths(Paths paths)
		{
			if (paths == null) {
				Debug.Log("print paths: EMPTY");
				return;
			}
			Debug.Log (paths.Count + " paths ------- ");
			int c = 0;
			foreach(Path p in paths)
				Debug.Log ("["+(c++)+"] " + pathToString(p));
			Debug.Log ("end paths ------- ");

		}





		/***********************************
		 From a given vertex, get its previous vertex point.
		 If the given point is the first one, 
		 it will return  the last vertex;
		 ***********************************/
		public IntPoint previousPoint(int index) {		

			if (path.Count > 0)
				return path[( ((index-1)<0) ? (path.Count-1) : index-1)];;
			return new IntPoint();
		}
		public IntPoint nextPoint(int index) {		
			if (path.Count > 0)
				return path[ (index+1) % path.Count ];
			return new IntPoint();
		}

		public static IntRect getBounds (Path path)
		{
			Paths paths = new Paths();
			paths.Add(path);
			return AXClipperLib.Clipper.GetBounds(paths);

		}











		public  int[] getSegmentLengths()
		{
			segment_lengths = new int[path.Count];

			int segment_length = 0;

			for (int i=0; i<path.Count; i++) {
					segment_length 		= (int) Distance(previousPoint(i), path[i]);
					segment_lengths[i] 	= segment_length;

					/*
					if (i > 0) 
						curve_distance 	   += segment_length;

					curve_distances[i] 	= curve_distance;
					*/
			}
			return segment_lengths;


		}







		// TRANSFORM_PATH
		public static void shiftPath(Path path, IntPoint ip)
		{
			if (path == null)
				return;

			for (int j=0; j<path.Count; j++)
			{
				path[j] = new IntPoint(path[j].X + ip.X, path[j].Y + ip.Y);
			}
		}

		// TRANSFORM_PATHS
		public static void shiftPaths(Paths paths,  IntPoint ip)
		{
			//Debug.Log(m);
			if (paths == null)
				return;


			for(int i=0; i<paths.Count; i++)
			{
				shiftPath(paths[i], ip);

			}
		}

		// TRANSFORM_POLY_TREE
		public static void shiftPolyTree(AXClipperLib.PolyTree polyTree, IntPoint ip)
		{
			if (polyTree == null)
				return;

			if (polyTree.Childs != null && polyTree.Childs.Count > 0)
				shiftPolyNode(polyTree.Childs, ip);
		}

		// TRANSFORM_POLY_NODE
		public static void shiftPolyNode(List<PolyNode> childs, IntPoint ip)
		{
			if (childs == null || childs.Count == 0)
				return;

			foreach(PolyNode child in childs)
			{
//				Path tmpPath = shiftPath(child.Contour, ip);
//				for (int i = 0; i < tmpPath.Count; i++) 
//					child.Contour[i] = tmpPath [i];

				shiftPath(child.Contour, ip);

				if (child.Childs != null)
					shiftPolyNode(child.Childs, ip);
			}
			
		}



		public static int sup(float num)
		{
			return (int) (num * AXGeometryTools.Utilities.IntPointPrecision);
		}




        public static Paths thicken(Paths paths, float t)
        {
            ClipperOffset co = new ClipperOffset();
            co.AddPaths(paths, JoinType.jtMiter, EndType.etOpenButt);

            AXClipperLib.PolyTree resPolytree = new AXClipperLib.PolyTree();
            co.Execute(ref resPolytree, (double)(t * AXGeometryTools.Utilities.IntPointPrecision));

            return Clipper.PolyTreeToPaths(resPolytree);

        }



		/// <summary>
		///  Home-grown offsetter that can handle open paths (which clipper can't)
		///  and returns paths for left and right. 
		/// </summary>
		/// <returns>The offsets.</returns>
		/// <param name="planSpline">Plan spline.</param>
		/// <param name="thickR">Thick r.</param>
		/// <param name="thickL">Thick l.</param>
		public static Paths  wallOffsets (Spline planSpline, float thickR, float thickL)
		{
			/*
			 	When generating a PlanSweep, we need to know the breaking angles of each plan layer.
			 	This is because the original plan node might be convex at a certain section offset, it is concave, depending on how far out that section goes. 
			 	
			 	To do this:
			 	1. for each section node, do an offset using clipper - or use bevelAngles of the orginnal plan and make a new spline, whichever is more efficient
			 	2. store these offset plans as AX.Splines in a list.
			 	3. use these Splines for the isSharp and isBlend conditionals
			 
			 */

			if ( planSpline == null  ||   planSpline.controlVertices == null 	||    planSpline.controlVertices.Count == 0)
				return null;


//			if (tex == null)
//				return null;




			Paths paths = new Paths();

			Path pathR = new Path();
			Path pathL = new Path();

			float samePointTolerence = .001f;


			int terminIndexSubtractor = 0;
			if (Vector2.Distance(planSpline.controlVertices[0], planSpline.controlVertices[planSpline.controlVertices.Count-1]) < samePointTolerence)
				terminIndexSubtractor = 1;


			//Matrix4x4 prevBevelTransform;

			for (int i=0; i<planSpline.controlVertices.Count-terminIndexSubtractor; i++)
			{

				Matrix4x4 bevelTransform = planSpline.nodeTransforms[i];

				if (planSpline.shapeState == ShapeState.Open)
				{
					if (i==0)
						bevelTransform = planSpline.begTransform;
					else if (i==planSpline.controlVertices.Count-1)
						bevelTransform = planSpline.endTransform;
				}	


				// Transform plan vert
				Vector3 vertr = bevelTransform.MultiplyPoint(new Vector3(thickR, 0, 0));
				Debug.Log(vertr);

				pathR.Add(Pather.Vector2IPWithPrecision(new Vector2(vertr.x, vertr.z)));

				Vector3 vertl = bevelTransform.MultiplyPoint(new Vector3(thickL, 0, 0));
				pathL.Add(Pather.Vector2IPWithPrecision(new Vector2(vertl.x, vertl.z)));


			}

			paths.Add(pathR);
			paths.Add(pathL);

			return paths;
		}







	} // /PATHER















	public struct VertexProperties
	{
		public Vector2 bisector;
		public float	bevelAngle;

		public VertexProperties(Vector2 _b, float _a)
		{
			bisector 	= _b;
			bevelAngle 	= _a;
		}

	}
	public  struct PointOnPathSegment
	{
		public int 			index;
		public float			t;
		public IntPoint 	point;

		public PointOnPathSegment(float _t, IntPoint _p) 
		{
				t	= _t;
			point	= _p;
			index 	= 0;

		}
		public PointOnPathSegment(int _i, float _t) 
		{
			index 	= _i;
				t	= _t;
			point	= new IntPoint(-999999, -999999);
		}

		public PointOnPathSegment(int _i, float _t, IntPoint _p) 
		{
			index 	= _i;
				t	= _t;
			point	= _p;
		}

















	}













} //\AX
