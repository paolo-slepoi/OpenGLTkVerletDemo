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

		public List<VertexConstraint> VertexConstatints{ get; private set; } //per gestire la mesh
		public List<Vec3> VertexOld { get; private set; }
		public List<Vec3> VertexNow { get { return VertexList; } set { VertexList = value; } } 

		void GenerateVertexConstraintsFromFaces(){
			foreach (var f in Faces) {
				var v0 = f.posIndex [0];
				var v1 = f.posIndex [1];
				var v2 = f.posIndex [2];

				var v01 = VertexList [v1] - VertexList [v0];
				var v02 = VertexList [v2] - VertexList [v0];
				var v12 = VertexList [v2] - VertexList [v1];

				VertexConstatints.Add(new VertexConstraint(){Vertex0 = v0,Vertex1 = v1, distance = v01.Norm()});
				VertexConstatints.Add(new VertexConstraint(){Vertex0 = v0,Vertex1 = v2, distance = v02.Norm()});
				VertexConstatints.Add(new VertexConstraint(){Vertex0 = v1,Vertex1 = v2, distance = v12.Norm()});

			}
		}

		public void ApplyForce(Vec3 force){
			Force = Force + force;
		//	Force.Normalize ();
			Console.WriteLine ("Applied force" + Force.ToString ());
		}
	
		public void UpdatePosition(float dt){
		

			Vec3 acc = Force.Mult(1/Mass);
		//	acc.Normalize ();
			Force.W = Force.W * 1.1f;
			var pNext = new List<Vec3> ();
			for (var i = 0; i<VertexNow.Count; i++) {
				var pNow = VertexNow [i];
				var pOld = VertexOld [i];
				pNext.Add (pNow * 2 - pOld + acc * dt*dt);
			}
			VertexOld = VertexNow;
			VertexNow = pNext;


		}

		public void FixCollisionWithSphere(Vec3 sphereCenter, float sphereRadius,IMatrix model){
			bool collided = false;
			for (var i = 0; i<VertexList.Count; i++) {
				//var v = new Vec3(model.Direct.Dot(VertexList [i]));

				var v = new Vec3(model.Direct.Dot(VertexList [i]));

				//var v = VertexList [i];
				var diff = (v - sphereCenter);
				var distance = diff.Norm();
				if (distance > sphereRadius)
					continue;
				collided = true;
				v = diff.Normalized () * sphereRadius;
				VertexList[i] = new Vec3 (model.Inverse.Dot (v));
				//VertexList [i] = new Vec3( model.Direct.Dot(diff.Normalized ()*sphereRadius));
				//VertexList [i] = diff.Normalized () * sphereRadius;
				//VertexList [i] = diff;
				//VertexList [i] = new Vec3(model.Inverse.Dot(new Vec4 (5, 5, 5,1)));
				//VertexList [i] = v+diff;
				//VertexList [i] = VertexOld [i];
				//VertexList [i] = VertexList [i].Mult(1.1f);
				//VertexList [i] = v.Normalized ();
				//VertexList [i] = diff.Normalized () * distance;
				Console.WriteLine (string.Format(
					"v: {0} -- diff: {1} -- distance:{2} -- sphere center: {3} --- sphere radius : {4}\n --- v+diff : {5}" +
					" --- v normalized : {6} -- diff normalized : {7} -- v norm:{8} -- diff norm : {9}\n" +
					" VList: {10}",
					v,diff,distance,sphereCenter,sphereRadius,(v+diff),v.Normalized(),diff.Normalized(),v.Norm(),diff.Norm(),VertexList[i]
					));
				//System.Environment.Exit (0);
			}
			if (!collided)
				return;

			Force.W = Force.W * 0.5f;
//			if (Force.W < 0.2f)
//				Force = new Vec3 (0, 0, 0);
			Console.WriteLine ("force : " + Force);
			//ApplyConstraints ();
		}

		public void FixZCollision(float z){
			return;
		}



		public void ApplyConstraints(float noise){
			foreach (var constraint in VertexConstatints) {
				var v0 = (Vec3)VertexList [constraint.Vertex0];
				var v1 = (Vec3)VertexList [constraint.Vertex1];

				var delta = (v1 - v0);
				var currentDistance = delta.Norm();
				var diff = (currentDistance - constraint.distance) / currentDistance;

				VertexList [constraint.Vertex0] = v0 + delta * (0.5f * diff*noise);
				VertexList [constraint.Vertex1] = v1 - delta * (0.5f * diff*noise);  

			}
		}
		public void ApplyConstraints(){
			ApplyConstraints (1.0f);
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
			VertexConstatints = new List<VertexConstraint> ();
			VertexOld = new List<Vec3> ();
			Force = new Vec3 (0, 0, 0);
			Mass = 0;
			foreach (var v in VertexNow)
				VertexOld.Add (v * 1);
			GenerateVertexConstraintsFromFaces ();
		}
	}
}
