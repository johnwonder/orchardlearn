using System.Linq;
using Autofac;
using Autofac.Core;

namespace Orchard.Commands {
    public class CommandModule : Module {
        
        /// <summary>
        /// 注册Command后 触发 Setup的时候先触发HelpCommand 然后是SetupCommand
        /// </summary>
        /// <param name="componentRegistry"></param>
        /// <param name="registration"></param>
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {

            if (!registration.Services.Contains(new TypedService(typeof(ICommandHandler))))
                return;

            var builder = new CommandHandlerDescriptorBuilder();
            var descriptor = builder.Build(registration.Activator.LimitType);
            registration.Metadata.Add(typeof(CommandHandlerDescriptor).FullName, descriptor);
        }
    }
}
