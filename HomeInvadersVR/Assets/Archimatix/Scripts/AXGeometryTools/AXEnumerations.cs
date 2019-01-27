using System;

namespace AXGeometryTools
{
	public enum ShapeState {Closed, Open};
	public enum GeneratorType {Shape, MesherGenerators, Repeaters};
	public enum ColliderType {None, Box, Sphere, Capsule, Mesh, ConvexMesh};
	public enum RepeaterItem {Node, Cell, SpanU, SpanV, Corner};
	public enum Axis {NONE, X, Y, Z, NX, NY, NZ};
	public enum ThumbnailState {Open, Closed, Custom};

	public enum CurvePointType {Point, BezierMirrored, BezierUnified, BezierBroken, Smooth };

	public enum LineType{Line, Rail, Opening};

	public enum PrecisionLevel {Millimeter, Meter, Kilometer};
	 


}

