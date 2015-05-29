using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Caching {
    public class DefaultCacheContextAccessor : ICacheContextAccessor {

        /// <summary>
        /// http://www.cnblogs.com/zhy2002/archive/2008/11/07/1329314.html
        /// http://www.cnblogs.com/xuxiaoguang/archive/2008/10/08/1306127.html
        /// 它标记的静态字段的存取是依赖当前线程，而独立于其他线程的
        /// </summary>
        [ThreadStatic]
        private static IAcquireContext _threadInstance;

        public static IAcquireContext ThreadInstance {
            get { return _threadInstance; }
            set { _threadInstance = value; }
        }

        public IAcquireContext Current {
            get { return ThreadInstance; }
            set { ThreadInstance = value; }
        }
    }
}