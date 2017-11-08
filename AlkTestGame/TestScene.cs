using AlkaronEngine.Scene;
using Microsoft.Xna.Framework;
using AlkaronEngine.Gui;
using Microsoft.Xna.Framework.Graphics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Graphics2D;

namespace AlkTestGame
{
   class MyStaticMesh : StaticMesh
   {
      public MyStaticMesh(IRenderConfiguration renderConfig)
         : base(renderConfig)
      {
         VertexBuffer = new VertexBuffer(renderConfig.GraphicsDevice,
                                         VertexPositionColor.VertexDeclaration, 3, BufferUsage.WriteOnly);

         VertexPositionColor vec1 = new VertexPositionColor(new Vector3(-1.5f, 0, 0), Color.Red);
         VertexPositionColor vec2 = new VertexPositionColor(new Vector3(0, 2, 0), Color.Blue);
         VertexPositionColor vec3 = new VertexPositionColor(new Vector3(1.5f, -3, 0), Color.Green);

         VertexBuffer.SetData<VertexPositionColor>(new VertexPositionColor[] { vec1, vec2, vec3 });

         PrimitiveType = PrimitiveType.TriangleList;
         PrimitiveCount = 1;
      }
   }

   class TestScene : BaseScene
   {
      public override Color BackgroundColor
      {
         get
         {
            return Color.CornflowerBlue;
         }
      }

      public TestScene()
      {
      }

      protected override void InitUI()
      {
         base.InitUI();

         MouseCursor = new MouseCursor(Game1.MainGame.SceneManager, 
                                       new Cursor(new AlkaronEngine.Graphics2D.Texture(Game1.MainGame.SceneManager, Game1.MainGame.Content.Load<Texture2D>("cursor"))));

         UIWindow wnd = new UIWindow(Game1.MainGame.SceneManager);
         wnd.Show();
         
         UILabel label = new UILabel(Game1.MainGame.SceneManager, "Hello World!", Game1.MainGame.DefaultFont);
         label.PositionAnchor = UIPositionAnchor.TopRight;
         label.BackgroundColor = Color.Red;
         label.Width = 200;
         label.Height = 20;
         wnd.AddComponent(label);
      }

      protected override void Init3D()
      {
         base.Init3D();

         StaticMeshComponent comp = new StaticMeshComponent(new Vector3(-2, 0, 0));
         comp.StaticMesh = new MyStaticMesh(Game1.MainGame.SceneManager);
         SceneGraph.AddComponent(comp);

         StaticMeshComponent comp2 = new StaticMeshComponent(new Vector3(5, 0, 0));
         comp2.StaticMesh = new StaticMeshCube(Game1.MainGame.SceneManager);
         SceneGraph.AddComponent(comp2);
      }
   }
}
