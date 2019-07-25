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
            //internal List<Asset> ImportedAssets = new List<Asset>();

            internal List<Surface2D> ImportedSurfaces = new List<Surface2D>();
            internal List<Materials.Material> ImportedMaterials = new List<Materials.Material>();
            internal List<MeshAsset> ImportedMeshes = new List<MeshAsset>();

            internal bool ImportStaticMeshOnly;
            internal bool ImportAsSkeletalMesh;
            internal AssetSettings AssetSettings;
            internal int MeshCounter;

            internal Gltf Model;
            internal List<byte[]> RawBuffers = new List<byte[]>();

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

            string extension = Path.GetExtension(inputFile);
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
            context.BaseFolder = Path.GetDirectoryName(fullFilename);
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
                    ImportGLTFFile(context); //fullFilename, assetName, packageToSaveIn, importedAssets);

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

            // Load all scenes
            LoadScenes(context);

            ReportProgress(context, "Finished");
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

            ConstructedShader constructedShader = new ConstructedShader(context.BaseAssetName + "_Material_" + mat.Name);
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
                throw new NotImplementedException();
            }
            else
            {
                if (mat.EmissiveFactor != null)
                {
                    //throw new NotImplementedException();
                }
            }

            result.LoadFromConstructedShader(constructedShader);

            context.ImportedMaterials.Add(result);
        }

        private static void LoadScenes(AssetImporterGltfMeshContext context)
        {
            for (int i = 0; i < context.Model.Scenes.Length; i++)
            {
                ReportProgress(context, "Loading scene #" + i);

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
            Matrix4x4 worldMatrix = parentMatrix * GetWorldMatrix(node);

            if (node.Camera.HasValue)
            {
                LoadCamera(context, node.Camera.Value, worldMatrix);
            } else if (node.Mesh.HasValue)
            {
                LoadMesh(context, node.Mesh.Value, worldMatrix);
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

        private static void LoadMesh(AssetImporterGltfMeshContext context, int meshIndex, Matrix4x4 worldMatrix)
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

                TangentVertex[] vertices = new TangentVertex[positionAccessor.Count];
                BufferView positionBufferView = context.Model.BufferViews[positionAccessor.BufferView.Value];
                BufferView normalsBufferView = null;
                if (normalAccessor != null)
                {
                    normalsBufferView = context.Model.BufferViews[normalAccessor.BufferView.Value];
                }
                BufferView texCoordBufferView = null;
                if (texcoordAccessor != null)
                {
                    texCoordBufferView = context.Model.BufferViews[texcoordAccessor.BufferView.Value];
                }
                BufferView tangentBufferView = null;
                if (tangentAccessor != null)
                {
                    tangentBufferView = context.Model.BufferViews[tangentAccessor.BufferView.Value];
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

                StaticMesh staticMesh = null;

                if (prim.Indices == null)
                {
                    staticMesh = StaticMesh.FromVertices(vertices, context.AssetSettings.GraphicsDevice, calcTangents);
                }
                else
                {
                    int primIndex = prim.Indices.Value;
                    Accessor indexAccessor = GetAccessorByIndex(primIndex, context.Model);
                    if (indexAccessor != null)
                    {
                        if (indexAccessor.Type != Accessor.TypeEnum.SCALAR)
                        {
                            throw new InvalidDataException("Index accessor must have type SCALAR");
                        }

                        uint[] indices = new uint[indexAccessor.Count];

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

                        staticMesh = StaticMesh.FromVertices(vertices, indices, context.AssetSettings.GraphicsDevice, calcTangents);
                    }
                }

                staticMesh.Name = mesh.Name + $"_{p}_{context.MeshCounter}.staticMesh";
                context.MeshCounter++;
                context.PackageToSaveIn.StoreAsset(staticMesh);

                staticMesh.RootTransform = worldMatrix;
                staticMesh.Material = LookupMaterialForMesh(prim, context);

                context.ImportedMeshes.Add(staticMesh);
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
