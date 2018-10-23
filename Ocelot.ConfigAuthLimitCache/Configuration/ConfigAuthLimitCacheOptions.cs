using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.ConfigAuthLimitCache.Configuration
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 重写管道需要传入的配置项
    /// </summary>
    public class ConfigAuthLimitCacheOptions
    {
        /// <summary>
        /// 是否启动限流,默认 true
        /// </summary>
        public bool EnableRateLimit { get; set; } = true;

        /// <summary>
        /// 是否启动授权,默认 true
        /// </summary>
        public bool EnableAuthorization { get; set; } = true;

        /// <summary>
        /// 限流校验的头部标识，默认 client_id
        /// </summary>
        public string ClientKey { get; set; } = "client_id";

        /// <summary>
        /// 提取数据缓存过期时间(秒),默认1个小时
        /// </summary>
        public int CacheExpireTime { get; set; } = 3600;

        /// <summary>
        /// 缓存默认前缀，防止重复
        /// </summary>
        public string CachePrefix { get; set; } = "oce";

        /// <summary>
        /// 数据库连接字符串，使用不同数据库时自行修改,默认实现了SQLSERVER
        /// </summary>
        public string DbConnectionStrings { get; set; }

        /// <summary>
        /// Redis连接字符串,需要传入,数组形式，多个则采用集群模式
        /// </summary>
        public List<string> RedisConnectionStrings { get; set; }
    }
}
