﻿// Copyright(c) 2021 ICSharpCode
// Licensed under the MIT license. See the LICENSE file in the project root for more information.

using ILSpyX.Backend.Application;
using ILSpyX.Backend.LSP.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ILSpyX.Backend.LSP;

class Program
{
    static async Task Main(string[] args)
    {
        //System.Diagnostics.Debugger.Launch();

        Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Debug()
                    .MinimumLevel.Verbose()
                    .CreateLogger();

        Log.Logger.Information("ILSpy LSP Backend starting");

        var server = await LanguageServer.From(options =>
            options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .ConfigureLogging(
                    x => x
                        .AddSerilog(Log.Logger)
                        .AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Debug))
                .AddDefaultLoggingProvider()
                .WithServices(ConfigureServices)
                .WithHandler<InitWithAssembliesHandler>()
                .WithHandler<AddAssemblyHandler>()
                .WithHandler<DecompileNodeHandler>()
                .WithHandler<GetNodesHandler>()
                .WithHandler<RemoveAssemblyHandler>()
                .WithHandler<SearchHandler>()
                .WithHandler<AnalyzeHandler>()
             );

        server.LogInfo($"ILSpy LSP Backend PID: {Environment.ProcessId}");

        await server.WasShutDown;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddILSpyXServices()
            .AddILSpyXTreeNodeProviders();
    }
}
