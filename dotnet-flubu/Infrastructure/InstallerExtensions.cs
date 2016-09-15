﻿using flubu.Commanding;
using flubu.Scripting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace flubu.Infrastructure
{
    public static class InstallerExtensions
    {
        public static IServiceCollection RegisterAll(this IServiceCollection services)
        {
            services.AddLogging();

            services
                .AddSingleton<IFileExistsService, FileExistsService>()
                .AddSingleton<IBuildScriptLocator, BuildScriptLocator>()
                .AddSingleton<IScriptLoader, ScriptLoader>()
                .AddSingleton<IFlubuCommandParser, FlubuCommandParser>()
                .AddSingleton<ICommandExecutor, CommandExecutor>()
                .AddSingleton(new CommandLineApplication(false)
                {
                    Name = "dotnet flubu",
                    FullName = ".netcore flubu",
                    Description = "flubu"
                });

            return services;
        }
    }
}
