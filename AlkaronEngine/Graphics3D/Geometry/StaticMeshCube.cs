using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D.Geometry
{
   public class StaticMeshCube : StaticMesh
   {
      public StaticMeshCube(IRenderConfiguration setRenderConfig)
         : base(setRenderConfig)
      {
         PrimitiveType = PrimitiveType.TriangleList;
         PrimitiveCount = 12;

         VertexPositionColor[] verts = new VertexPositionColor[36];

         // Front
         verts[0] = new VertexPositionColor(new Vector3(-1,  1,  1), Color.Red);
         verts[1] = new VertexPositionColor(new Vector3( 1,  1,  1), Color.Red);
         verts[2] = new VertexPositionColor(new Vector3( 1, -1,  1), Color.Red);
         verts[3] = new VertexPositionColor(new Vector3(-1,  1,  1), Color.Red);
         verts[4] = new VertexPositionColor(new Vector3( 1, -1,  1), Color.Red);
         verts[5] = new VertexPositionColor(new Vector3(-1, -1,  1), Color.Red);
         // Back
         verts[6] = new VertexPositionColor(new Vector3(-1, 1, -1), Color.Blue);
         verts[7] = new VertexPositionColor(new Vector3(1, -1, -1), Color.Blue);
         verts[8] = new VertexPositionColor(new Vector3(1, 1, -1), Color.Blue);
         verts[9] = new VertexPositionColor(new Vector3(-1, 1, -1), Color.Blue);
         verts[10] = new VertexPositionColor(new Vector3(-1, -1, -1), Color.Blue);
         verts[11] = new VertexPositionColor(new Vector3(1, -1, -1), Color.Blue);
         // Left
         verts[12] = new VertexPositionColor(new Vector3(-1, 1, -1), Color.Purple);
         verts[13] = new VertexPositionColor(new Vector3(-1, 1, 1), Color.Purple);
         verts[14] = new VertexPositionColor(new Vector3(-1, -1, 1), Color.Purple);
         verts[15] = new VertexPositionColor(new Vector3(-1, 1, -1), Color.Purple);
         verts[16] = new VertexPositionColor(new Vector3(-1, -1, 1), Color.Purple);
         verts[17] = new VertexPositionColor(new Vector3(-1, -1, -1), Color.Purple);
         // Right
         verts[18] = new VertexPositionColor(new Vector3(1, 1, -1), Color.Indigo);
         verts[19] = new VertexPositionColor(new Vector3(1, -1, 1), Color.Indigo);
         verts[20] = new VertexPositionColor(new Vector3(1, 1, 1), Color.Indigo);
         verts[21] = new VertexPositionColor(new Vector3(1, 1, -1), Color.Indigo);
         verts[22] = new VertexPositionColor(new Vector3(1, -1, -1), Color.Indigo);
         verts[23] = new VertexPositionColor(new Vector3(1, -1, 1), Color.Indigo);
         // Top
         verts[24] = new VertexPositionColor(new Vector3(1, 1, -1), Color.Green);
         verts[25] = new VertexPositionColor(new Vector3(1, 1, 1), Color.Green);
         verts[26] = new VertexPositionColor(new Vector3(-1, 1, 1), Color.Green);
         verts[27] = new VertexPositionColor(new Vector3(1, 1, -1), Color.Green);
         verts[28] = new VertexPositionColor(new Vector3(-1, 1, 1), Color.Green);
         verts[29] = new VertexPositionColor(new Vector3(-1, 1, -1), Color.Green);
         // Bottom
         verts[30] = new VertexPositionColor(new Vector3(1, -1, -1), Color.Yellow);
         verts[31] = new VertexPositionColor(new Vector3(-1, -1, 1), Color.Yellow);
         verts[32] = new VertexPositionColor(new Vector3(1, -1, 1), Color.Yellow);
         verts[33] = new VertexPositionColor(new Vector3(1, -1, -1), Color.Yellow);
         verts[34] = new VertexPositionColor(new Vector3(-1, -1, -1), Color.Yellow);
         verts[35] = new VertexPositionColor(new Vector3(-1, -1, 1), Color.Yellow);

         VertexBuffer = new VertexBuffer(renderConfig.GraphicsDevice,
                                         VertexPositionColor.VertexDeclaration,
                                         36, BufferUsage.WriteOnly);
         VertexBuffer.SetData(verts);
      }
   }
}
