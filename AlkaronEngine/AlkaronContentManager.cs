using System;
using System.IO;
using System.Reflection;
using System.Linq;

namespace AlkaronEngine
{
    public class AlkaronContentManager
    {
        internal static string ResourcesPrefix = "AlkaronEngine.Resources.";

        private string[] resourceNames;

        public AlkaronContentManager() 
        {
            resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

        private string FindResource(string assetName)
        {
            string namesp = ResourcesPrefix;
            string fullAssetNameWithOrgExt = namesp + assetName;

            for (int i = 0; i < resourceNames.Length; i++)
            {
                if (resourceNames[i] == assetName ||
                    resourceNames[i] == fullAssetNameWithOrgExt)
                {
                    return resourceNames[i]; 
                }
            }

            return assetName;
        }

        protected Stream OpenStream(string assetName)
        {
            var fullName = FindResource(assetName);

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
            if (stream != null)
            {
                return stream; 
            }

            return null;
        }

        /// <summary>
        /// Directly opens a Stream to an embedded resource.
        /// </summary>
        public Stream OpenResourceStream(string resourceName)
        {
            var fullName = FindResource(resourceName);

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
            if (stream != null)
            {
                return stream;
            }

            return null;
        }

        /// <summary>
        /// Directly reads the bytes of an embedded resource.
        /// </summary>
        public ReadOnlySpan<byte> OpenResourceBytes(string resourceName)
        {
            var fullName = FindResource(resourceName);

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
            if (stream != null)
            {
                using (stream)
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);

                    ReadOnlySpan<byte> result = new ReadOnlySpan<byte>(data);
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the resource identified by its full resource path
        /// (e.g. "AlkaronEngine.Resources.EngineMaterials.package") is actually
        /// a resource inside the binary.
        /// </summary>
        internal bool IsResource(string fullResourceName)
        {
            return resourceNames.Contains(fullResourceName); 
        }

        /// <summary>
        /// Returns the "filename" of an embedded resource, e.g.
        /// "AlkaronEngine.Resources.EngineMaterials.package" becomes
        /// "EngineMaterials.package".
        /// </summary>
        internal static string GetResourceFilename(string fullResourceName)
        {
            if (fullResourceName.StartsWith(ResourcesPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return fullResourceName.Substring(ResourcesPrefix.Length);
            }

            return fullResourceName;
        }

        /// <summary>
        /// Returns an array of the resources ending with "extension".
        /// </summary>
        internal string[] GetResourcesByType(string extension)
        {
            extension = extension.ToLowerInvariant();

            return (from r in resourceNames
                    where r.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)
                    select r).ToArray();
        }
    }
}
