using System;

namespace Orchard.Environment.Extensions.Models {
    public static class DefaultExtensionTypes {
        public const string Module = "Module";
        public const string Theme = "Theme";

        public static bool IsModule(string extensionType) {
            //判断扩展类型是否是Module
            return string.Equals(extensionType, Module, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTheme(string extensionType) {
            //判断主题类型是否是Theme
            return string.Equals(extensionType, Theme, StringComparison.OrdinalIgnoreCase);
        }
    }
}