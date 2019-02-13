using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace AlkaronEngine
{
    public class AlkaronContentManager : ContentManager
    {
        internal static string ResourcesPrefix = "AlkaronEngine.Resources.";

        private string[] resourceNames;

        public AlkaronContentManager(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
            resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

        private string FindResource(string assetName)
        {
            string namesp = ResourcesPrefix;
            string fullAssetNameWithOrgExt = namesp + assetName;
            string assetNameWithXNBExt = Path.ChangeExtension(assetName, ".xnb");
            string fullAssetNameWithXNBExt = namesp + assetNameWithXNBExt;

            for (int i = 0; i < resourceNames.Length; i++)
            {
                if (resourceNames[i] == assetName ||
                    resourceNames[i] == fullAssetNameWithOrgExt ||
                    resourceNames[i] == assetNameWithXNBExt ||
                    resourceNames[i] == fullAssetNameWithXNBExt)
                {
                    return resourceNames[i]; 
                }
            }

            return assetName;
        }

        protected override Stream OpenStream(string assetName)
        {
            var fullName = FindResource(assetName);

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
            if (stream != null)
            {
                return stream; 
            }

            return base.OpenStream(assetName);
        }

        /// <summary>
        /// Directly opens a Stream to an embedded resource.
        /// 
        /// Completely unrelated to the XNA content pipeline.
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
