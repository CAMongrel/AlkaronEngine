using AlkaronEngine.Actors;
using AlkaronEngine.Assets.Importers;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Input;
using AlkaronEngine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

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
        }

        public override void Close()
        {
            base.Close();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void Init(IRenderConfiguration setRenderConfig)
        {
            base.Init(setRenderConfig);
        }

        public override bool KeyPressed(Keys key, GameTime gameTime)
        {
            return base.KeyPressed(key, gameTime);
        }

        public override bool KeyReleased(Keys key, GameTime gameTime)
        {
            return base.KeyReleased(key, gameTime);
        }

        public override void PointerDown(Vector2 position, PointerType pointerType, GameTime gameTime)
        {
            base.PointerDown(position, pointerType, gameTime);
        }

        public override void PointerMoved(Vector2 position, GameTime gameTime)
        {
            base.PointerMoved(position, gameTime);
        }

        public override void PointerUp(Vector2 position, PointerType pointerType, GameTime gameTime)
        {
            base.PointerUp(position, pointerType, gameTime);
        }

        public override void PointerWheelChanged(Vector2 deltaValue, GameTime gameTime)
        {
            base.PointerWheelChanged(deltaValue, gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void CreateDefaultCamera()
        {
            var camComponent = new ArcBallCameraComponent(Vector3.Zero, 10.0f, 45.0f, -45.0f, 0.0f, RenderConfig.ScreenSize, 0.1f, 500.0f);

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
            
            var package = MainGame.Instance.PackageManager.LoadPackage("test", true);
            var meshes = package.GetAssetsByType("StaticMesh");

            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] is StaticMesh)
                {
                    AddStaticMesh(meshes[i] as StaticMesh);
                }
            }
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
                AssetImporterGltfMesh.Import(file, assetName, assetName, (obj) =>
                {
                    Console.WriteLine("Import state: " + obj.State);
                }, out importedAssets);
            }

            for (int i = 0; i < importedAssets.Count; i++)
            {
                if (importedAssets[i] is StaticMesh)
                {
                    AddStaticMesh(importedAssets[i] as StaticMesh);
                }
            }

            var package = MainGame.Instance.PackageManager.LoadPackage(assetName, false);
            package.Save();
        }

        protected override void InitUI()
        {
            base.InitUI();
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
