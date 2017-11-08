using System;
using AlkaronEngine.Graphics2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkaronEngine.Graphics3D
{
   public class StaticMeshCube : StaticMesh
   {
      public StaticMeshCube(IRenderConfiguration setRenderConfig)
         : base(setRenderConfig)
      {
         PrimitiveType = PrimitiveType.TriangleList;
         PrimitiveCount = 4;

         VertexPositionColor[] verts = new VertexPositionColor[12];

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

         VertexBuffer = new VertexBuffer(renderConfig.GraphicsDevice,
                                         VertexPositionColor.VertexDeclaration,
                                         12, BufferUsage.WriteOnly);
         VertexBuffer.SetData(verts);
      }
   }
}
