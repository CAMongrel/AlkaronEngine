#region Using directives
using System;
using System.Runtime.InteropServices;
using System.Numerics;
using Veldrid;
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
            Vector3 setTangent)
        {
            Position = setPos;
            TexCoord = new Vector2(setU, setV);
            Normal = setNormal;
            Tangent = setTangent;
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
            Vector3 setTangent)
        {
            Position = setPos;
            TexCoord = setUv;
            Normal = setNormal;
            Tangent = setTangent;
        } // TangentVertex(setPos, setUv, setNormal)
        #endregion

        #region To string
        /// <summary>
        /// To string
        /// </summary>
        public override string ToString()
        {
            return "TangentVertex(pos=" + Position + ", " +
                "u=" + TexCoord.X + ", " +
                "v=" + TexCoord.Y + ", " +
                "normal=" + Normal + ", " +
                "tangent=" + Tangent + ")";
        } // ToString()
        #endregion

        #region Generate vertex declaration
        /// <summary>
        /// Vertex elements for Mesh.Clone
        /// </summary>
        public static readonly VertexLayoutDescription VertexLayout =
            GenerateVertexLayoutDescription();

        /// <summary>
        /// Generate vertex declaration
        /// </summary>
        private static VertexLayoutDescription GenerateVertexLayoutDescription()
        {
            VertexLayoutDescription result = new VertexLayoutDescription(new VertexElementDescription[]
            {
                new VertexElementDescription("Position", VertexElementFormat.Float3, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("TexCoord", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("Normal", VertexElementFormat.Float3, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("Tangent", VertexElementFormat.Float3, VertexElementSemantic.TextureCoordinate),
            });

            return result;
        } // GenerateVertexElements()
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
            int setTangentIndex, int setTexCoordIndex)
        {
            positionIndex = setPositionIndex;
            normalIndex = setNormalIndex;
            tangentIndex = setTangentIndex;
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
