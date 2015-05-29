using System;

namespace Orchard.Caching {
    /// <summary>
    /// 想要获得的内容接口，缓存
    /// </summary>
    public interface IAcquireContext {
        Action<IVolatileToken> Monitor { get; }
    }

    public class AcquireContext<TKey> : IAcquireContext {
        /// <summary>
        /// 通过构造函数传入key 和Monitor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="monitor"></param>
        public AcquireContext(TKey key, Action<IVolatileToken> monitor) {
            Key = key;
            Monitor = monitor;
        }

        public TKey Key { get; private set; }
        public Action<IVolatileToken> Monitor { get; private set; }
    }

    /// <summary>
    /// Simple implementation of "IAcquireContext" given a lamdba
    /// 给予lamdba表达式的IAcquireContext的简单实现
    /// </summary>
    public class SimpleAcquireContext : IAcquireContext {
        private readonly Action<IVolatileToken> _monitor;//泛型委托

        /// <summary>
        /// 通过构造函数传入monitor
        /// </summary>
        /// <param name="monitor"></param>
        public SimpleAcquireContext(Action<IVolatileToken> monitor) {
            _monitor = monitor;
        }
        /// <summary>
        /// 获取Monitor
        /// </summary>
        public Action<IVolatileToken> Monitor {
            get { return _monitor; }
        }
    }
}
