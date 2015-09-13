using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeTableBuilder {
        private readonly IList<ShapeAlterationBuilder> _alterationBuilders = new List<ShapeAlterationBuilder>();
        private readonly Feature _feature;

        public ShapeTableBuilder(Feature feature) {
            _feature = feature;
        }

        /// <summary>
        /// 返回有Feature和shapeType的ShapeAlterationBuilder
        /// 并且添加进AlterationBuilders集合
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public ShapeAlterationBuilder Describe(string shapeType) {
            var alterationBuilder = new ShapeAlterationBuilder(_feature, shapeType);
            _alterationBuilders.Add(alterationBuilder);
            return alterationBuilder;
        }

        /// <summary>
        /// alterationBuilders通过 Describe方法 添加 订阅
        /// 然后通过
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ShapeAlteration> BuildAlterations() {
            return _alterationBuilders.Select(b => b.Build());
        }
    }
}