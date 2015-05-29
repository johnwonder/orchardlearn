using System;

namespace Orchard.Mvc.Routes {
    /// <summary>
    /// Small worker class to perform path prefix adjustments
    /// 路径前缀的调整
    /// </summary>
    public class UrlPrefix {
        private readonly string _prefix;

        public UrlPrefix(string prefix) {
            _prefix = prefix.TrimStart('~').Trim('/');
        }

        /// <summary>
        /// 移除 prefix ~/foo ~/foo/bar 变成~/bar
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string RemoveLeadingSegments(string path) {
            var beginIndex = 0;
            if (path.Length > beginIndex && path[beginIndex] == '~')
                ++beginIndex;
            if (path.Length > beginIndex && path[beginIndex] == '/')
                ++beginIndex;

            var endIndex = beginIndex + _prefix.Length;
            if (path.Length == endIndex) {
                // no-op
            }
            else if (path.Length > endIndex && path[endIndex] == '/') {
                // don't include slash after segment in result
                ++endIndex;
            }
            else {
                // too short to compare - return unmodified
                return path;
            }
            //~/foo/bar foo
            if (string.Compare(path, beginIndex, _prefix, 0, _prefix.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                return path.Substring(0, beginIndex) + path.Substring(endIndex);
            }

            return path;
        }

        public string PrependLeadingSegments(string path) {
            if (path == "~") {
                // special case for peculiar situation
                return "~/" + _prefix + "/";
            }

            var index = 0;
            if (path.Length > index && path[index] == '~')
                ++index;
            if (path.Length > index && path[index] == '/')
                ++index;

            return path.Substring(0, index) + _prefix + '/' + path.Substring(index);
        }
    }
}
