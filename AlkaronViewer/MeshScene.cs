using AlkaronEngine.Actors;
using AlkaronEngine.Assets.Importers;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Components;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.Geometry;
using AlkaronEngine.Input;
using AlkaronEngine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AlkaronViewer
{

    class MeshScene : BaseScene
    {
        private GltfModelManager modelManager;

        private BaseActor focusActor;

        public MeshScene()
        {
            focusActor = null;

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

            PresentModel("BoxAnimated", true, GltfModelEntryType.Base);
        }

        private void PresentModel(string name, bool isSkeletalMesh, GltfModelEntryType type = GltfModelEntryType.Base)
        {
            string file = modelManager.GetModelPath(name, type);

            string assetName = Path.GetFileNameWithoutExtension(Path.GetFileName(file));

            List<AlkaronEngine.Assets.Asset> importedAssets = null;
            if (isSkeletalMesh)
            {
                AssetImporterSkeletalMesh.Import(file, assetName, assetName, out importedAssets);
            }
            else
            {
                AssetImporterStaticMesh.Import(file, assetName, assetName, out importedAssets);
            }

            for (int i = 0; i < importedAssets.Count; i++)
            {
                if (importedAssets[i] is StaticMesh)
                {
                    SetStaticMesh(importedAssets[i] as StaticMesh);
                    break; 
                }
            }
        }

        protected override void InitUI()
        {
            base.InitUI();
        }

        public void SetStaticMesh(StaticMesh mesh)
        {
            if (focusActor != null &&
                focusActor.IsAddedToSceneGraph)
            {
                SceneGraph.RemoveActor(focusActor);

                focusActor = null;
            }

            if (mesh == null)
            {
                return;
            }

            focusActor = new StaticMeshActor();
            ((StaticMeshActor)focusActor).StaticMeshComponent.AddStaticMesh(mesh);

            SceneGraph.AddActor(focusActor);
        }
    }
}
