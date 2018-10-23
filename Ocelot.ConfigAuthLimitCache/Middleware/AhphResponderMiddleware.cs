using Microsoft.AspNetCore.Http;
using Ocelot.Errors;
using Ocelot.Middleware;
using Ocelot.Responder;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ocelot.DependencyInjection;
using Ocelot.Logging;
using Ocelot.ConfigAuthLimitCache.Core;
using Ocelot.ConfigAuthLimitCache.Extensions;

namespace Ocelot.ConfigAuthLimitCache.Middleware
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 重写请求中间件，修复bug，并指定统一的状态码返回,请求结束时捕获最终的结果，输出或返回
    /// </summary>
    public class AhphResponderMiddleware: OcelotMiddleware
    {
        private readonly OcelotRequestDelegate _next;
        private readonly IHttpResponder _responder;
        private readonly IErrorsToHttpStatusCodeMapper _codeMapper;

        public AhphResponderMiddleware(OcelotRequestDelegate next,
            IHttpResponder responder,
            IOcelotLoggerFactory loggerFactory,
            IErrorsToHttpStatusCodeMapper codeMapper
           )
            : base(loggerFactory.CreateLogger<AhphResponderMiddleware>())
        {
            _next = next;
            _responder = responder;
            _codeMapper = codeMapper;
        }

        public async Task Invoke(DownstreamContext context)
        {
            await _next.Invoke(context);

            if (context.IsError)
            {
                //  Logger.LogWarning($"{context.Errors.ToErrorString()} errors found in {MiddlewareName}. Setting error response for request path:{context.HttpContext.Request.Path}, request method: {context.HttpContext.Request.Method}");

                // SetErrorResponse(context.HttpContext, context.Errors);

                var errmsg = context.Errors[0].Message;
                int httpstatus = _codeMapper.Map(context.Errors);
                var errResult = new ErrorResult() { errcode = httpstatus, errmsg = errmsg };
                var message = errResult.ToJson();
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.HttpContext.Response.WriteAsync(message);
                return;

            }
            else if (context.DownstreamResponse == null)
            {//添加如果管道强制终止，不做任何处理,修复未将对象实例化错误

            }
            else
            {
                Logger.LogDebug("no pipeline errors, setting and returning completed response");
                await _responder.SetResponseOnHttpContext(context.HttpContext, context.DownstreamResponse);
            }
        }

        private void SetErrorResponse(HttpContext context, List<Error> errors)
        {
            var statusCode = _codeMapper.Map(errors);
            _responder.SetErrorResponseOnContext(context, statusCode);
        }
    }
}
