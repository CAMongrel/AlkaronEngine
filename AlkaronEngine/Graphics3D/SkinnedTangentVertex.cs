using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics
{
	/// <summary>
	/// TangentVertex, custom format with postition, normal, tangent and one set
	/// of uv coordinates.
	[StructLayout(LayoutKind.Sequential)]
	public struct SkinnedTangentVertex
	{
		#region Variables
		/// <summary>
		/// Position
		/// </summary>
		public Vector3 Position;
		/// <summary>
		/// Texture coordinates
		/// </summary>
		public Vector2 TexCoord;
		/// <summary>
		/// Tangent
		/// </summary>
		public Vector3 Normal;
		/// <summary>
		/// Normal
		/// </summary>
		public Vector3 Tangent;
		/// <summary>
		/// Tangent
		/// </summary>
		public Vector3 BiTangent;
		/// <summary>
		/// Bone/Joint indices
		/// </summary>
		public Vector4 JointIndices;
		/// <summary>
		/// Bone/Joint weights
		/// </summary>
		public Vector4 JointWeights;
		#endregion

		#region Properties
		/// <summary>
		/// Stride size, in XNA called SizeInBytes. I'm just conforming with that.
		/// </summary>
		public static int SizeInBytes
		{
			get
			{
				// 4 bytes per float:
				// 3 floats pos, 2 floats uv, 3 floats normal, 
				// 3 float tangent, 3 float bitangent, 4 float joint
				// indices and 4 float joint weight
				return 4 * (3 + 2 + 3 + 3 + 3 + 4 + 4);
			} // get
		} // StrideSize
		#endregion

		#region Methods
		#region Constructor
		/// <summary>
		/// Create tangent vertex
		/// </summary>
		/// <param name="setPos">Set position</param>
		/// <param name="setU">Set u texture coordinate</param>
		/// <param name="setV">Set v texture coordinate</param>
		/// <param name="setNormal">Set normal</param>
		/// <param name="setTangent">Set tangent</param>
		public SkinnedTangentVertex(
			Vector3 setPos,
			float setU, float setV,
			Vector3 setNormal,
			Vector3 setTangent,
			Vector3 setBitangent,
			Vector4 setJointIndices,
			Vector4 setJointWeights)
		{
			Position = setPos;
			TexCoord = new Vector2(setU, setV);
			Normal = setNormal;
			Tangent = setTangent;
			BiTangent = setBitangent;
			JointIndices = setJointIndices;
			JointWeights = setJointWeights;
		} // TangentVertex(setPos, setU, setV)

		/// <summary>
		/// Create tangent vertex
		/// </summary>
		/// <param name="setPos">Set position</param>
		/// <param name="setUv">Set uv texture coordinates</param>
		/// <param name="setNormal">Set normal</param>
		/// <param name="setTangent">Set tangent</param>
		public SkinnedTangentVertex(
			Vector3 setPos,
			Vector2 setUv,
			Vector3 setNormal,
			Vector3 setTangent,
			Vector3 setBitangent,
			Vector4 setJointIndices,
			Vector4 setJointWeights)
		{
			Position = setPos;
			TexCoord = setUv;
			Normal = setNormal;
			Tangent = setTangent;
			BiTangent = setBitangent;
			JointIndices = setJointIndices;
			JointWeights = setJointWeights;
		} // TangentVertex(setPos, setUv, setNormal)
		#endregion

		#region To string
		/// <summary>
		/// To string
		/// </summary>
		public override string ToString()
		{
			return "SkinnedTangentVertex(pos=" + Position + ", " +
				"u=" + TexCoord.X + ", " +
				"v=" + TexCoord.Y + ", " +
				"normal=" + Normal + ", " +
				"tangent=" + Tangent + ", " +
				"bitangent=" + BiTangent + ", " +
				"jointIndices=" + JointIndices + ", " +
				"jointWeights=" + JointWeights + ")";
		} // ToString()
		#endregion

		#region Generate vertex declaration
		/// <summary>
		/// Vertex elements for Mesh.Clone
		/// </summary>
		public static readonly VertexDeclaration VertexDecl =
			GenerateVertexDeclaration();

		/// <summary>
		/// Generate vertex declaration
		/// </summary>
		private static VertexDeclaration GenerateVertexDeclaration()
		{
			VertexElement[] decl = new VertexElement[]
				{
					new VertexElement(0, VertexElementFormat.Vector3, 
											VertexElementUsage.Position, 0),
			                            
					new VertexElement(12, VertexElementFormat.Vector2,
											 VertexElementUsage.TextureCoordinate, 0),
				                            
					new VertexElement(20, VertexElementFormat.Vector3,
											 VertexElementUsage.Normal, 0),
				                            
					new VertexElement(32, VertexElementFormat.Vector3,
											 VertexElementUsage.Tangent, 0),

					new VertexElement(44, VertexElementFormat.Vector3,
											 VertexElementUsage.Binormal, 0),
											 
					new VertexElement(56, VertexElementFormat.Vector4,
                                          VertexElementUsage.BlendIndices, 0),
					
					new VertexElement(72, VertexElementFormat.Vector4,
 										  VertexElementUsage.BlendWeight, 0),
				};

            return new VertexDeclaration(decl);
		} // GenerateVertexElements()
		#endregion
		#endregion

		#region Nearly equal
		/// <summary>
		/// Returns true if two vertices are nearly equal. For example the
		/// tangent or normal data does not have to match 100%.
		/// Used to optimize vertex buffers and to generate indices.
		/// </summary>
		/// <param name="a">A</param>
		/// <param name="b">B</param>
		/// <returns>Bool</returns>
		public static bool NearlyEquals(SkinnedTangentVertex a,
			SkinnedTangentVertex b)
		{
			//SkinningWithColladaModelsInXna.Helpers.Log.Write("Compare a=" + a.pos + ", " + a.uv + ", "+a.normal+
			//  " with b=" + b.pos + ", " + b.uv+ ", "+b.normal);
			//return false;
			// Position has to match, else it is just different vertex
			return a.Position == b.Position &&
				// Ignore blend indices and blend weights, they are the same
				// anyway, because they are calculated from the bone distances.
				Math.Abs(a.TexCoord.X - b.TexCoord.X) < 0.001f &&
				Math.Abs(a.TexCoord.Y - b.TexCoord.Y) < 0.001f &&
				// Normals and tangents do not have to be very close, we can't see
				// any difference between small variations here, but by optimizing
				// similar vertices we can improve the overall rendering performance.
				(a.Normal - b.Normal).Length() < 0.1f &&
				(a.Tangent - b.Tangent).Length() < 0.1f;
			//SkinningWithColladaModelsInXna.Helpers.Log.Write("ret=" + ret);
			//			return ret;
		} // NearlyEqual(a, b)
		#endregion
	}
}
