﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MsilDecompiler.Host.Providers;
using System.Linq;
using Mono.Cecil;

namespace MsilDecompiler.Host
{
    public class DecompileTypeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDecompilationProvider _decompilationProvider;

        public DecompileTypeMiddleware(RequestDelegate next, IDecompilationProvider decompilationProvider)
        {
            _next = next;
            _decompilationProvider = decompilationProvider;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.HasValue)
            {
                var endpoint = httpContext.Request.Path.Value;
                if (endpoint == MsilDecompilerEndpoints.DecompileType)
                {
                    await Task.Run(() =>
                    {
                        var requestObject = JsonHelper.DeserializeRequestObject(httpContext.Request.Body)
                            .ToObject<DecompileTypeRequest>();
                        var rid = JsonHelper.DeserializeRequestObject(httpContext.Request.Body)
                            .ToObject<DecompileTypeRequest>().Rid;
                        var code = new DecompileCode { Decompiled = _decompilationProvider.GetCode(requestObject.AssemblyPath, TokenType.TypeDef, rid) };
                        MiddlewareHelpers.WriteTo(httpContext.Response, code);
                    });
                    return;
                }
            }

            await _next(httpContext);
        }
    }
}