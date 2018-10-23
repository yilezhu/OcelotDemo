using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocelot.ConfigAuthLimitCache.Models
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 全局配置相关
    /// </summary>
    public class OcelotGlobalConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string GatewayName { get; set; }

        [StringLength(100)]
        public string RequestIdKey { get; set; }

        [StringLength(100)]
        public string BaseUrl { get; set; }

        [StringLength(50)]
        public string DownstreamScheme { get; set; }

        [StringLength(300)]
        public string ServiceDiscoveryProvider { get; set; }

        [StringLength(300)]
        public string QoSOptions { get; set; }

        [StringLength(300)]
        public string LoadBalancerOptions { get; set; }

        [StringLength(300)]
        public string HttpHandlerOptions { get; set; }

        public DateTime? LastUpdateTime { get; set; }
    }
}
