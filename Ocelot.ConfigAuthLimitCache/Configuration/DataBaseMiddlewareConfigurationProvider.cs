using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Responses;
using Ocelot.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Ocelot.ConfigAuthLimitCache.Configuration
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 从数据库中加载配置的Provider
    /// </summary>
    public class DataBaseConfigurationProvider
    {
        public static OcelotMiddlewareConfigurationDelegate Get = async builder =>
        {
            var fileConfigRepo = builder.ApplicationServices.GetService<IFileConfigurationRepository>();
            var fileConfig = builder.ApplicationServices.GetService<IOptionsMonitor<FileConfiguration>>();
            var internalConfigCreator = builder.ApplicationServices.GetService<IInternalConfigurationCreator>();
            var internalConfigRepo = builder.ApplicationServices.GetService<IInternalConfigurationRepository>();
            await SetFileConfigInDataBase(builder, fileConfigRepo, fileConfig, internalConfigCreator, internalConfigRepo);

        };

        private static async Task SetFileConfigInDataBase(IApplicationBuilder builder,
            IFileConfigurationRepository fileConfigRepo, IOptionsMonitor<FileConfiguration> fileConfig,
            IInternalConfigurationCreator internalConfigCreator, IInternalConfigurationRepository internalConfigRepo)
        {

            // get the config from consul.
            var fileConfigFromDataBase = await fileConfigRepo.Get();

            if (IsError(fileConfigFromDataBase))
            {
                ThrowToStopOcelotStarting(fileConfigFromDataBase);
            }
            //else if (ConfigNotStoredInDataBase(fileConfigFromDataBase))
            //{
            //    //there was no config in consul set the file in config in consul
            //    await fileConfigRepo.Set(fileConfig.CurrentValue);
            //}
            else
            {
                await fileConfigRepo.Set(fileConfigFromDataBase.Data);
                // create the internal config from consul data
                var internalConfig = await internalConfigCreator.Create(fileConfigFromDataBase.Data);

                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
                else
                {
                    // add the internal config to the internal repo
                    var response = internalConfigRepo.AddOrReplace(internalConfig.Data);

                    if (IsError(response))
                    {
                        ThrowToStopOcelotStarting(response);
                    }
                }

                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
            }
        }

        private static void ThrowToStopOcelotStarting(Response config)
        {
            throw new Exception($"Unable to start Ocelot, errors are: {string.Join(",", config.Errors.Select(x => x.ToString()))}");
        }

        private static bool IsError(Response response)
        {
            return response == null || response.IsError;
        }

        private static bool ConfigNotStoredInDataBase(Response<FileConfiguration> fileConfigFromDataBase)
        {
            return fileConfigFromDataBase.Data == null;
        }
    }
}
