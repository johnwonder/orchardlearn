using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Mvc.Spooling;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Mvc.ViewEngines.ThemeAwareness {
    public interface ILayoutAwareViewEngine : IDependency, IViewEngine {
    }

    /// <summary>
    /// LayoutAwareViewEnginey的引入 包含了依赖当前主题去寻找正确View的逻辑，
    /// 代理了实际视图引擎的渲染工作。
    /// </summary>
    public class LayoutAwareViewEngine : ILayoutAwareViewEngine {
        private readonly WorkContext _workContext;
        private readonly IThemeAwareViewEngine _themeAwareViewEngine;
        private readonly IDisplayHelperFactory _displayHelperFactory;

        public LayoutAwareViewEngine(
            WorkContext workContext,
            IThemeAwareViewEngine themeAwareViewEngine,
            IDisplayHelperFactory displayHelperFactory) {
            _workContext = workContext;
            _themeAwareViewEngine = themeAwareViewEngine;
            _displayHelperFactory = displayHelperFactory;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache) {
            return _themeAwareViewEngine.FindPartialView(controllerContext, partialViewName, useCache, true);
        }

        /// <summary>
        /// ViewEngineCollection属性的FindView方法，如果返回的ViewEngineResult包含一个具体的View（View属性不为空），则直接返回该ViewEngineResult，否则抛出一个InvalidOperation异常，并将通过ViewEngineResult的SearchedLocations属性表示的搜寻位置列表格式化成一个字符串作为该异常的消息，所以图8-5所示的搜寻位置列表实际上是抛出的InvalidOperation异常的消息。
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="viewName"></param>
        /// <param name="masterName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) {
            var viewResult = _themeAwareViewEngine.FindPartialView(controllerContext, viewName, useCache, true);

            if (viewResult.View == null) {
                return viewResult;
            }

            if (!ThemeFilter.IsApplied(controllerContext.RequestContext)) {
                return viewResult;
            }

            var layoutView = new LayoutView((viewContext, writer, viewDataContainer) => {
                Logger.Information("Rendering layout view");

                var childContentWriter = new HtmlStringWriter();

                var childContentViewContext = new ViewContext(
                    viewContext, 
                    viewContext.View, 
                    viewContext.ViewData, 
                    viewContext.TempData,
                    childContentWriter);

                //Shapes Views ShapeResult Display
                viewResult.View.Render(childContentViewContext, childContentWriter);
                //_workContext.Layout 应该是一个Shape
                _workContext.Layout.Metadata.ChildContent = childContentWriter;

                var display = _displayHelperFactory.CreateHelper(viewContext, viewDataContainer);
                IHtmlString result = display(_workContext.Layout);
                //Display里会去调模板页
                writer.Write(result.ToHtmlString());

                Logger.Information("Done rendering layout view");
            }, 
            (context, view) =>
                    viewResult.ViewEngine.ReleaseView(context, viewResult.View)
                
                );

            return new ViewEngineResult(layoutView, this);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view) {
            var layoutView = (LayoutView)view;
            layoutView.ReleaseView(controllerContext, view);
        }

        class LayoutView : IView, IViewDataContainer {
            private readonly Action<ViewContext, TextWriter, IViewDataContainer> _render;
            private readonly Action<ControllerContext, IView> _releaseView;

            public LayoutView(Action<ViewContext, TextWriter, IViewDataContainer> render, Action<ControllerContext, IView> releaseView) {
                _render = render;
                _releaseView = releaseView;
            }

            public ViewDataDictionary ViewData { get; set; }

            public void Render(ViewContext viewContext, TextWriter writer) {
                ViewData = viewContext.ViewData;
                _render(viewContext, writer, this);
            }

            public void ReleaseView(ControllerContext context, IView view) {
                _releaseView(context, view);
            }
        }
    }
}
