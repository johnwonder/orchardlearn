using System.Collections.Generic;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy {
    /// <summary>
    /// This service determines which paths to examine, and provides
    /// the shape type based on the template file paths discovered
    /// //确定检测哪些路径 ，在发现模板文件的基础上提供shape type 
    /// </summary>
    public interface IShapeTemplateHarvester : IDependency {
        IEnumerable<string> SubPaths();
        IEnumerable<HarvestShapeHit> HarvestShape(HarvestShapeInfo info);
    }

    public class BasicShapeTemplateHarvester : IShapeTemplateHarvester {

        /// <summary>
        /// 子路径 Views,Views,Views/Parts,Views/Fields
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> SubPaths() {
            return new[] { "Views", "Views/Items", "Views/Parts", "Views/Fields" };
        }

        public IEnumerable<HarvestShapeHit> HarvestShape(HarvestShapeInfo info) {
            var lastDash = info.FileName.LastIndexOf('-');
            var lastDot = info.FileName.LastIndexOf('.');
            if (lastDot <= 0 || lastDot < lastDash) {
                yield return new HarvestShapeHit {
                    ShapeType = Adjust(info.SubPath, info.FileName, null)
                };
            }
            else {
                //FileName 为GetFileNameWithoutExtension得到的文件名
                var displayType = info.FileName.Substring(lastDot + 1);
                yield return new HarvestShapeHit {
                    ShapeType = Adjust(info.SubPath, info.FileName.Substring(0, lastDot), displayType),
                    DisplayType = displayType
                };
            }
        }

        static string Adjust(string subPath, string fileName, string displayType) {
            var leader = "";
            if (subPath.StartsWith("Views/") && subPath != "Views/Items") {
                leader = subPath.Substring("Views/".Length) + "_";
            }

            // canonical shape type names must not have - or . to be compatible 
            // with display and shape api calls)))
            var shapeType = leader + fileName.Replace("--", "__").Replace("-", "__").Replace('.', '_');

            if (string.IsNullOrEmpty(displayType)) {
                return shapeType.ToLowerInvariant();
            }
            var firstBreakingSeparator = shapeType.IndexOf("__");
            if (firstBreakingSeparator <= 0) {
                return (shapeType + "_" + displayType).ToLowerInvariant();
            }

            return (shapeType.Substring(0, firstBreakingSeparator) + "_" + displayType + shapeType.Substring(firstBreakingSeparator)).ToLowerInvariant();
        }
    }

    public class HarvestShapeInfo {
        public string SubPath { get; set; }
        public string FileName { get; set; }
        public string TemplateVirtualPath { get; set; }
    }

    public class HarvestShapeHit {
        public string ShapeType { get; set; }
        public string DisplayType { get; set; }
    }

    public interface IShapeTemplateViewEngine : IDependency {
        IEnumerable<string> DetectTemplateFileNames(IEnumerable<string> fileNames);
    }

}
