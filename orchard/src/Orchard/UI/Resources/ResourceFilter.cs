using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;

namespace Orchard.UI.Resources {
    public class ResourceFilter : FilterProvider, IResultFilter {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public ResourceFilter(
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }
        /// <summary>
        /// http://blog.csdn.net/it_xiaohong/article/details/7283495
        /// IResultFilter 和 IActionFilter 一样提供2个方法，执行前和执行后，分别是在 返回Result之前执行和返回Result之后执行。
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
                return;

            var ctx = _workContextAccessor.GetContext();
            var head = ctx.Layout.Head;//这个Head其实是在CoreShapes中去创建的 created.New.DocumentZone(ZoneName: "Head");
            var tail = ctx.Layout.Tail;
            head.Add(_shapeFactory.Metas());
            head.Add(_shapeFactory.HeadLinks());//输出RegisterLink方法
            head.Add(_shapeFactory.StylesheetLinks());//输出Style.Include
            head.Add(_shapeFactory.HeadScripts()); //HeadScripts 调用CoreShapes中的HeadScripts方法
            tail.Add(_shapeFactory.FootScripts());
            //Add 只是放进集合 要到调用Display( Head)时才会调用 
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}