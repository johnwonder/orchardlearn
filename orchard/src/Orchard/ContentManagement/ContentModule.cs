using Autofac;

namespace Orchard.ContentManagement {
    /// <summary>
    /// 应该是在ShellContainerFactory中注册
    /// </summary>
    public class ContentModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>().InstancePerDependency();
        }
    }
}
