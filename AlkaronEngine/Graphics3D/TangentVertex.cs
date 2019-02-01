#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace AlkaronEngine.Graphics
{
	/// <summary>
	/// TangentVertex, custom format with postition, normal, tangent and one set
	/// of uv coordinates.
	[StructLayout(LayoutKind.Sequential)]
	public struct TangentVertex
	{
		#region Variables
		/// <summary>
		/// Position
		/// </summary>
		public Vector3 pos;
		/// <summary>
		/// Texture coordinates
		/// </summary>
		public Vector2 uv;
		/// <summary>
		/// Tangent
		/// </summary>
		public Vector3 normal;
		/// <summary>
		/// Normal
		/// </summary>
		public Vector3 tangent;
		/// <summary>
		/// Tangent
		/// </summary>
		public Vector3 bitangent;
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
				// 3 floats pos, 2 floats uv, 3 floats normal, 3 float tangent and 3 float bitangent
				return 4 * (3 + 2 + 3 + 3 + 3);
			} // get
		} // StrideSize

		/// <summary>
		/// U texture coordinate
		/// </summary>
		/// <returns>Float</returns>
		public float U
		{
			get
			{
				return uv.X;
			} // get
		} // U

		/// <summary>
		/// V texture coordinate
		/// </summary>
		/// <returns>Float</returns>
		public float V
		{
			get
			{
				return uv.Y;
			} // get
		} // V
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
		public TangentVertex(
			Vector3 setPos,
			float setU, float setV,
			Vector3 setNormal,
			Vector3 setTangent,
			Vector3 setBitangent)
		{
			pos = setPos;
			uv = new Vector2(setU, setV);
			normal = setNormal;
			tangent = setTangent;
			bitangent = setBitangent;
		} // TangentVertex(setPos, setU, setV)

		/// <summary>
		/// Create tangent vertex
		/// </summary>
		/// <param name="setPos">Set position</param>
		/// <param name="setUv">Set uv texture coordinates</param>
		/// <param name="setNormal">Set normal</param>
		/// <param name="setTangent">Set tangent</param>
		public TangentVertex(
			Vector3 setPos,
			Vector2 setUv,
			Vector3 setNormal,
			Vector3 setTangent,
			Vector3 setBitangent)
		{
			pos = setPos;
			uv = setUv;
			normal = setNormal;
			tangent = setTangent;
			bitangent = setBitangent;
		} // TangentVertex(setPos, setUv, setNormal)
		#endregion

		#region To string
		/// <summary>
		/// To string
		/// </summary>
		public override string ToString()
		{
			return "TangentVertex(pos=" + pos + ", " +
				"u=" + uv.X + ", " +
				"v=" + uv.Y + ", " +
				"normal=" + normal + ", " +
				"tangent=" + tangent + ", " +
				"bitangent=" + bitangent + ")";
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
											 VertexElementUsage.Binormal, 1)
				};

			return new VertexDeclaration(decl);
		} // GenerateVertexElements()
		#endregion

		#region Is declaration tangent vertex declaration
		/*
		/// <summary>
		/// Returns true if declaration is tangent vertex declaration.
		/// </summary>
		public static bool IsTangentVertexDeclaration(
			InputElement[] declaration)
		{
			return
				declaration.Length == 4 &&
				declaration[0].SemanticName == "position" &&
				declaration[1].SemanticName == "normal" &&
				declaration[2].SemanticName == "tangent" &&
				declaration[3].SemanticName == "texcoord";
		} // IsTangentVertexDeclaration(declaration)
		*/
		#endregion
		#endregion
	}

	/// <summary>
	/// Index vertex keeps for each element an index, can be used to identify
	/// vertices without float comparisons.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct IndexVertex : IComparable<IndexVertex>
	{
		#region Variables
		public int positionIndex;
		public int normalIndex;
		public int tangentIndex;
        public int bitangentIndex;
		public int texCoordIndex;
		#endregion

		#region Constructor
		/// <summary>
		/// Construct a vertex
		/// </summary>
		/// <param name="setPositionIndex"></param>
		/// <param name="setNormalIndex"></param>
		/// <param name="setTangentIndex"></param>
		/// <param name="setTexCoordIndex"></param>
		public IndexVertex(int setPositionIndex, int setNormalIndex,
			int setTangentIndex, int setBitangentIndex, int setTexCoordIndex)
		{
			positionIndex = setPositionIndex;
			normalIndex = setNormalIndex;
			tangentIndex = setTangentIndex;
			bitangentIndex = setBitangentIndex;
			texCoordIndex = setTexCoordIndex;
		}
		#endregion

		#region Methods
		public static bool operator ==(IndexVertex v1, IndexVertex v2)
		{
			return v1.positionIndex == v2.positionIndex &&
				v1.normalIndex == v2.normalIndex &&
				//v1.tangentIndex == v2.tangentIndex &&
				v1.texCoordIndex == v2.texCoordIndex;
		}

		public static bool operator !=(IndexVertex v1, IndexVertex v2)
		{
			return !(v1 == v2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public int CompareTo(IndexVertex other)
		{
			if (positionIndex != other.positionIndex)
				return positionIndex < other.positionIndex ? -1 : 1;
			if (normalIndex != other.normalIndex)
				return normalIndex < other.normalIndex ? -1 : 1;
			//if(tangentIndex != other.tangentIndex)
			//  return tangentIndex < other.tangentIndex ? -1 : 1;
			if (texCoordIndex != other.texCoordIndex)
				return texCoordIndex < other.texCoordIndex ? -1 : 1;
			return 0;
		}
		#endregion
	}
} // namespace WornEdges.Engine.Model
