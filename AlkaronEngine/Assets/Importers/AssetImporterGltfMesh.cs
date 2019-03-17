using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Graphics;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            internal List<Asset> ImportedAssets = new List<Asset>();
            internal bool ImportStaticMeshOnly;
            internal bool ImportAsSkeletalMesh;

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
            out List<Asset> importedAssets)
        {
            importedAssets = new List<Asset>();

            // Remember input filename
            string inputFile = fullFilename;
            if (File.Exists(inputFile) == false)
            {
                AlkaronCoreGame.Core.Log("Import file '" + inputFile + "' is not valid!");
                return false;
            }

            string extension = Path.GetExtension(inputFile);
            if (string.IsNullOrWhiteSpace(extension))
            {
                AlkaronCoreGame.Core.Log("Import file '" + inputFile + "' has an invalid file extension!");
                return false;
            }

            extension = extension.ToLowerInvariant();
            if (allowedExtensions.Contains(extension) == false)
            {
                AlkaronCoreGame.Core.Log("Import file '" + inputFile + "' has an invalid file extension!");
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

            string packageName = setPackageName;

            if (AlkaronCoreGame.Core.PackageManager.DoesPackageExist(packageName))
            {
                context.PackageToSaveIn = AlkaronCoreGame.Core.PackageManager.LoadPackage(packageName, false);
            }
            else
            {
                context.PackageToSaveIn = AlkaronCoreGame.Core.PackageManager.CreatePackage(packageName,
                    Path.Combine(AlkaronCoreGame.Core.ContentDirectory, packageName));
            }

            if (context.PackageToSaveIn == null)
            {
                AlkaronCoreGame.Core.Log("Unable to create or find the package for this asset");
                return false;
            }

            try
            {
                if (extension == ".gltf" ||
                    extension == ".glb")
                {
                    ImportGLTFFile(context); //fullFilename, assetName, packageToSaveIn, importedAssets);
                }
                else
                {
                    throw new NotImplementedException("Import for mesh with extension '" + extension + "' is not implemented.");
                }
            }
            catch (Exception ex)
            {
                AlkaronCoreGame.Core.Log("Failed to import Mesh: " + ex);
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
            }
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

                    LoadNode(context, node, Matrix.Identity);
                }
            }
        }

        private static Matrix GetWorldMatrix(Node node)
        {
            if (node.Matrix != null)
            {
                return new Matrix(node.Matrix[ 0], node.Matrix[ 1], node.Matrix[ 2], node.Matrix[ 3],
                                  node.Matrix[ 4], node.Matrix[ 5], node.Matrix[ 6], node.Matrix[ 7],
                                  node.Matrix[ 8], node.Matrix[ 9], node.Matrix[10], node.Matrix[11],
                                  node.Matrix[12], node.Matrix[13], node.Matrix[14], node.Matrix[15]);
            }
            else
            {
                Matrix resultMatrix = Matrix.Identity;

                if (node.Translation != null)
                {
                    Matrix translationMat = Matrix.CreateTranslation(node.Translation[0], node.Translation[1], node.Translation[2]);
                    resultMatrix *= translationMat;
                }

                if (node.Rotation != null)
                {
                    Matrix rotationMat = Matrix.CreateFromQuaternion(new Quaternion(node.Rotation[0], node.Rotation[1], node.Rotation[2], node.Rotation[3]));
                    resultMatrix *= rotationMat;
                }

                if (node.Scale != null)
                {
                    Matrix scaleMat = Matrix.CreateScale(node.Scale[0], node.Scale[1], node.Scale[2]);
                    resultMatrix *= scaleMat;
                }

                return resultMatrix;
            }
        }

        private static void LoadNode(AssetImporterGltfMeshContext context, Node node, Matrix parentMatrix)
        {
            Matrix worldMatrix = parentMatrix * GetWorldMatrix(node);

            if (node.Camera.HasValue)
            {
                LoadCamera(context, node.Camera.Value, worldMatrix);
            } else if (node.Mesh.HasValue)
            {
                LoadMesh(context, node.Mesh.Value, worldMatrix);
            }
        }

        private static void LoadCamera(AssetImporterGltfMeshContext context, int cameraIndex, Matrix worldMatrix)
        {
            var camera = context.Model.Cameras[cameraIndex];
        }

        private static string GetImageAssetName(Image img, int index, string fullFilename)
        {
            string surfaceAssetName = "";
            if (img.Uri.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
            {
                surfaceAssetName = Path.GetFileNameWithoutExtension(Path.GetFileName(fullFilename)) + "_image_" + index;
            }
            else
            {
                surfaceAssetName = Path.GetFileNameWithoutExtension(img.Uri);
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

                    AssetImporterSurface2D.Import(str, surfaceAssetName, context.PackageToSaveIn.PackageName, context.FullFilename, out Surface2D surface);
                    if (surface != null)
                    {
                        context.ImportedAssets.Add(surface);
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

        private static void LoadMesh(AssetImporterGltfMeshContext context, int meshIndex, Matrix worldMatrix)
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
                        if (normalsSpan != null)
                        {
                            vertices[v].BiTangent = Vector3.Cross(normalsSpan[v], vertices[v].Tangent) * tangentSpan[v].W;
                        }
                    }
                }

                StaticMesh staticMesh = null;

                if (prim.Indices == null)
                {
                    staticMesh = StaticMesh.FromVertices(vertices);
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

                        staticMesh = StaticMesh.FromVertices(vertices, indices);
                    }
                }

                staticMesh.Name = mesh.Name + "_" + p + ".staticMesh";
                context.PackageToSaveIn.StoreAsset(staticMesh);

                Materials.Material material = CreateMaterialForMesh(prim, context.Model);
                staticMesh.Material = material;

                context.ImportedAssets.Add(staticMesh);
            }
        }

        private static Materials.Material CreateMaterialForMesh(MeshPrimitive prim, Gltf model)
        {
            var mat = AlkaronCoreGame.Core.AssetManager.Load<Materials.Material>("EngineMaterials.BasicEffect.material");

            Materials.Material result = mat; //new PbrMaterial(AlkaronCoreGame.Core.SceneManager);
            //result.Effect = AlkaronCoreGame.Core.SceneManager.RenderManager.

            return result;
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
