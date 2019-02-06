using AlkaronEngine.Assets.Materials;
using AlkaronEngine.Assets.Meshes;
using AlkaronEngine.Graphics;
using AlkaronEngine.Graphics2D;
using AlkaronEngine.Graphics3D.Geometry;
using glTFLoader.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AlkaronEngine.Assets.Importers
{
    public static class AssetImporterStaticMesh
    {
        private static readonly string[] allowedExtensions = new string[] { ".gltf", ".glb" };

        public static bool Import(string fullFilename,
            string setAssetName,
            string setPackageName,
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

            // Create asset and package names
            string assetName = setAssetName;
            string packageName = setPackageName;

            Package packageToSaveIn = null;

            assetName = Path.ChangeExtension(assetName, ".staticMesh");

            if (AlkaronCoreGame.Core.PackageManager.DoesPackageExist(packageName))
            {
                packageToSaveIn = AlkaronCoreGame.Core.PackageManager.LoadPackage(packageName);
            }
            else
            {
                packageToSaveIn = AlkaronCoreGame.Core.PackageManager.CreatePackage(packageName,
                    Path.Combine(AlkaronCoreGame.Core.ContentDirectory, packageName));
            }

            if (packageToSaveIn == null)
            {
                AlkaronCoreGame.Core.Log("Unable to create or find the package for this asset");
                return false;
            }

            try
            {
                if (extension == ".gltf" ||
                    extension == ".glb")
                {
                    ImportGLTFFile(fullFilename, assetName, packageToSaveIn, importedAssets);
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
        private static void ImportGLTFFile(string fullFilename, string assetName, Package packageToSaveIn, List<Asset> importedAssets)
        {
            string baseFolder = Path.GetDirectoryName(fullFilename);

            var model = glTFLoader.Interface.LoadModel(fullFilename);

            // Load all binary referenced buffers (vertices, indices, animations, etc.)
            List<byte[]> rawBuffers = LoadBuffers(model, fullFilename);

            // Import all textures as Surface2D assets
            importedAssets.AddRange(LoadTextures(packageToSaveIn, model, fullFilename));

            // Import all meshes
            for (int i = 0; i < model.Meshes.Length; i++)
            {
                var mesh = model.Meshes[i];

                importedAssets.AddRange(LoadMesh(packageToSaveIn, model, rawBuffers, mesh));
            }
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

        private static List<Surface2D> LoadTextures(Package packageToSaveIn, Gltf model, string fullFilename)
        {
            List<Surface2D> resultList = new List<Surface2D>();

            if (model.Textures == null)
            {
                return resultList; 
            }

            for (int t = 0; t < model.Textures.Length; t++)
            {
                glTFLoader.Schema.Texture tex = model.Textures[t];
                if (tex.Source == null)
                {
                    continue; 
                }

                int imageIndex = tex.Source.Value;
                Image img = model.Images[imageIndex];

                using (Stream str = glTFLoader.Interface.OpenImageFile(model, imageIndex, fullFilename))
                {
                    string surfaceAssetName = GetImageAssetName(img, imageIndex, fullFilename);

                    AssetImporterSurface2D.Import(str, surfaceAssetName, packageToSaveIn.PackageName, "", null, out Surface2D surface);
                    if (surface != null)
                    {
                        resultList.Add(surface); 
                    }
                }
            }

            return resultList;
        }

        private static List<byte[]> LoadBuffers(Gltf model, string fullFilename)
        {
            List<byte[]> rawBuffers = new List<byte[]>();
            for (int i = 0; i < model.Buffers.Length; i++)
            {
                rawBuffers.Add(glTFLoader.Interface.LoadBinaryBuffer(model, i, fullFilename));
            }
            return rawBuffers;
        }

        private static List<StaticMesh> LoadMesh(Package packageToSaveIn, Gltf model, List<byte[]> rawBuffers, Mesh mesh)
        {
            List<StaticMesh> staticMeshes = new List<StaticMesh>();

            for (int p = 0; p < mesh.Primitives.Length; p++)
            {
                var prim = mesh.Primitives[p];

                if (prim.Mode != MeshPrimitive.ModeEnum.TRIANGLES)
                {
                    throw new NotImplementedException("Modes other than TRIANGLES are not implemented (yet)");
                }

                Accessor positionAccessor = GetAccessorByType("POSITION", model, prim);
                if (positionAccessor.Type != Accessor.TypeEnum.VEC3)
                {
                    throw new InvalidDataException("POSITION accessor must have type VEC3");
                }

                Accessor normalAccessor = GetAccessorByType("NORMAL", model, prim);
                if (normalAccessor != null &&
                    normalAccessor.Type != Accessor.TypeEnum.VEC3)
                {
                    throw new InvalidDataException("NORMAL accessor must have type VEC3");
                }

                Accessor texcoordAccessor = GetAccessorByType("TEXCOORD_0", model, prim);
                if (texcoordAccessor != null &&
                    texcoordAccessor.Type != Accessor.TypeEnum.VEC2)
                {
                    throw new InvalidDataException("TEXCOORD accessor must have type VEC2");
                }

                Accessor tangentAccessor = GetAccessorByType("TANGENT", model, prim);
                if (tangentAccessor != null &&
                    tangentAccessor.Type != Accessor.TypeEnum.VEC4)
                {
                    throw new InvalidDataException("TANGENT accessor must have type VEC4");
                }

                TangentVertex[] vertices = new TangentVertex[positionAccessor.Count];
                BufferView positionBufferView = model.BufferViews[positionAccessor.BufferView.Value];
                BufferView normalsBufferView = null;
                if (normalAccessor != null)
                {
                    normalsBufferView = model.BufferViews[normalAccessor.BufferView.Value];
                }
                BufferView texCoordBufferView = null;
                if (texcoordAccessor != null)
                {
                    texCoordBufferView = model.BufferViews[texcoordAccessor.BufferView.Value];
                }
                BufferView tangentBufferView = null;
                if (tangentAccessor != null)
                {
                    tangentBufferView = model.BufferViews[tangentAccessor.BufferView.Value];
                }

                ReadOnlySpan<Vector3> positionSpan = MemoryMarshal.Cast<byte, Vector3>(
                    new ReadOnlySpan<byte>(rawBuffers[positionBufferView.Buffer], positionBufferView.ByteOffset + positionAccessor.ByteOffset, positionAccessor.Count * 12));
                ReadOnlySpan<Vector3> normalsSpan = null;
                if (normalsBufferView != null)
                {
                    normalsSpan = MemoryMarshal.Cast<byte, Vector3>(
                        new ReadOnlySpan<byte>(rawBuffers[normalsBufferView.Buffer], normalsBufferView.ByteOffset + normalAccessor.ByteOffset, normalAccessor.Count * 12));
                }
                ReadOnlySpan<Vector2> texCoordSpan = null;
                if (texCoordBufferView != null)
                {
                    texCoordSpan = MemoryMarshal.Cast<byte, Vector2>(
                        new ReadOnlySpan<byte>(rawBuffers[texCoordBufferView.Buffer], texCoordBufferView.ByteOffset + texcoordAccessor.ByteOffset, texcoordAccessor.Count * 8));
                }
                ReadOnlySpan<Vector4> tangentSpan = null;
                if (tangentBufferView != null)
                {
                    tangentSpan = MemoryMarshal.Cast<byte, Vector4>(
                        new ReadOnlySpan<byte>(rawBuffers[tangentBufferView.Buffer], tangentBufferView.ByteOffset + tangentAccessor.ByteOffset, tangentAccessor.Count * 16));
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
                    Accessor indexAccessor = GetAccessorByIndex(primIndex, model);
                    if (indexAccessor != null)
                    {
                        if (indexAccessor.Type != Accessor.TypeEnum.SCALAR)
                        {
                            throw new InvalidDataException("Index accessor must have type SCALAR");
                        }

                        uint[] indices = new uint[indexAccessor.Count];

                        BufferView indexBufferView = model.BufferViews[indexAccessor.BufferView.Value];
                        switch (indexAccessor.ComponentType)
                        {
                            case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                                {
                                    ReadOnlySpan<byte> indexSpan =
                                        new ReadOnlySpan<byte>(rawBuffers[indexBufferView.Buffer], indexBufferView.ByteOffset + indexAccessor.ByteOffset, indexAccessor.Count);

                                    for (int idx = 0; idx < indices.Length; idx++)
                                    {
                                        indices[idx] = (uint)indexSpan[idx];
                                    }
                                }
                                break;

                            case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                                {
                                    ReadOnlySpan<ushort> indexSpan = MemoryMarshal.Cast<byte, ushort>(
                                        new ReadOnlySpan<byte>(rawBuffers[indexBufferView.Buffer], indexBufferView.ByteOffset + indexAccessor.ByteOffset, indexAccessor.Count * 2));

                                    for (int idx = 0; idx < indices.Length; idx++)
                                    {
                                        indices[idx] = (uint)indexSpan[idx];
                                    }
                                }
                                break;

                            case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                                {
                                    ReadOnlySpan<uint> indexSpan = MemoryMarshal.Cast<byte, uint>(
                                        new ReadOnlySpan<byte>(rawBuffers[indexBufferView.Buffer], indexBufferView.ByteOffset + indexAccessor.ByteOffset, indexAccessor.Count * 4));

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
                packageToSaveIn.StoreAsset(staticMesh.Name, staticMesh);
                staticMeshes.Add(staticMesh);

                PbrMaterial material = CreateMaterialForMesh(prim, model);
            }

            return staticMeshes;
        }

        private static PbrMaterial CreateMaterialForMesh(MeshPrimitive prim, Gltf model)
        {
            PbrMaterial result = new PbrMaterial(AlkaronCoreGame.Core.SceneManager);
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
