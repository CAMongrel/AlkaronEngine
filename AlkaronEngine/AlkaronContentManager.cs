using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Content;

namespace AlkaronEngine
{
    public class AlkaronContentManager : ContentManager
    {
        private string[] resourceNames;

        public AlkaronContentManager(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
            resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

        private string FindResource(string assetName)
        {
            string namesp = "AlkaronEngine.Resources.";
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
    }
}
