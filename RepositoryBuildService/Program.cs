using System;
using Topshelf;
using Topshelf.HostConfigurators;

namespace RepositoryBuildService
{
    public class Program
    {
        public static void Main()
        {
            var exitCode = HostFactory.Run(HostConfiguration);

            var exitCodeValue = (int) Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }

        private static void HostConfiguration(HostConfigurator configurator)
        {
            configurator.Service<IService>(service =>
            {
                service.ConstructUsing(s => new AutomaticRepositoryBuildService());
                service.WhenStarted(s => s.Start());
                service.WhenStopped(s => s.Stop());
            });

            configurator.RunAsLocalSystem();
            configurator.SetServiceName("AutomaticRepositoryBuildService");
            configurator.SetDisplayName("Automatic Repository Build Service");
            configurator.SetDescription("Service used to automatically build an Arma3Sync Repository.");
        }
    }
}
