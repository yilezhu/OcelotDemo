using Dapper;
using Ocelot.Cache;
using Ocelot.ConfigAuthLimitCache.Configuration;
using Ocelot.ConfigAuthLimitCache.Extensions;
using Ocelot.ConfigAuthLimitCache.Models;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Logging;
using Ocelot.Responses;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.ConfigAuthLimitCache.Repository
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 实现从SQLSERVER数据库中提取配置信息
    /// </summary>
    public class SqlServerFileConfigurationRepository : IFileConfigurationRepository
    {
        private readonly IOcelotCache<FileConfiguration> _cache;
        private readonly IOcelotLogger _logger;
        private readonly ConfigAuthLimitCacheOptions _option;
        public SqlServerFileConfigurationRepository(ConfigAuthLimitCacheOptions option, IOcelotCache<FileConfiguration> cache, IOcelotLoggerFactory loggerFactory)
        {
            _option = option;
            _cache = cache;
            _logger = loggerFactory.CreateLogger<SqlServerFileConfigurationRepository>();
        }

        public Task<Response> Set(FileConfiguration fileConfiguration)
        {
            _cache.AddAndDelete(_option.CachePrefix + "FileConfiguration", fileConfiguration, TimeSpan.FromSeconds(1800), "");
            return Task.FromResult((Response)new OkResponse());
        }

        /// <summary>
        /// 提取配置信息
        /// </summary>
        /// <returns></returns>
        public async Task<Response<FileConfiguration>> Get()
        {
            var config = _cache.Get(_option.CachePrefix + "FileConfiguration", "");

            if (config != null)
            {
                return new OkResponse<FileConfiguration>(config);
            }
            #region 提取配置信息
            var file = new FileConfiguration();
            string glbsql = "select top 1 * from OcelotGlobalConfiguration where IsDefault=1";
            //提取全局配置信息
            using (var connection = new SqlConnection(_option.DbConnectionStrings))
            {
                var result = await connection.QueryFirstOrDefaultAsync<OcelotGlobalConfiguration>(glbsql);
                if (result != null)
                {
                    var glb = new FileGlobalConfiguration();
                    glb.BaseUrl = result.BaseUrl;
                    glb.DownstreamScheme = result.DownstreamScheme;
                    glb.RequestIdKey = result.RequestIdKey;
                    if (!String.IsNullOrEmpty(result.HttpHandlerOptions))
                    {
                        glb.HttpHandlerOptions = result.HttpHandlerOptions.ToObject<FileHttpHandlerOptions>();
                    }
                    if (!String.IsNullOrEmpty(result.LoadBalancerOptions))
                    {
                        glb.LoadBalancerOptions = result.LoadBalancerOptions.ToObject<FileLoadBalancerOptions>();
                    }
                    if (!String.IsNullOrEmpty(result.QoSOptions))
                    {
                        glb.QoSOptions = result.QoSOptions.ToObject<FileQoSOptions>();
                    }
                    if (!String.IsNullOrEmpty(result.ServiceDiscoveryProvider))
                    {
                        glb.ServiceDiscoveryProvider = result.ServiceDiscoveryProvider.ToObject<FileServiceDiscoveryProvider>();
                    }
                    file.GlobalConfiguration = glb;

                    //提取路由信息

                    string routesql = "select * from OcelotReRoutes where OcelotGlobalConfigurationId=@OcelotGlobalConfigurationId and IsStatus=1";
                    var routeresult = (await connection.QueryAsync<OcelotReRoutes>(routesql, new { OcelotGlobalConfigurationId=result.Id })).AsList();
                    if (routeresult != null && routeresult.Count > 0)
                    {
                        var reroutelist = new List<FileReRoute>();
                        foreach (var model in routeresult)
                        {
                            var m = new FileReRoute();
                            if (!String.IsNullOrEmpty(model.AuthenticationOptions))
                            {
                                m.AuthenticationOptions = model.AuthenticationOptions.ToObject<FileAuthenticationOptions>();
                            }
                            if (!String.IsNullOrEmpty(model.CacheOptions))
                            {
                                m.FileCacheOptions = model.CacheOptions.ToObject<FileCacheOptions>();
                            }
                            if (!String.IsNullOrEmpty(model.DelegatingHandlers))
                            {
                                m.DelegatingHandlers = model.DelegatingHandlers.ToObject<List<string>>();
                            }
                            if (!String.IsNullOrEmpty(model.LoadBalancerOptions))
                            {
                                m.LoadBalancerOptions = model.LoadBalancerOptions.ToObject<FileLoadBalancerOptions>();
                            }
                            if (!String.IsNullOrEmpty(model.QoSOptions))
                            {
                                m.QoSOptions = model.QoSOptions.ToObject<FileQoSOptions>();
                            }
                            if (!String.IsNullOrEmpty(model.DownstreamHostAndPorts))
                            {
                                m.DownstreamHostAndPorts = model.DownstreamHostAndPorts.ToObject<List<FileHostAndPort>>();
                            }
                            //开始赋值
                            m.DownstreamPathTemplate = model.DownstreamPathTemplate;
                            m.DownstreamScheme = model.DownstreamScheme;
                            m.Key = model.Key;
                            m.Priority = model.Priority ?? 0;
                            m.RequestIdKey = model.RequestIdKey;
                            m.ServiceName = model.ServiceName;
                            m.Timeout = model.Timeout ?? 0;
                            m.UpstreamHost = model.UpstreamHost;
                            if (!String.IsNullOrEmpty(model.UpstreamHttpMethod))
                            {
                                m.UpstreamHttpMethod = model.UpstreamHttpMethod.ToObject<List<string>>();
                            }
                            m.UpstreamPathTemplate = model.UpstreamPathTemplate;
                            reroutelist.Add(m);
                        }
                        file.ReRoutes = reroutelist;
                    }
                }
                else
                {
                    throw new Exception("未监测到配置信息");
                }
            }
            #endregion
            if (file.ReRoutes == null || file.ReRoutes.Count == 0)
            {
                return new OkResponse<FileConfiguration>(null);
            }
            return new OkResponse<FileConfiguration>(file);
        }
    }
}
