using System;
using System.Collections.Generic;

namespace MathTools
{
	public struct VertexConstraint{
		public int Vertex0 { get; set; }
		public int Vertex1 { get; set; }
		public float distance { get; set; }
	}

	public class VerletMesh : Mesh
	{
		public float Mass { get; set; }
		public Vec3 Force { get; set; }

		public List<VertexConstraint> VertexConstraints{ get; set;} 
		public List<Vec3> VertexOld { get; private set; }
		public List<Vec3> VertexNow { get { return VertexList; } set { VertexList = value; } }
		public IMatrix Model { get; set; }

		public void GenerateVertexConstraintsFromFaces(){
			foreach (var f in Faces) {
				var v0 = f.posIndex [0];
				var v1 = f.posIndex [1];
				var v2 = f.posIndex [2];

				var v01 = VertexList [v1] - VertexList [v0];
				var v02 = VertexList [v2] - VertexList [v0];
				var v12 = VertexList [v2] - VertexList [v1];

				VertexConstraints.Add(new VertexConstraint(){Vertex0 = v0,Vertex1 = v1, distance = v01.Norm()});
				VertexConstraints.Add(new VertexConstraint(){Vertex0 = v0,Vertex1 = v2, distance = v02.Norm()});
				VertexConstraints.Add(new VertexConstraint(){Vertex0 = v1,Vertex1 = v2, distance = v12.Norm()});

			}
			var vc = new List<VertexConstraint> (VertexConstraints);
			int found = VertexConstraints.Count;
			for (var i = 0; i<VertexConstraints.Count; i++)
				for (var j = i+1; j<VertexConstraints.Count; j++) {
					var v0 = VertexConstraints [i].Vertex0;
					var v1 = VertexConstraints [i].Vertex1;
					
				if((v0 == VertexConstraints [j].Vertex0 && v1 == VertexConstraints [j].Vertex1) 
				   || (v1 == VertexConstraints [j].Vertex0 && v0 == VertexConstraints [j].Vertex1))
					vc.Remove(VertexConstraints[j]);
				}
			VertexConstraints = vc;
			Console.WriteLine("Found " + found + " .... removed " + (found - VertexConstraints.Count) );


		}

		public void ApplyForce(Vec3 force){
			Force = Force + force;
		}
	
		public void PhysicStep(float dt){
		
			Vec3 acc = Force.Mult(1/Mass);
			var pNext = new List<Vec3> ();
			for (var i = 0; i<VertexNow.Count; i++) {

				var pNow = VertexNow [i];
				var pOld = VertexOld [i];
				var colliding = pNow.IsColliding;
				pNow.IsColliding = false;
				pNext.Add (colliding? pNow : pNow * 2 - pOld + acc * dt*dt);
			
			}
			VertexOld = VertexNow;
			VertexNow = pNext;


		}

		public void FixCollisionWithSphere(Vec3 sphereCenter, float sphereRadius){
			var offset = 0.5f;
			var sc = Model.Inverse.Dot (sphereCenter);
			for (var i = 0; i<VertexList.Count; i++) {
			
				var v = VertexList [i];
				var diff = (v - sc);
				var distance = diff.Norm();
				if (distance > sphereRadius+offset) {
					VertexList [i].IsColliding |= false;
					continue;
				}
				v = new Vec3(sc+(diff.Normalized () * (sphereRadius+offset)));
				VertexList[i] = v;
				VertexList [i].IsColliding = true;
			}

		}

		public void FixCollisionWithCapsule(Vec3 pointA, Vec3 pointB, float radius){
			var a = new Vec3(Model.Inverse.Dot (pointA));
			var b = new Vec3(Model.Inverse.Dot (pointB));
			var abNormalized = (b-a).Normalized ();
			var abNorm = (b-a).Norm ();

			for (var i = 0; i<VertexList.Count; i++) {
				var v = VertexList [i];
				var p = a + abNormalized.Mult((v - a).Dot (abNormalized));

				if ((v - p).Norm () > radius)
					continue;

				if ((p - a).Norm () > abNorm + radius || (p - b).Norm () > abNorm + radius)
					continue;
				VertexList [i] = p+(v - p).Normalized () * radius;
				VertexList [i].IsColliding |= true;

			}
		}

		public void ApplyConstraints(){
			foreach (var constraint in VertexConstraints) {
				var a = (Vec3)VertexList [constraint.Vertex0];
				var b = (Vec3)VertexList [constraint.Vertex1];

				var delta = (b - a);
				var currentDistance = delta.Norm();
				var x = currentDistance - constraint.distance;
				//if ( > )
					// continue;

				var d = delta * (0.5f * (x) / currentDistance);
				VertexList [constraint.Vertex0] = a + d;
				VertexList [constraint.Vertex1] = b - d;

				VertexList [constraint.Vertex0].IsColliding = a.IsColliding;
				VertexList [constraint.Vertex1].IsColliding = b.IsColliding;


			}
		}

		public VerletMesh ():base(){
			init ();
		}
		public VerletMesh(Mesh m){

			Faces = m.Faces;
			VertexList = m.VertexList;
			Normals = m.Normals;
			init ();
		}
		void init(){
			VertexConstraints = new List<VertexConstraint> ();
			VertexOld = new List<Vec3> ();
			Force = new Vec3 (0, 0, 0);
			Mass = 0;
			Model = IMatrix.Identity ();
			foreach (var v in VertexNow)
				VertexOld.Add (v * 1);

		}
	}
}

