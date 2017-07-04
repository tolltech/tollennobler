﻿using System;
using System.IO;
using System.Linq;
using log4net.Config;
using Ninject.Modules;

namespace Tolltech.TollEnnobler
{
    public class ConfigurationModule : NinjectModule
    {
        private readonly string log4NetFileName;

        public ConfigurationModule(string log4NetFileName = null)
        {
            this.log4NetFileName = log4NetFileName;
        }

        public override void Load()
        {
            if (!string.IsNullOrWhiteSpace(log4NetFileName))
            {
                var fileInfo = new FileInfo(log4NetFileName);
                if (!fileInfo.Exists)
                    throw new Exception($"Logger configuration file {fileInfo.FullName} not found");
                XmlConfigurator.Configure(fileInfo);
            }

            IoCResolver.Resolve((@interface, implementation) => this.Bind(@interface).To(implementation), "TolltechBilling");
        }

        public static class IoCResolver
        {
            public static void Resolve(Action<Type, Type> resolve, params string[] assmeblyNames)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => assmeblyNames.Any(y => x.FullName.StartsWith(y))).ToArray();
                var interfaces = assemblies.SelectMany(x => x.GetTypes().Where(y => y.IsInterface)).ToArray();
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