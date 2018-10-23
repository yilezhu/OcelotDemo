using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocelot.Cache;
using Ocelot.ConfigAuthLimitCache.Configuration;
using Ocelot.ConfigAuthLimitCache.Repository;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.ConfigAuthLimitCache.DependencyInjection
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 基于Ocelot扩展的依赖注入
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加默认的注入方式，所有需要传入的参数都是用默认值
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IOcelotBuilder AddAuthLimitCache(this IOcelotBuilder builder, Action<ConfigAuthLimitCacheOptions> option)
        {
            builder.Services.Configure(option);
            builder.Services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<ConfigAuthLimitCacheOptions>>().Value);
            #region 注入其他配置信息
            //重写提取Ocelot配置信息
            builder.Services.AddSingleton(DataBaseConfigurationProvider.Get);
            //builder.Services.AddHostedService<FileConfigurationPoller>();
            builder.Services.AddSingleton<IFileConfigurationRepository, SqlServerFileConfigurationRepository>();
            //注入自定义限流配置
            //注入认证信息
            #endregion
            return builder;
        }
    }
}
