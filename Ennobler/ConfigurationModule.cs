using System;
using System.Linq;
using Ninject.Modules;
using Vostok.Logging.Abstractions;

namespace Tolltech.Ennobler
{
    public class ConfigurationModule : NinjectModule
    {
        private static readonly ILog log = LogProvider.Get().ForContext<ConfigurationModule>();

        private readonly ISettings settings;

        public ConfigurationModule(ISettings settings)
        {
            this.settings = settings;
        }

        public override void Load()
        {
            Bind<ISettings>().ToConstant(settings);

            IoCResolver.Resolve((@interface, implementation) => this.Bind(@interface).To(implementation), new [] { "Tolltech", settings.RootNamespaceForNinjectConfiguring } );
        }

        public static class IoCResolver
        {
            public static void Resolve(Action<Type, Type> resolve, params string[] assmeblyNames)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => assmeblyNames.Any(y => x.FullName.StartsWith(y))).ToArray();

                log.Info($"DI Configuring {string.Join("\r\n", assemblies.Select(x => x.FullName))}");

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
