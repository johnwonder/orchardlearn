﻿using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Themes;

namespace Orchard.Mvc.ViewEngines.ThemeAwareness {

    public class ThemedViewResultFilter : FilterProvider, IResultFilter {
        private readonly IThemeManager _themeManager;
        private readonly WorkContext _workContext;
        private readonly ILayoutAwareViewEngine _layoutAwareViewEngine;

        public ThemedViewResultFilter(IThemeManager themeManager, WorkContext workContext, ILayoutAwareViewEngine layoutAwareViewEngine) {
            _themeManager = themeManager;
            _workContext = workContext;
            _layoutAwareViewEngine = layoutAwareViewEngine;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResultBase = filterContext.Result as ViewResultBase;
            if (viewResultBase == null) {
                return;
            }

            if (_workContext.CurrentTheme == null) {
                _workContext.CurrentTheme = _themeManager.GetRequestTheme(filterContext.RequestContext);
            }

          viewResultBase.ViewEngineCollection = new ViewEngineCollection(new[] { _layoutAwareViewEngine });
            //如果去掉这个ViewEngine，那么只会输出Index.cshtml
            //Document.cshtml 貌似没加载 导致<head>标签没有内容
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}