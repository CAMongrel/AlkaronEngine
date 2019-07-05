using AlkaronEngine.Actors;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Gui;
using AlkaronEngine.Input;
using AlkaronEngine.Scene;
using System;
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
                baseModelFolder = @"D:\Temp\";
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
        }

        protected override void CreateDefaultCamera()
        {
            var camComponent = new ArcBallCameraComponent(Vector3.Zero, 10.0f, 45.0f, -45.0f, 0.0f, ScreenSize, 0.1f, 500.0f);

            CurrentCamera = new CameraActor(camComponent);

            SceneGraph.AddActor(CurrentCamera);
        }

        protected override void Init3D()
        {
            base.Init3D();

            //AssetImporterMaterial.Import("/Users/henning/Projects/Research/GitHub/SkinnedEffect.dx11.mgfxo", "SkinnedEffect", "EngineMaterials", out var material);
            //AssetImporterMaterial.Import("/Users/henning/Projects/Research/GitHub/SpriteEffect.dx11.mgfxo", "SpriteEffect", "EngineMaterials", out material);

            //var package = MainGame.Instance.PackageManager.LoadPackage("EngineMaterials", false);
            //package.Save();

            //var mat = MainGame.Instance.AssetManager.Load<AlkaronEngine.Assets.Materials.Material>("EngineMaterials.BasicEffect.material");

            //PresentModel("BoxAnimated", true, GltfModelEntryType.Base);
            //PresentModel("Monster", false, GltfModelEntryType.Base);
            
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
            /*string file = modelManager.GetModelPath(name, type);

            string assetName = Path.GetFileNameWithoutExtension(Path.GetFileName(file));

            List<AlkaronEngine.Assets.Asset> importedAssets = null;
            if (isSkeletalMesh)
            {
                //AssetImporterSkeletalMesh.Import(file, assetName, assetName, out importedAssets);
            }
            else
            {
                /*AssetImporterGltfMesh.Import(file, assetName, assetName, (obj) =>
                {
                    Console.WriteLine("Import state: " + obj.State);
                }, out importedAssets);*/
            /*}

            for (int i = 0; i < importedAssets.Count; i++)
            {
                if (importedAssets[i] is StaticMesh)
                {
                    AddStaticMesh(importedAssets[i] as StaticMesh);
                }
            }

            var package = MainGame.Instance.PackageManager.LoadPackage(assetName, false);
            package.Save();
            */
        }

        protected override void InitUI()
        {
            base.InitUI();

            UIWindow window = new UIWindow();
            window.BackgroundColor = RgbaFloat.Red;
            window.Show();
        }

        /*public void AddStaticMesh(StaticMesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            StaticMeshActor focusActor = new StaticMeshActor();
            ((StaticMeshActor)focusActor).StaticMeshComponent.AddStaticMesh(mesh);

            SceneGraph.AddActor(focusActor);
        }*/
    }
}
