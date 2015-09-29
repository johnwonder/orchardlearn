using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders {
    public class CoreModuleFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly IExtensionHarvester _extensionHarvester;

        public CoreModuleFolders(IEnumerable<string> paths, IExtensionHarvester extensionHarvester) {
            _paths = paths;
            _extensionHarvester = extensionHarvester;
        }
        /// <summary>
        /// 跟ModuleFolders一样也是寻找Module.txt 
        /// 只是这边在Core中寻找 ，比如Settings 安装的时候要用到
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(_paths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}