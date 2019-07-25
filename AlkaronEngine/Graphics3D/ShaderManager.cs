using AlkaronEngine.Assets.Materials;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace AlkaronEngine.Graphics3D
{
    class ShaderManager
    {
        private object lockObj = new object();
        private Dictionary<string, Shader> loadedFragmentShaders;

        internal Shader StaticMeshVertexShader { get; private set; }

        private byte[] staticMeshVertexShaderSpirv;
        //private byte[] skeletalMeshVertexShader;

        internal ShaderManager()
        {
            loadedFragmentShaders = new Dictionary<string, Shader>();
        }

        internal void Initialize()
        {
            var result = SpirvCompilation.CompileGlslToSpirv(
                Encoding.UTF8.GetString(AlkaronCoreGame.Core.AlkaronContent.OpenResourceBytes("StaticMeshVertex.glsl")), 
                null, ShaderStages.Vertex, new GlslCompileOptions());
            staticMeshVertexShaderSpirv = result.SpirvBytes;

            CompileFragmentShader("SimpleColorFragment",
                Encoding.UTF8.GetString(AlkaronCoreGame.Core.AlkaronContent.OpenResourceBytes("SimpleColorFragment.glsl")));
            CompileFragmentShader(Material.DefaultPBRFragmentName,
                Encoding.UTF8.GetString(AlkaronCoreGame.Core.AlkaronContent.OpenResourceBytes("DefaultPBRFragment.glsl")));
        }

        internal void CompileFragmentShader(string name, string shaderCode, string entryPoint = "main")
        {
            name = name.ToLowerInvariant();

            var result = SpirvCompilation.CompileGlslToSpirv(
                shaderCode, null, ShaderStages.Fragment, new GlslCompileOptions());

            var shaders = AlkaronCoreGame.Core.GraphicsDevice.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(
                    ShaderStages.Vertex,
                    staticMeshVertexShaderSpirv,
                    "main"),
                new ShaderDescription(
                    ShaderStages.Fragment,
                    result.SpirvBytes,
                    "main")
                );

            lock (lockObj)
            {
                if (StaticMeshVertexShader == null)
                {
                    StaticMeshVertexShader = shaders[0];
                }

                if (loadedFragmentShaders.ContainsKey(name))
                {
                    loadedFragmentShaders[name] = shaders[1];
                }
                else
                {
                    loadedFragmentShaders.Add(name, shaders[1]);
                }
            }
        }

        internal Shader? GetFragmentShaderByName(string name)
        {
            name = name.ToLowerInvariant();
            Shader? result = null;
            lock (lockObj)
            {
                if (loadedFragmentShaders.ContainsKey(name))
                {
                    result = loadedFragmentShaders[name];
                }
            }
            return result;
        }
    }
}
