using AlkaronEngine.Actors;
using AlkaronEngine.Assets.Importers;
using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Gui;
using AlkaronEngine.Input;
using AlkaronEngine.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Veldrid;

namespace AlkaronViewer
{
    class MeshScene : BaseScene
    {
        private GltfModelManager modelManager;

        //private BaseActor focusActor;

        public MeshScene()
        {
            string baseModelFolder = "";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                baseModelFolder = @"D:\Projekte\Henning\GitHub\glTF-Sample-Models\2.0\";
            }
            else
            {
                baseModelFolder = @"/Users/henning/Projects/Research/GitHub/glTF-Sample-Models/2.0/";
            }

            modelManager = new GltfModelManager(baseModelFolder);
            modelManager.BuildModelList();

            ClearColor = RgbaFloat.CornflowerBlue;
        }

        public override void Close()
        {
            base.Close();
        }

        public override void Draw(double deltaTime, RenderContext renderContext)
        {
            base.Draw(deltaTime, renderContext);
        }

        public override bool KeyPressed(Key key, double deltaTime)
        {
            return base.KeyPressed(key, deltaTime);
        }

        public override bool KeyReleased(Key key, double deltaTime)
        {
            return base.KeyReleased(key, deltaTime);
        }

        public override void PointerDown(Vector2 position, PointerType pointerType, double deltaTime)
        {
            base.PointerDown(position, pointerType, deltaTime);
        }

        public override void PointerMoved(Vector2 position, double deltaTime)
        {
            base.PointerMoved(position, deltaTime);
        }

        public override void PointerUp(Vector2 position, PointerType pointerType, double deltaTime)
        {
            base.PointerUp(position, pointerType, deltaTime);
        }

        public override void PointerWheelChanged(Vector2 deltaValue, double deltaTime)
        {
            base.PointerWheelChanged(deltaValue, deltaTime);
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            label.Text = "Camera Position: " + CurrentCamera.CameraComponent.Center;
        }

        protected override void CreateDefaultCamera()
        {
            var camComponent = new ArcBallCameraComponent(Vector3.Zero, 100.0f, 45.0f, -45.0f, 0.0f, ScreenSize, 0.1f, 5000.0f);

            CurrentCamera = new CameraActor(camComponent);

            SceneGraph.AddActor(CurrentCamera);
        }

        protected override void Init3D()
        {
            base.Init3D();

            StaticMesh box = StaticMesh.FromVertices(new Vector3[] {
                new Vector3(-1, -1, 0),
                new Vector3(-1,  5, 0),
                new Vector3( 1, -1, 0),

                new Vector3( 1, -1, 0),
                new Vector3(-1,  5, 0),
                new Vector3( 1,  5, 0),
            }, MainGame.Instance.GraphicsDevice, false);
            //AddStaticMesh(box);

            //AssetImporterMaterial.Import("/Users/henning/Projects/Research/GitHub/SkinnedEffect.dx11.mgfxo", "SkinnedEffect", "EngineMaterials", out var material);
            //AssetImporterMaterial.Import("/Users/henning/Projects/Research/GitHub/SpriteEffect.dx11.mgfxo", "SpriteEffect", "EngineMaterials", out material);

            //var package = MainGame.Instance.PackageManager.LoadPackage("EngineMaterials", false);
            //package.Save();

            //var mat = MainGame.Instance.AssetManager.Load<AlkaronEngine.Assets.Materials.Material>("EngineMaterials.BasicEffect.material");

            //PresentModel("BoxAnimated", false, GltfModelEntryType.Base);
            //PresentModel("2CylinderEngine", false, GltfModelEntryType.Base);
            //PresentModel("Suzanne", false, GltfModelEntryType.Base);
            //PresentModel("NormalTangentTest", false, GltfModelEntryType.Base);
            //PresentModel("SciFiHelmet", false, GltfModelEntryType.Base);
            //PresentModel("MetalRoughSpheres", false, GltfModelEntryType.Base);
            //PresentModel("WaterBottle", false, GltfModelEntryType.Base);
            //PresentModel("Sponza", false, GltfModelEntryType.Base);
            PresentModel("AlphaBlendModeTest", false, GltfModelEntryType.Base);

            /*var package = MainGame.Instance.PackageManager.LoadPackage("test", true);
            var meshes = package.GetAssetsByType("StaticMesh");

            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] is StaticMesh)
                {
                    AddStaticMesh(meshes[i] as StaticMesh);
                }
            }*/
        }

        private void PresentModel(string name, bool isSkeletalMesh, GltfModelEntryType type = GltfModelEntryType.Base)
        {
            string file = modelManager.GetModelPath(name, type);

            string assetName = Path.GetFileNameWithoutExtension(Path.GetFileName(file));

            List<AlkaronEngine.Assets.Asset> importedAssets = null;
            if (isSkeletalMesh)
            {
                //AssetImporterSkeletalMesh.Import(file, assetName, assetName, out importedAssets);
            }
            else
            {
                AssetImporterGltfMesh.Import(file, assetName, assetName, true, (obj) =>
                {
                    Console.WriteLine("Import state: " + obj.State);
                }, MainGame.Instance.AssetManager.AssetSettings, out importedAssets);
            }

            for (int i = 0; i < importedAssets.Count; i++)
            {
                if (importedAssets[i] is StaticMesh)
                {
                    AddStaticMesh(importedAssets[i] as StaticMesh);
                }
            }

            /*var package = MainGame.Instance.PackageManager.LoadPackage(assetName, false);
            package.Save();*/
        }

        private UILabel label;

        protected override void InitUI()
        {
            base.InitUI();

            UIWindow window = new UIWindow();
            window.Show();

            AssetImporterSurface2D.Import("d:\\temp\\image.jpg", null, null, MainGame.Instance.AssetManager.AssetSettings, out var tex);

            UIImage image = new UIImage(tex);
            image.X = 50;
            image.Y = 50;
            image.Alpha = 1.0f;
            image.Width = 200;
            image.Height = 200;
            window.AddComponent(image);

            image = new UIImage(tex);
            image.X = 10;
            image.Y = 10;
            image.Alpha = 0.5f;
            image.Width = 200;
            image.Height = 200;
            window.AddComponent(image);

            label = new UILabel("Hello World!");
            label.X = 0;
            label.Y = 0;
            label.Width = 200;
            label.Height = 20;
            label.TextAlignHorizontal = UITextAlignHorizontal.Left;
            label.ForegroundColor = RgbaFloat.Yellow;
            window.AddComponent(label);
        }

        public void AddStaticMesh(StaticMesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            StaticMeshActor focusActor = new StaticMeshActor();
            ((StaticMeshActor)focusActor).StaticMeshComponent.AddStaticMesh(mesh);

            SceneGraph.AddActor(focusActor);
        }
    }
}
