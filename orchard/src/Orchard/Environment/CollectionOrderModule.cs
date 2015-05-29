using System;
using System.Collections;
using Autofac.Core;

namespace Orchard.Environment {
    internal class CollectionOrderModule : IModule {
        public void Configure(IComponentRegistry componentRegistry) {
            componentRegistry.Registered += (sender, registered) => {
                // only bother watching enumerable resolves
                //只操心集合的解析
                var limitType = registered.ComponentRegistration.Activator.LimitType;
                if (typeof(IEnumerable).IsAssignableFrom(limitType)) {
                    registered.ComponentRegistration.Activated += (sender2, activated) => {
                        // Autofac's IEnumerable feature returns an Array
                        if (activated.Instance is Array) {
                            // Orchard needs FIFO, not FILO, component order
                            Array.Reverse((Array)activated.Instance);
                        }
                    };
                }
            };
        }
    }
}
