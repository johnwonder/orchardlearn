using System;
using ClaySharp.Implementation;
using Orchard.DisplayManagement;

namespace Orchard.UI.Zones {
    public class LayoutWorkContext : IWorkContextStateProvider {
        private readonly IShapeFactory _shapeFactory;

        public LayoutWorkContext(IShapeFactory shapeFactory) {
            _shapeFactory = shapeFactory;
        }

        /// <summary>
        /// 获取Layout
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public Func<WorkContext, T> Get<T>(string name) {
            //名称为Layout的才返回
            if (name == "Layout") {
                //创建Shape
                var layout = _shapeFactory.Create("Layout", Arguments.Empty());
                return ctx => (T)layout;
            }
            return null;
        }
    }
}
