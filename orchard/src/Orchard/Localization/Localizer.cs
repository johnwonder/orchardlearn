using System.Linq;
using Orchard.Localization;

namespace Orchard.Localization {
    /// <summary>
    /// 返回地方化的字符
    /// </summary>
    /// <param name="text"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate LocalizedString Localizer(string text, params object[] args);
}

namespace Orchard.Mvc.Html {
    public static class LocalizerExtensions {
        public static LocalizedString Plural(this Localizer T, string textSingular, string textPlural, int count, params object[] args) {
            ///单数和复数的区别，如 1 minute and  2 minutes
            return T(count == 1 ? textSingular : textPlural, new object[] { count }.Concat(args).ToArray());
        }
    }
}