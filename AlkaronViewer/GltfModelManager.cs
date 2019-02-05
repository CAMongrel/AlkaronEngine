using System;
using System.Collections.Generic;
using System.IO;

namespace AlkaronViewer
{
    enum GltfModelEntryType
    {
        Unknown,
        Base,
        Embedded,
        Binary,
        Draco,
        PBR
    }

    class GltfModelEntry
    {
        public List<string> Files = new List<string>();
        public List<GltfModelEntryType> Types = new List<GltfModelEntryType>();
    }

    internal class GltfModelManager
    {
        private string BaseModelFolder;
        private Dictionary<string, GltfModelEntry> ModelLookup;

        public GltfModelManager(string setBaseModelFolder)
        {
            BaseModelFolder = setBaseModelFolder;
            ModelLookup = new Dictionary<string, GltfModelEntry>();
        }

        internal string GetModelPath(string name, GltfModelEntryType type = GltfModelEntryType.Base)
        {
            name = name.ToLowerInvariant();

            if (ModelLookup.ContainsKey(name))
            {
                GltfModelEntry entry = ModelLookup[name];
                if (entry.Types.Contains(type))
                {
                    int index = entry.Types.IndexOf(type);
                    return entry.Files[index];
                }
                else
                {
                    return null; 
                }
            }
            else
            {
                return null; 
            }
        }

        internal void BuildModelList()
        {
            ModelLookup.Clear();

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(BaseModelFolder, "*.gltf", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(BaseModelFolder, "*.glb", SearchOption.AllDirectories));

            for (int i = 0; i < files.Count; i++)
            {
                string name = Path.GetFileName(Path.GetFileNameWithoutExtension(files[i])).ToLowerInvariant();

                if (ModelLookup.ContainsKey(name))
                {
                    GltfModelEntry entry = ModelLookup[name];

                    var type = GetEntryTypeFromFile(files[i]);
                    if (entry.Types.Contains(type))
                    {
                        continue;
                    }

                    entry.Files.Add(files[i]);
                    entry.Types.Add(type);
                }
                else
                {
                    GltfModelEntry entry = new GltfModelEntry();

                    var type = GetEntryTypeFromFile(files[i]);

                    entry.Files.Add(files[i]);
                    entry.Types.Add(type);

                    ModelLookup.Add(name, entry);
                }
            }
        }

        private static GltfModelEntryType GetEntryTypeFromFile(string filename)
        {
            string directory = Path.GetDirectoryName(filename);
            if (directory.EndsWith("glTF", StringComparison.InvariantCultureIgnoreCase))
            {
                return GltfModelEntryType.Base;
            }
            else if (directory.EndsWith("glTF-Binary", StringComparison.InvariantCultureIgnoreCase))
            {
                return GltfModelEntryType.Binary;
            }
            else if (directory.EndsWith("glTF-Draco", StringComparison.InvariantCultureIgnoreCase))
            {
                return GltfModelEntryType.Draco;
            }
            else if (directory.EndsWith("glTF-Embedded", StringComparison.InvariantCultureIgnoreCase))
            {
                return GltfModelEntryType.Embedded;
            }
            else if (directory.EndsWith("glTF-pbrSpecularGlossiness", StringComparison.InvariantCultureIgnoreCase))
            {
                return GltfModelEntryType.PBR;
            }

            return GltfModelEntryType.Unknown;
        }
    }
}
