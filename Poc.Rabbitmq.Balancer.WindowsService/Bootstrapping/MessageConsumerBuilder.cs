using System;
using System.Diagnostics;
using Autofac;
using Poc.Rabbitmq.Balancer.WindowsService.Bootstrapping.Fluent;
using Vueling.DIRegister.AssemblyDiscovery.ServiceLibrary;

namespace Poc.Rabbitmq.Balancer.WindowsService.Bootstrapping
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CustomRules.Maintenability", "VY1001:GlobalUseDecoratedServices")]
    public class MessageConsumerBuilder : IRegisterCustomisations, IBuildWindowsService
    {
        #region .: Boilerplate (don't change) :.

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CustomRules.Maintenability", "VY1000:GlobalNotUseServiceLocatorPattern")]
        private readonly ContainerBuilder _builder;
        private readonly RegisterDefinition _registerDefinition;
        

        private MessageConsumerBuilder(params Type[] eventHandlers)
        {
            _builder = new ContainerBuilder();
        }
                
        public static IRegisterCustomisations RegisterMessageHandlers(params Type[] eventHandlers)
        {
            return new MessageConsumerBuilder(eventHandlers);
        }

        public ConsumerWindowsService BuildWithVerbose()
        {
            return Build(true);
        }

        public IContainer BuildDebug()
        {
            try
            {
                UpdateBuilder(_reflectionRegistrator.Container);

                return _reflectionRegistrator.Container;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not build windows service: " + ex);
                throw;
            }
        }

        public ConsumerWindowsService Build()
        {
            return Build(false);
        }

        private ConsumerWindowsService Build(bool verbose)
        {
            try
            {
                return new ConsumerWindowsService();
            }
            catch(Exception ex)
            {
                Trace.TraceError("Could not build windows service: " + ex);
                throw;
            }
        }

        private void UpdateBuilder(IContainer container)
        {
            _builder.Update(container);
        }

        #endregion .: Boilerplate (don't change) :.
    }
}
