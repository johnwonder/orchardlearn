using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    public abstract class ExtensionLoaderBase : IExtensionLoader {
        private readonly IDependenciesFolder _dependenciesFolder;

        protected ExtensionLoaderBase(IDependenciesFolder dependenciesFolder) {
            _dependenciesFolder = dependenciesFolder;
        }

        public abstract int Order { get; }
        public string Name { get { return this.GetType().Name; } }

        public virtual IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor descriptor) {
            return Enumerable.Empty<ExtensionReferenceProbeEntry>();
        }

        public virtual Assembly LoadReference(DependencyReferenceDescriptor reference) {
            return null;
        }

        /// <summary>
        /// 是否兼容
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="references"></param>
        /// <returns></returns>
        public virtual bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references) {
            return true;
        }

        public abstract ExtensionProbeEntry Probe(ExtensionDescriptor descriptor);

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            //在dependencies中获取
            var dependency = _dependenciesFolder.GetDescriptor(descriptor.Id);
            if (dependency != null && dependency.LoaderName == this.Name) {//查找加载器是否一样
                return LoadWorker(descriptor);
            }
            return null;
        }

        public virtual void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) { }
        public virtual void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) { }

        public virtual void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) { }
        public virtual void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) { }
        public virtual void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) { }

        public virtual void Monitor(ExtensionDescriptor extension, Action<IVolatileToken> monitor) { }

        protected abstract ExtensionEntry LoadWorker(ExtensionDescriptor descriptor);

        public virtual IEnumerable<ExtensionCompilationReference> GetCompilationReferences(DependencyDescriptor dependency) {
            return Enumerable.Empty<ExtensionCompilationReference>();
        }

        public virtual IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor dependency) {
            return Enumerable.Empty<string>();
        }
    }
}