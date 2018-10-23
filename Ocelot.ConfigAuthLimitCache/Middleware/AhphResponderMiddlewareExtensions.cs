using Ocelot.Middleware.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.ConfigAuthLimitCache.Middleware
{
    public static class AhphResponderMiddlewareExtensions
    {
        public static IOcelotPipelineBuilder UseAhphResponderMiddleware(this IOcelotPipelineBuilder builder)
        {
            return builder.UseMiddleware<AhphResponderMiddleware>();
        }
    }
}
