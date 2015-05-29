using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions {
    public interface IExtensionManager {
        /// <summary>
        /// 搜集外部调用。。
        /// http://www.cnblogs.com/zhengym/p/3299097.html
        /// </summary>
        /// <returns></returns>
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<FeatureDescriptor> AvailableFeatures();

        ExtensionDescriptor GetExtension(string id);

        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors);
    }

    /// <summary>
    /// ExtensionManager扩展方法
    /// </summary>
    public static class ExtensionManagerExtensions {
        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return extensionManager.AvailableFeatures().Where(fd => descriptor.Features.Any(sf => sf.Name == fd.Id));
        }
    }
}
