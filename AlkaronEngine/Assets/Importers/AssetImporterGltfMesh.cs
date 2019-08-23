using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Graphics;
using AlkaronEngine.Graphics3D;
using AlkaronEngine.Util;
using glTFLoader.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using static AlkaronEngine.Assets.Meshes.SkeletalMesh;

namespace AlkaronEngine.Assets.Importers
{
    public class AssetImporterGltfMeshProgress
    {
        public string State;
    }

    public static class AssetImporterGltfMesh
    {
        class AssetImporterGltfMeshContext
        {
            internal string BaseFolder;
            internal string FullFilename;
            internal string BaseAssetName;
            internal Package PackageToSaveIn = null;

            internal List<Surface2D> ImportedSurfaces = new List<Surface2D>();
            internal List<Materials.Material> ImportedMaterials = new List<Materials.Material>();
            internal List<MeshAsset> ImportedMeshes = new List<MeshAsset>();

            internal bool ImportStaticMeshOnly;
            internal bool ImportAsSkeletalMesh;
            internal AssetSettings AssetSettings;
            internal int MeshCounter;

            internal Gltf Model;
            internal List<byte[]> RawBuffers = new List<byte[]>();

            internal List<SkeletalAnimation> Animations = new List<SkeletalAnimation>();
            internal List<RuntimeBone[]> BonesList = new List<RuntimeBone[]>();

            internal Action<AssetImporterGltfMeshProgress> ProgressCallback;
        }

        private static readonly string[] allowedExtensions = new string[] { ".gltf", ".glb" };

        public static bool Import(string fullFilename,
            string setBaseAssetName,
            string setPackageName,
            bool importStaticMeshOnly,
            Action<AssetImporterGltfMeshProgress> progressCallback,
            AssetSettings assetSettings,
            out List<Asset> importedAssets)
        {
            importedAssets = new List<Asset>();

            // Remember input filename
            string inputFile = fullFilename;
            if (File.Exists(inputFile) == false)
            {
                Log.Status("Import file '" + inputFile + "' is not valid!");
                return false;
            }

            string? extension = Path.GetExtension(inputFile);
            if (string.IsNullOrWhiteSpace(extension))
            {
                Log.Status("Import file '" + inputFile + "' has an invalid file extension!");
                return false;
            }

            extension = extension.ToLowerInvariant();
            if (allowedExtensions.Contains(extension) == false)
            {
                Log.Status("Import file '" + inputFile + "' has an invalid file extension!");
                return false;
            }

            // Create context
            AssetImporterGltfMeshContext context = new AssetImporterGltfMeshContext();
            context.ProgressCallback = progressCallback;
            context.FullFilename = fullFilename;
            context.BaseAssetName = setBaseAssetName;
            context.BaseFolder = Path.GetDirectoryName(fullFilename) ?? AlkaronCoreGame.Core.ContentDirectory;
            context.ImportStaticMeshOnly = importStaticMeshOnly;
            context.ImportAsSkeletalMesh = !context.ImportStaticMeshOnly;
            context.AssetSettings = assetSettings;
            context.MeshCounter = 0;

            string packageName = setPackageName;

            if (AlkaronCoreGame.Core.PackageManager.DoesPackageExist(packageName))
            {
                context.PackageToSaveIn = AlkaronCoreGame.Core.PackageManager.LoadPackage(packageName, false, assetSettings);
            }
            else
            {
                context.PackageToSaveIn = AlkaronCoreGame.Core.PackageManager.CreatePackage(
                    packageName,
                    Path.Combine(AlkaronCoreGame.Core.ContentDirectory, packageName), 
                    assetSettings);
            }

            if (context.PackageToSaveIn == null)
            {
                Log.Status("Unable to create or find the package for this asset");
                return false;
            }

            try
            {
                if (extension == ".gltf" ||
                    extension == ".glb")
                {
                    ImportGLTFFile(context);

                    importedAssets.AddRange(context.ImportedSurfaces);
                    importedAssets.AddRange(context.ImportedMaterials);
                    importedAssets.AddRange(context.ImportedMeshes);
                }
                else
                {
                    throw new NotImplementedException("Import for mesh with extension '" + extension + "' is not implemented.");
                }
            }
            catch (Exception ex)
            {
                Log.Status("Failed to import Mesh: " + ex);
                return false;
            }

            return true;
        }

        #region ImportGLTFFile
        private static void ReportProgress(AssetImporterGltfMeshContext context, string state)
        {
            context.ProgressCallback?.Invoke(new AssetImporterGltfMeshProgress() { State = state });
        }

        private static void ImportGLTFFile(AssetImporterGltfMeshContext context) //string fullFilename, string assetName, Package packageToSaveIn, List<Asset> importedAssets)
        {
            ReportProgress(context, "Deserializing GLTF model");

            context.Model = glTFLoader.Interface.LoadModel(context.FullFilename);

            ReportProgress(context, "Loading buffers");

            // Load all binary referenced buffers (vertices, indices, animations, etc.)
            LoadBuffers(context);

            ReportProgress(context, "Loading textures");

            // Import all textures as Surface2D assets
            LoadTextures(context);

            ReportProgress(context, "Loading materials");

            // Import all materials
            LoadMaterials(context);

            // Import all skins
            if (context.ImportAsSkeletalMesh)
            {
                LoadAnimations(context);

                LoadSkins(context);
            }

            // Load all scenes
            LoadScenes(context);

            ReportProgress(context, "Finished");
        }

        private static void LoadAnimations(AssetImporterGltfMeshContext context)
        {
            for (int i = 0; i < context.Model.Animations.Length; i++)
            {
                Animation animation = context.Model.Animations[i];

                var res = LoadAnimation(context, animation);
                context.Animations.Add(res);
            }
        }

        class AnimationFrame
        {
            public Vector3 translation = Vector3.Zero;
            public Quaternion rotation = Quaternion.Identity;
            public Vector3 scale = Vector3.One;
            public Matrix4x4 transformationMat = Matrix4x4.Identity;

            public Matrix4x4 Matrix =>
                Matrix4x4.CreateScale(scale) *
                Matrix4x4.CreateFromQuaternion(rotation) *
                Matrix4x4.CreateTranslation(translation);
        }

        class AnimationBoneData
        {
            public int nodeIdx;

            public AnimationFrame[] Frames = null;
        }

        class SkeletalAnimation
        {
            public List<AnimationBoneData> Bones = new List<AnimationBoneData>();
        }

        private static SkeletalAnimation LoadAnimation(AssetImporterGltfMeshContext context, Animation animation)
        {
            SkeletalAnimation result = new SkeletalAnimation();

            for (int c = 0; c < animation.Channels.Length; c++)
            {
                AnimationChannel channel = animation.Channels[c];
                AnimationSampler sampler = animation.Samplers[channel.Sampler];

                var inputAccessor = GetAccessorByIndex(sampler.Input, context.Model);
                var outputAccessor = GetAccessorByIndex(sampler.Output, context.Model);

                BufferView? inputBufferView = null;
                if (inputAccessor != null &&
                    inputAccessor.BufferView != null)
                {
                    inputBufferView = context.Model.BufferViews[inputAccessor.BufferView.Value];
                }

                BufferView? outputBufferView = null;
                if (outputAccessor != null &&
                    outputAccessor.BufferView != null)
                {
                    outputBufferView = context.Model.BufferViews[outputAccessor.BufferView.Value];
                }

                ReadOnlySpan<float> inputSpan = null;
                if (inputBufferView != null)
                {
                    inputSpan = MemoryMarshal.Cast<byte, float>(
                        new ReadOnlySpan<byte>(
                            context.RawBuffers[inputBufferView.Buffer], 
                            inputBufferView.ByteOffset + inputAccessor.ByteOffset, inputAccessor.Count * 4));
                }

                int nodeIdx = channel.Target.Node.Value;
                AnimationChannelTarget.PathEnum path = channel.Target.Path;

                AnimationBoneData helper = (from h in result.Bones
                                          where h.nodeIdx == nodeIdx
                                          select h).FirstOrDefault();

                if (helper == null)
                {
                    helper = new AnimationBoneData()
                    {
                        nodeIdx = nodeIdx
                    };
                    helper.Frames = new AnimationFrame[inputSpan.Length];

                    result.Bones.Add(helper);
                }

                switch (outputAccessor.Type)
                {
                    case Accessor.TypeEnum.VEC3:
                        {
                            ReadOnlySpan<Vector3> outputSpan = null;
                            if (outputBufferView != null)
                            {
                                outputSpan = MemoryMarshal.Cast<byte, Vector3>(
                                    new ReadOnlySpan<byte>(
                                        context.RawBuffers[outputBufferView.Buffer],
                                        outputBufferView.ByteOffset + outputAccessor.ByteOffset, outputAccessor.Count * 12));
                            }

                            for (int idx = 0; idx < inputSpan.Length; idx++)
                            {
                                if (helper.Frames[idx] == null)
                                {
                                    helper.Frames[idx] = new AnimationFrame();
                                }

                                if (path == AnimationChannelTarget.PathEnum.translation)
                                {
                                    helper.Frames[idx].translation = outputSpan[idx];
                                }
                                if (path == AnimationChannelTarget.PathEnum.scale)
                                {
                                    helper.Frames[idx].scale = outputSpan[idx];
                                }
                            }
                        }
                        break;

                    case Accessor.TypeEnum.VEC4:
                        {
                            ReadOnlySpan<Vector4> outputSpan = null;
                            if (outputBufferView != null)
                            {
                                outputSpan = MemoryMarshal.Cast<byte, Vector4>(
                                    new ReadOnlySpan<byte>(
                                        context.RawBuffers[outputBufferView.Buffer],
                                        outputBufferView.ByteOffset + outputAccessor.ByteOffset, outputAccessor.Count * 16));

                                for (int idx = 0; idx < inputSpan.Length; idx++)
                                {
                                    if (helper.Frames[idx] == null)
                                    {
                                        helper.Frames[idx] = new AnimationFrame();
                                    }

                                    if (path == AnimationChannelTarget.PathEnum.rotation)
                                    {
                                        helper.Frames[idx].rotation = 
                                            new Quaternion(outputSpan[idx].X, outputSpan[idx].Y, outputSpan[idx].Z, outputSpan[idx].W);
                                    }
                                }
                            }
                        }
                        break;

                    case Accessor.TypeEnum.MAT4:
                        {
                            ReadOnlySpan<Matrix4x4> outputSpan = null;
                            if (outputBufferView != null)
                            {
                                outputSpan = MemoryMarshal.Cast<byte, Matrix4x4>(
                                    new ReadOnlySpan<byte>(
                                        context.RawBuffers[outputBufferView.Buffer],
                                        outputBufferView.ByteOffset + outputAccessor.ByteOffset, outputAccessor.Count * 4 * 16));

                                for (int idx = 0; idx < inputSpan.Length; idx++)
                                {
                                    if (helper.Frames[idx] == null)
                                    {
                                        helper.Frames[idx] = new AnimationFrame();
                                    }

                                    if (path == AnimationChannelTarget.PathEnum.weights)
                                    {
                                        helper.Frames[idx].transformationMat = outputSpan[idx];
                                    }
                                }
                            }
                        }
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            return result;
        }

        private static void LoadMaterials(AssetImporterGltfMeshContext context)
        {
            for (int i = 0; i < context.Model.Materials.Length; i++)
            {
                glTFLoader.Schema.Material mat = context.Model.Materials[i];

                CreateMaterial(mat, context);
            }
        }

        private static void CreateMaterial(glTFLoader.Schema.Material mat, AssetImporterGltfMeshContext context)
        {
            if (mat.PbrMetallicRoughness == null)
            {
                throw new NotImplementedException();
            }

            var pbr = mat.PbrMetallicRoughness;

            Materials.Material result = new Materials.Material();

            string extension = "Static";
            Shader vertexShader = AlkaronCoreGame.Core.ShaderManager.StaticMeshVertexShader;
            if (context.ImportAsSkeletalMesh)
            {
                extension = "Skeletal";
                vertexShader = AlkaronCoreGame.Core.ShaderManager.SkeletalMeshVertexShader;
            }

            ConstructedShader constructedShader = new ConstructedShader(
                context.BaseAssetName + "_Material_" + mat.Name + "_" + extension,
                vertexShader,
                context.ImportAsSkeletalMesh);
            switch (mat.AlphaMode)
            {
                case glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE:
                    constructedShader.BlendMode = BlendMode.Opaque;
                    break;
                case glTFLoader.Schema.Material.AlphaModeEnum.MASK:
                    constructedShader.BlendMode = BlendMode.Mask;
                    constructedShader.AlphaCutoff = mat.AlphaCutoff;
                    break;
                case glTFLoader.Schema.Material.AlphaModeEnum.BLEND:
                    constructedShader.BlendMode = BlendMode.Blend;
                    break;
            }

            // Albedo
            if (pbr.BaseColorTexture != null)
            {
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "Albedo",
                    Type = ConstructedShaderInputType.DiffuseAlbedo,
                    Value = (Surface2D)context.ImportedSurfaces[pbr.BaseColorTexture.Index],
                    ValueType = ConstructedShaderInputValueType.Texture
                });
            }
            else
            {
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "Albedo",
                    Type = ConstructedShaderInputType.DiffuseAlbedo,
                    Value = pbr.BaseColorFactor,
                    ValueType = ConstructedShaderInputValueType.ConstantValue
                });
            }

            // Metallic/Roughness
            if (pbr.MetallicRoughnessTexture != null)
            {
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "MetallicRoughness",
                    Type = ConstructedShaderInputType.MetallicRoughnessCombined,
                    Value = (Surface2D)context.ImportedSurfaces[pbr.MetallicRoughnessTexture.Index],
                    ValueType = ConstructedShaderInputValueType.Texture
                });
            }
            else
            {
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "Metallic",
                    Type = ConstructedShaderInputType.Metallic,
                    Value = pbr.MetallicFactor,
                    ValueType = ConstructedShaderInputValueType.ConstantValue
                });
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "Roughness",
                    Type = ConstructedShaderInputType.Roughness,
                    Value = pbr.RoughnessFactor,
                    ValueType = ConstructedShaderInputValueType.ConstantValue
                });
            }

            // Normal Map
            if (mat.NormalTexture != null)
            {
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "Normal",
                    Type = ConstructedShaderInputType.Normal,
                    Value = (Surface2D)context.ImportedSurfaces[mat.NormalTexture.Index],
                    ValueType = ConstructedShaderInputValueType.Texture
                });
            }

            // AmbientOcclusion
            if (mat.OcclusionTexture != null)
            {
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "AmbientOcclusion",
                    Type = ConstructedShaderInputType.AmbientOcclusion,
                    Value = (Surface2D)context.ImportedSurfaces[mat.OcclusionTexture.Index],
                    ValueType = ConstructedShaderInputValueType.Texture
                });
            }

            // Emissive
            if (mat.EmissiveTexture != null)
            {
                constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                {
                    Name = "Emissive",
                    Type = ConstructedShaderInputType.Emissive,
                    Value = (Surface2D)context.ImportedSurfaces[mat.EmissiveTexture.Index],
                    ValueType = ConstructedShaderInputValueType.Texture
                });
            }
            else
            {
                if (mat.EmissiveFactor != null)
                {
                    constructedShader.Inputs.Elements.Add(new ConstructedShaderInputElement()
                    {
                        Name = "Emissive",
                        Type = ConstructedShaderInputType.Emissive,
                        Value = mat.EmissiveFactor,
                        ValueType = ConstructedShaderInputValueType.ConstantValue
                    });
                }
            }

            result.LoadFromConstructedShader(constructedShader);

            context.ImportedMaterials.Add(result);
        }

        private static void LoadScenes(AssetImporterGltfMeshContext context)
        {
            for (int i = 0; i < context.Model.Scenes.Length; i++)
            {
                ReportProgress(context, "Loading scene #" + i + " of " + context.Model.Scenes.Length);

                // Load all nodes
                for (int j = 0; j < context.Model.Scenes[i].Nodes.Length; j++)
                {
                    var node = context.Model.Nodes[context.Model.Scenes[i].Nodes[j]];

                    LoadNode(context, node, Matrix4x4.Identity);
                }
            }
        }

        private static Matrix4x4 GetWorldMatrix(Node node)
        {
            Matrix4x4 resultMatrix = Matrix4x4.Identity;
            if (node.Matrix != null)
            {
                resultMatrix *= new Matrix4x4(node.Matrix[ 0], node.Matrix[ 1], node.Matrix[ 2], node.Matrix[ 3],
                                              node.Matrix[ 4], node.Matrix[ 5], node.Matrix[ 6], node.Matrix[ 7],
                                              node.Matrix[ 8], node.Matrix[ 9], node.Matrix[10], node.Matrix[11],
                                              node.Matrix[12], node.Matrix[13], node.Matrix[14], node.Matrix[15]);
            }

            if (node.Scale != null)
            {
                Matrix4x4 scaleMat = Matrix4x4.CreateScale(node.Scale[0], node.Scale[1], node.Scale[2]);
                resultMatrix *= scaleMat;
            }

            if (node.Rotation != null)
            {
                Matrix4x4 rotationMat = Matrix4x4.CreateFromQuaternion(new Quaternion(node.Rotation[0], node.Rotation[1], node.Rotation[2], node.Rotation[3]));
                resultMatrix *= rotationMat;
            }

            if (node.Translation != null)
            {
                Matrix4x4 translationMat = Matrix4x4.CreateTranslation(node.Translation[0], node.Translation[1], node.Translation[2]);
                resultMatrix *= translationMat;
            }

            return resultMatrix;
        }

        private static void LoadNode(AssetImporterGltfMeshContext context, Node node, Matrix4x4 parentMatrix)
        {
            Matrix4x4 worldMatrix = GetWorldMatrix(node) * parentMatrix;

            if (node.Camera.HasValue)
            {
                LoadCamera(context, node.Camera.Value, worldMatrix);
            } else if (node.Mesh.HasValue)
            {
                LoadMesh(context, node.Mesh.Value, node.Skin.HasValue ? node.Skin.Value : -1, worldMatrix);
            }
            else
            {
                if (node.Children != null)
                {
                    for (int i = 0; i < node.Children.Length; i++)
                    {
                        LoadNode(context, context.Model.Nodes[node.Children[i]], worldMatrix);
                    }
                }
            }
        }

        private static void LoadSkins(AssetImporterGltfMeshContext context)
        {
            for (int sk = 0; sk < context.Model.Skins.Length; sk++)
            {
                Skin skin = context.Model.Skins[sk];

                var ibmAccessor = GetAccessorByIndex(skin.InverseBindMatrices.Value, context.Model);

                BufferView? ibmBufferView = null;
                if (ibmAccessor != null &&
                    ibmAccessor.BufferView != null)
                {
                    ibmBufferView = context.Model.BufferViews[ibmAccessor.BufferView.Value];
                }

                ReadOnlySpan<Matrix4x4> matrixSpan = null;
                if (ibmBufferView != null)
                {
                    matrixSpan = MemoryMarshal.Cast<byte, Matrix4x4>(
                        new ReadOnlySpan<byte>(context.RawBuffers[ibmBufferView.Buffer], ibmBufferView.ByteOffset + ibmAccessor.ByteOffset, ibmAccessor.Count * (4 * 4 * 4)));
                }

                RuntimeBone[] bones = new RuntimeBone[skin.Joints.Length];
                for (int i = 0; i < skin.Joints.Length; i++)
                {
                    int jointIndex = skin.Joints[i];

                    bones[i] = new RuntimeBone();
                    bones[i].id = "Bone_" + i;
                    bones[i].Tag = jointIndex;
                    bones[i].invBoneSkinMatrix = matrixSpan[i];
                    bones[i].initialMatrix = Matrix4x4.Identity;
                    bones[i].animationMatrices = new Matrix4x4[context.Animations[0].Bones[i].Frames.Length];
                    for (int j = 0; j < context.Animations[0].Bones[i].Frames.Length; j++)
                    {
                        bones[i].animationMatrices[j] = context.Animations[0].Bones[i].Frames[j].Matrix;
                    }
                }

                // Build hierarchy
                for (int i = 0; i < skin.Joints.Length; i++)
                {
                    int jointIndex = skin.Joints[i];
                    var node = context.Model.Nodes[jointIndex];
                    if (node.Children == null)
                    {
                        continue;
                    }
                    bones[i].children = (from b in bones
                                         where node.Children.Contains(b.Tag)
                                         select b).ToArray();
                    for (int j = 0; j < bones[i].children.Length; j++)
                    {
                        bones[i].children[j].parent = bones[i];
                    }
                }

                if (skin.Skeleton != null)
                {
                    var node = context.Model.Nodes[skin.Skeleton.Value];
                    bones[0].initialMatrix = GetWorldMatrix(node);
                }

                context.BonesList.Add(bones);
            }
        }

        private static void LoadCamera(AssetImporterGltfMeshContext context, int cameraIndex, Matrix4x4 worldMatrix)
        {
            var camera = context.Model.Cameras[cameraIndex];
        }

        private static string GetImageAssetName(Image img, int index, string fullFilename)
        {
            string surfaceAssetName = "";
            if (img.Uri != null)
            {
                if (img.Uri.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
                {
                    surfaceAssetName = Path.GetFileNameWithoutExtension(Path.GetFileName(fullFilename)) + "_image_" + index;
                }
                else
                {
                    surfaceAssetName = Path.GetFileNameWithoutExtension(img.Uri);
                }
            }
            else
            {
                surfaceAssetName = "surface" + index;
            }
            return surfaceAssetName;
        }

        private static void LoadTextures(AssetImporterGltfMeshContext context)
        {
            if (context.Model.Textures == null)
            {
                return; 
            }

            for (int t = 0; t < context.Model.Textures.Length; t++)
            {
                glTFLoader.Schema.Texture tex = context.Model.Textures[t];
                if (tex.Source == null)
                {
                    continue; 
                }

                int imageIndex = tex.Source.Value;
                Image img = context.Model.Images[imageIndex];

                using (Stream str = glTFLoader.Interface.OpenImageFile(context.Model, imageIndex, context.FullFilename))
                {                    
                    string surfaceAssetName = GetImageAssetName(img, imageIndex, context.FullFilename);
                    surfaceAssetName += "_" + context.ImportedSurfaces.Count;

                    AssetImporterSurface2D.Import(str, surfaceAssetName, context.PackageToSaveIn.PackageName, context.FullFilename, 
                        context.AssetSettings, out Surface2D surface);
                    if (surface != null)
                    {
                        context.ImportedSurfaces.Add(surface);
                    }
                }
            }
        }

        private static void LoadBuffers(AssetImporterGltfMeshContext context)
        {
            for (int i = 0; i < context.Model.Buffers.Length; i++)
            {
                context.RawBuffers.Add(glTFLoader.Interface.LoadBinaryBuffer(context.Model, i, context.FullFilename));
            }
        }

        private static void LoadMesh(AssetImporterGltfMeshContext context, int meshIndex, int skinIndex, Matrix4x4 worldMatrix)
        {
            Mesh mesh = context.Model.Meshes[meshIndex];

            for (int p = 0; p < mesh.Primitives.Length; p++)
            {
                var prim = mesh.Primitives[p];

                if (prim.Mode != MeshPrimitive.ModeEnum.TRIANGLES)
                {
                    throw new NotImplementedException("Modes other than TRIANGLES are not implemented (yet)");
                }

                Accessor positionAccessor = GetAccessorByType("POSITION", context.Model, prim);
                if (positionAccessor.Type != Accessor.TypeEnum.VEC3)
                {
                    throw new InvalidDataException("POSITION accessor must have type VEC3");
                }

                Accessor normalAccessor = GetAccessorByType("NORMAL", context.Model, prim);
                if (normalAccessor != null &&
                    normalAccessor.Type != Accessor.TypeEnum.VEC3)
                {
                    throw new InvalidDataException("NORMAL accessor must have type VEC3");
                }

                Accessor texcoordAccessor = GetAccessorByType("TEXCOORD_0", context.Model, prim);
                if (texcoordAccessor != null &&
                    texcoordAccessor.Type != Accessor.TypeEnum.VEC2)
                {
                    throw new InvalidDataException("TEXCOORD accessor must have type VEC2");
                }

                Accessor tangentAccessor = GetAccessorByType("TANGENT", context.Model, prim);
                if (tangentAccessor != null &&
                    tangentAccessor.Type != Accessor.TypeEnum.VEC4)
                {
                    throw new InvalidDataException("TANGENT accessor must have type VEC4");
                }

                Accessor jointsAccessor = null;
                Accessor weightsAccessor = null;
                if (context.ImportAsSkeletalMesh)
                {
                    jointsAccessor = GetAccessorByType("JOINTS_0", context.Model, prim);
                    if (jointsAccessor != null &&
                        jointsAccessor.Type != Accessor.TypeEnum.VEC4)
                    {
                        throw new InvalidDataException("JOINTS_0 accessor must have type VEC4");
                    }

                    weightsAccessor = GetAccessorByType("WEIGHTS_0", context.Model, prim);
                    if (weightsAccessor != null &&
                        weightsAccessor.Type != Accessor.TypeEnum.VEC4)
                    {
                        throw new InvalidDataException("WEIGHTS_0 accessor must have type VEC4");
                    }
                }

                BufferView positionBufferView = context.Model.BufferViews[positionAccessor.BufferView.Value];
                BufferView? normalsBufferView = null;
                if (normalAccessor != null &&
                    normalAccessor.BufferView != null)
                {
                    normalsBufferView = context.Model.BufferViews[normalAccessor.BufferView.Value];
                }
                BufferView? texCoordBufferView = null;
                if (texcoordAccessor != null &&
                    texcoordAccessor.BufferView != null)
                {
                    texCoordBufferView = context.Model.BufferViews[texcoordAccessor.BufferView.Value];
                }
                BufferView? tangentBufferView = null;
                if (tangentAccessor != null &&
                    tangentAccessor.BufferView != null)
                {
                    tangentBufferView = context.Model.BufferViews[tangentAccessor.BufferView.Value];
                }
                BufferView? jointsBufferView = null;
                if (jointsAccessor != null &&
                    jointsAccessor.BufferView != null)
                {
                    jointsBufferView = context.Model.BufferViews[jointsAccessor.BufferView.Value];
                }
                BufferView? weightsBufferView = null;
                if (weightsAccessor != null &&
                    weightsAccessor.BufferView != null)
                {
                    weightsBufferView = context.Model.BufferViews[weightsAccessor.BufferView.Value];
                }

                ReadOnlySpan<Vector3> positionSpan = MemoryMarshal.Cast<byte, Vector3>(
                    new ReadOnlySpan<byte>(context.RawBuffers[positionBufferView.Buffer], positionBufferView.ByteOffset + positionAccessor.ByteOffset, positionAccessor.Count * 12));
                ReadOnlySpan<Vector3> normalsSpan = null;
                if (normalsBufferView != null)
                {
                    normalsSpan = MemoryMarshal.Cast<byte, Vector3>(
                        new ReadOnlySpan<byte>(context.RawBuffers[normalsBufferView.Buffer], normalsBufferView.ByteOffset + normalAccessor.ByteOffset, normalAccessor.Count * 12));
                }
                ReadOnlySpan<Vector2> texCoordSpan = null;
                if (texCoordBufferView != null)
                {
                    texCoordSpan = MemoryMarshal.Cast<byte, Vector2>(
                        new ReadOnlySpan<byte>(context.RawBuffers[texCoordBufferView.Buffer], texCoordBufferView.ByteOffset + texcoordAccessor.ByteOffset, texcoordAccessor.Count * 8));
                }
                ReadOnlySpan<Vector4> tangentSpan = null;
                if (tangentBufferView != null)
                {
                    tangentSpan = MemoryMarshal.Cast<byte, Vector4>(
                        new ReadOnlySpan<byte>(context.RawBuffers[tangentBufferView.Buffer], tangentBufferView.ByteOffset + tangentAccessor.ByteOffset, tangentAccessor.Count * 16));
                }
                ReadOnlySpan<Vector4us> jointsSpan = null;
                if (jointsBufferView != null)
                {
                    jointsSpan = MemoryMarshal.Cast<byte, Vector4us>(
                        new ReadOnlySpan<byte>(context.RawBuffers[jointsBufferView.Buffer], jointsBufferView.ByteOffset + jointsAccessor.ByteOffset, jointsAccessor.Count * 16));
                }
                ReadOnlySpan<Vector4> weightsSpan = null;
                if (weightsBufferView != null)
                {
                    weightsSpan = MemoryMarshal.Cast<byte, Vector4>(
                        new ReadOnlySpan<byte>(context.RawBuffers[weightsBufferView.Buffer], weightsBufferView.ByteOffset + weightsAccessor.ByteOffset, weightsAccessor.Count * 16));
                }

                MeshAsset? meshAsset = null;
                uint[] indices = null;
                if (prim.Indices != null)
                {
                    int primIndex = prim.Indices.Value;
                    Accessor indexAccessor = GetAccessorByIndex(primIndex, context.Model);
                    if (indexAccessor != null)
                    {
                        if (indexAccessor.Type != Accessor.TypeEnum.SCALAR)
                        {
                            throw new InvalidDataException("Index accessor must have type SCALAR");
                        }

                        indices = new uint[indexAccessor.Count];

                        BufferView indexBufferView = context.Model.BufferViews[indexAccessor.BufferView.Value];
                        switch (indexAccessor.ComponentType)
                        {
                            case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                                {
                                    ReadOnlySpan<byte> indexSpan =
                                        new ReadOnlySpan<byte>(context.RawBuffers[indexBufferView.Buffer], indexBufferView.ByteOffset + indexAccessor.ByteOffset, indexAccessor.Count);

                                    for (int idx = 0; idx < indices.Length; idx++)
                                    {
                                        indices[idx] = (uint)indexSpan[idx];
                                    }
                                }
                                break;

                            case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                                {
                                    ReadOnlySpan<ushort> indexSpan = MemoryMarshal.Cast<byte, ushort>(
                                        new ReadOnlySpan<byte>(context.RawBuffers[indexBufferView.Buffer], indexBufferView.ByteOffset + indexAccessor.ByteOffset, indexAccessor.Count * 2));

                                    for (int idx = 0; idx < indices.Length; idx++)
                                    {
                                        indices[idx] = (uint)indexSpan[idx];
                                    }
                                }
                                break;

                            case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                                {
                                    ReadOnlySpan<uint> indexSpan = MemoryMarshal.Cast<byte, uint>(
                                        new ReadOnlySpan<byte>(context.RawBuffers[indexBufferView.Buffer], indexBufferView.ByteOffset + indexAccessor.ByteOffset, indexAccessor.Count * 4));

                                    indices = indexSpan.ToArray();
                                }
                                break;

                            default:
                                throw new NotImplementedException("ComponentType " + indexAccessor.ComponentType + " not implemented.");
                        }
                    }
                }

                if (context.ImportAsSkeletalMesh == false)
                {
                    TangentVertex[] vertices = new TangentVertex[positionAccessor.Count];
                    bool calcTangents = tangentSpan == null && normalsSpan != null;
                    for (int v = 0; v < vertices.Length; v++)
                    {
                        vertices[v].Position = positionSpan[v];

                        if (normalsSpan != null)
                        {
                            vertices[v].Normal = normalsSpan[v];
                        }

                        if (texCoordSpan != null)
                        {
                            vertices[v].TexCoord = texCoordSpan[v];
                        }

                        if (tangentSpan != null)
                        {
                            vertices[v].Tangent = new Vector3(tangentSpan[v].X, tangentSpan[v].Y, tangentSpan[v].Z);
                        }
                        else
                        {
                            // Calculate tangents
                        }
                    }

                    if (indices == null)
                    {
                        meshAsset = StaticMesh.FromVertices(vertices, context.AssetSettings.GraphicsDevice, calcTangents);
                    }
                    else
                    {
                        meshAsset = StaticMesh.FromVertices(vertices, indices, context.AssetSettings.GraphicsDevice, calcTangents);
                    }
                    meshAsset.Name = mesh.Name + $"_{p}_{context.MeshCounter}.staticMesh";
                }
                else
                {
                    SkinnedTangentVertex[] vertices = new SkinnedTangentVertex[positionAccessor.Count];
                    bool calcTangents = tangentSpan == null && normalsSpan != null;
                    for (int v = 0; v < vertices.Length; v++)
                    {
                        vertices[v].Position = positionSpan[v];

                        if (normalsSpan != null)
                        {
                            vertices[v].Normal = normalsSpan[v];
                        }

                        if (texCoordSpan != null)
                        {
                            vertices[v].TexCoord = texCoordSpan[v];
                        }

                        if (tangentSpan != null)
                        {
                            vertices[v].Tangent = new Vector3(tangentSpan[v].X, tangentSpan[v].Y, tangentSpan[v].Z);
                        }
                        else
                        {
                            // Calculate tangents
                        }

                        if (weightsSpan != null)
                        {
                            vertices[v].JointWeights = weightsSpan[v];
                        }
                        if (jointsSpan != null)
                        {
                            vertices[v].JointIndices = new Vector4(jointsSpan[v].X, jointsSpan[v].Y, jointsSpan[v].Z, jointsSpan[v].W);
                        }
                    }

                    if (indices == null)
                    {
                        meshAsset = SkeletalMesh.FromVertices(vertices, context.AssetSettings.GraphicsDevice, calcTangents);
                    }
                    else
                    {
                        RuntimeBone[]? bones = null;
                        if (skinIndex > -1)
                        {
                            bones = context.BonesList[skinIndex];
                        }
                        meshAsset = SkeletalMesh.FromVertices(vertices, indices, bones, context.AssetSettings.GraphicsDevice, calcTangents);
                    }

                    meshAsset.Name = mesh.Name + $"_{p}_{context.MeshCounter}.skeletalMesh";
                }

                if (meshAsset != null)
                {
                    context.MeshCounter++;
                    context.PackageToSaveIn.StoreAsset(meshAsset);

                    meshAsset.RootTransform = worldMatrix;
                    meshAsset.Material = LookupMaterialForMesh(prim, context);

                    context.ImportedMeshes.Add(meshAsset);
                }
            }
        }

        private static Materials.Material LookupMaterialForMesh(MeshPrimitive prim, AssetImporterGltfMeshContext context)
        {
            if (prim.Material == null)
            {
                throw new NotImplementedException();
            }

            return context.ImportedMaterials[prim.Material.Value];
        }

        private static Accessor GetAccessorByType(string attribute, Gltf model, MeshPrimitive prim)
        {
            if (prim.Attributes.ContainsKey(attribute) == false)
            {
                return null; 
            }

            return GetAccessorByIndex(prim.Attributes[attribute], model);
        }

        private static Accessor GetAccessorByIndex(int accessorIndex, Gltf model)
        {
            if (accessorIndex < 0 || accessorIndex >= model.Accessors.Length)
            {
                return null;
            }

            return model.Accessors[accessorIndex];
        }

        private static Accessor GetAccessorForBufferView(int bufferIndex, Gltf model)
        {
            for (int i = 0; i < model.Accessors.Length; i++)
            {
                if (model.Accessors[i].BufferView == bufferIndex)
                {
                    return model.Accessors[i];
                }
            }

            return null;
        }
        #endregion
    }
}
