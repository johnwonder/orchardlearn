using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders {
    public class ThemeFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly IExtensionHarvester _extensionHarvester;
        
        /// <summary>
        /// paths参数在OrchardStarter中注入 WithParameter
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="extensionHarvester"></param>
        public ThemeFolders(IEnumerable<string> paths, IExtensionHarvester extensionHarvester) {
            _paths = paths;
            _extensionHarvester = extensionHarvester;
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(_paths, DefaultExtensionTypes.Theme, "Theme.txt", false/*isManifestOptional*/);
        }
    }
}