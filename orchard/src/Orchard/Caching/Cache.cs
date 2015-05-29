using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Orchard.Caching {
    /// <summary>
    /// 主要缓存类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class Cache<TKey, TResult> : ICache<TKey, TResult> {
        private readonly ICacheContextAccessor _cacheContextAccessor;
        private  ConcurrentDictionary<TKey, CacheEntry> _entries;

        public Cache(ICacheContextAccessor cacheContextAccessor) {
            _cacheContextAccessor = cacheContextAccessor;
            _entries = new ConcurrentDictionary<TKey, CacheEntry>();
        }
        /// <summary>
        /// 调试用
        /// </summary>
        public void WriteEntries()
        {
           if(_entries != null)
               foreach (var item in _entries)
               {
                   Debug.Write(item.Key + ";" + item.Value.ToString());
               }
        }
        /// <summary>
        /// 获取TResult类型的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="acquire">委托返回TResult</param>
        /// <returns></returns>
        public TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = _entries.AddOrUpdate(key,
                // "Add" lambda
                k => AddEntry(k, acquire),
                // "Update" lamdba
                (k, currentEntry) => UpdateEntry(currentEntry, k, acquire));

            return entry.Result;
        }

        private CacheEntry AddEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = CreateEntry(k, acquire);
            PropagateTokens(entry);
            return entry;
        }
        /// <summary>
        /// 遍历缓存条目中所有的令牌，如果其中一个令牌标识为缓存失效则重新创建缓存条目，否则返回当前缓存条目
        /// http://www.cnblogs.com/ants/p/3736538.html
        /// </summary>
        /// <param name="currentEntry"></param>
        /// <param name="k"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        private CacheEntry UpdateEntry(CacheEntry currentEntry, TKey k, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = (currentEntry.Tokens.Any(t => !t.IsCurrent)) ? CreateEntry(k, acquire) : currentEntry;
            PropagateTokens(entry);
            return entry;
        }

        private void PropagateTokens(CacheEntry entry) {
            // Bubble up volatile tokens to parent context
            if (_cacheContextAccessor.Current != null) {
                foreach (var token in entry.Tokens)
                    _cacheContextAccessor.Current.Monitor(token);//调用CacheEntry.AddToken方法 把Token加入CacheEntry的Tokens属性中
            }
        }


        private CacheEntry CreateEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire) {
            var entry = new CacheEntry();
            var context = new AcquireContext<TKey>(k, entry.AddToken);

            IAcquireContext parentContext = null;
            try {
                // Push context,外部的函数也会调用_cacheContextAccessor.Current
                parentContext = _cacheContextAccessor.Current;
                _cacheContextAccessor.Current = context;//最终Token还是会存在entry里

                entry.Result = acquire(context);//acquire为外部的委托，相当于用context作为参数调用外部的函数
                //外部的委托中调用context的Monitor entry.AddToken
            }
            finally {
                // Pop context //不同的缓存对象间
                _cacheContextAccessor.Current = parentContext;
            }
            entry.CompactTokens();//Distinct().ToArray();
            return entry;
        }
        /// <summary>
        /// 缓存实体类
        /// </summary>
        private class CacheEntry {
            private IList<IVolatileToken> _tokens;
            public TResult Result { get; set; }

            /// <summary>
            /// 获取不稳定的令牌迭代器(集合），通过接口来传递，设计。。
            /// </summary>
            public IEnumerable<IVolatileToken> Tokens {
                get {
                    return _tokens ?? Enumerable.Empty<IVolatileToken>();
                }
            }
            /// <summary>
            /// 添加令牌
            /// </summary>
            /// <param name="volatileToken"></param>
            public void AddToken(IVolatileToken volatileToken) {
                if (_tokens == null) {
                    _tokens = new List<IVolatileToken>();
                }

                _tokens.Add(volatileToken);
            }
            /// <summary>
            /// 压紧令牌(Distinct 再ToArray)
            /// </summary>
            public void CompactTokens() {
                if (_tokens != null)
                    _tokens = _tokens.Distinct().ToArray();
            }
        }
    }
}
