using System;
using System.IO;
using System.Linq;
using log4net.Config;
using Ninject.Modules;

namespace Tolltech.Ennobler
{
    public class ConfigurationModule : NinjectModule
    {
        private readonly ISettings settings;

        public ConfigurationModule(ISettings settings)
        {
            this.settings = settings;
        }

        public override void Load()
        {
            if (!string.IsNullOrWhiteSpace(settings.Log4NetFileName))
            {
                var fileInfo = new FileInfo(settings.Log4NetFileName);
                if (!fileInfo.Exists)
                    throw new Exception($"Logger configuration file {fileInfo.FullName} not found");
                XmlConfigurator.Configure(fileInfo);
            }

            this.Bind<ISettings>().ToConstant(settings);

            IoCResolver.Resolve((@interface, implementation) => this.Bind(@interface).To(implementation), new [] { "Tolltech", settings.RootNamespaceForNinjectConfiguring } );
        }

        public static class IoCResolver
        {
            public static void Resolve(Action<Type, Type> resolve, params string[] assmeblyNames)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => assmeblyNames.Any(y => x.FullName.StartsWith(y))).ToArray();

                Console.WriteLine($"DI Configuring {string.Join("\r\n", assemblies.Select(x => x.FullName))}");

                var interfaces = assemblies.SelectMany(x => x.GetTypes().Where(y => y.IsInterface)).Where(x => x.FullName != typeof(ISettings).FullName).ToArray();
                var types = assemblies.SelectMany(x => x.GetTypes().Where(y => !y.IsInterface && y.IsClass && !y.IsAbstract)).ToArray();
                foreach (var @interface in interfaces)
                {
                    var realisations = types.Where(x => @interface.IsAssignableFrom(x)).ToArray();
                    foreach (var realisation in realisations)
                    {
                        resolve(@interface, realisation);
                    }
                }
            }
        }
    }
}