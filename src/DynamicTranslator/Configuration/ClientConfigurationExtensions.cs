﻿using System;

using Abp.Dependency;
using Abp.Extensions;

using DynamicTranslator.Configuration.Startup;

namespace DynamicTranslator.Configuration
{
    public static class ClientConfigurationExtensions
    {
        public static void CreateOrConsolidate(this IClientConfiguration clientConfiguration, Action<IClientConfiguration> creator)
        {
            using (var appConfigManager = IocManager.Instance.ResolveAsDisposable<IAppConfigManager>())
            using (var appConfiguration = IocManager.Instance.ResolveAsDisposable<IApplicationConfiguration>())
            {
                if (clientConfiguration.Id.IsNullOrEmpty())
                {
                    creator(clientConfiguration);

                    appConfiguration.Object.ClientConfiguration = clientConfiguration;

                    appConfigManager.Object.SaveOrUpdate("ClientId", clientConfiguration.Id);
                }
            }
        }
    }
}