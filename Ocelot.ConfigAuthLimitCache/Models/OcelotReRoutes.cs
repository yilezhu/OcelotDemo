using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocelot.ConfigAuthLimitCache.Models
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// ReRoute的数据存储定义
    /// </summary>
    public class OcelotReRoutes
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Ocelot
        /// </summary>
        [Required]
        public int OcelotGlobalConfigurationId { get; set; }

        [Required]
        [StringLength(150)]
        public string UpstreamPathTemplate { get; set; }

        [Required]
        [StringLength(50)]
        public string UpstreamHttpMethod { get; set; }

        [StringLength(100)]
        public string UpstreamHost { get; set; }

        [StringLength(50)]
        public string DownstreamScheme { get; set; }

        [StringLength(200)]
        public string DownstreamPathTemplate { get; set; }

        [StringLength(500)]
        public string DownstreamHostAndPorts { get; set; }

        [StringLength(300)]
        public string AuthenticationOptions { get; set; }

        [StringLength(100)]
        public string RequestIdKey { get; set; }

        [StringLength(200)]
        public string CacheOptions { get; set; }

        [StringLength(100)]
        public string ServiceName { get; set; }

        [StringLength(200)]
        public string QoSOptions { get; set; }

        [StringLength(200)]
        public string LoadBalancerOptions { get; set; }

        [StringLength(100)]
        public string Key { get; set; }

        [StringLength(200)]
        public string DelegatingHandlers { get; set; }

        public int? Priority { get; set; }

        public int? Timeout { get; set; }

        [StringLength(50)]
        public string CreateUid { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(50)]
        public string UpdateUid { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int IsStatus { get; set; }
    }
}
