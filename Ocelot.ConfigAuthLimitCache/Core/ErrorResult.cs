using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.ConfigAuthLimitCache.Core
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 输出的错误实体
    /// </summary>
    public class ErrorResult
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        public string errmsg { get; set; }
    }
}
