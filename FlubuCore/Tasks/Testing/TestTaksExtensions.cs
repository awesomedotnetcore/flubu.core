﻿using System.IO;
using FlubuCore.Targeting;
using FlubuCore.Tasks.NetCore;

namespace FlubuCore.Tasks.Testing
{
    public static class TestTaksExtensions
    {
        public static ITarget DotnetUnitTest(this ITarget target, params string[] projects)
        {
            foreach (string project in projects)
            {
                target.AddTask(Dotnet.Test(project));
            }

            return target;
        }

        public static ITarget DotnetCoverage(this ITarget target, params string[] projects)
        {
            foreach (string project in projects)
            {
                target.AddTask(DotnetCoverage(project, null, null, null));
            }

            return target;
        }

        public static ITarget DotnetCoverage(this ITarget target, string projectPath, string output, params string[] excludeList)
        {
            target.AddTask(DotnetCoverage(projectPath, output, null, excludeList));
            return target;
        }

        public static ITarget DotnetCoverage(this ITarget target, string projectPath, string[] includeList, string[] excludeList)
        {
            return target.AddTask(DotnetCoverage(projectPath, null, includeList, excludeList));
        }

        public static ITarget ConvertToCobertura(this ITarget target, string input, string output)
        {
            target.AddTask(ConvertToCobertura(input, output));
            return target;
        }

        public static ITarget CoverageReport(this ITarget target, params string[] inputFiles)
        {
            target.AddTask(CoverageReport("coverage", inputFiles));
            return target;
        }

        private static OpenCoverToCoberturaTask ConvertToCobertura(string input, string output)
        {
            return new OpenCoverToCoberturaTask(input, output);
        }

        private static CoverageReportTask CoverageReport(string targetDir, params string[] inputFiles)
        {
            return new CoverageReportTask(inputFiles)
                .TargetFolder(targetDir);
        }

        private static OpenCoverTask DotnetCoverage(string projectPath, string output, string[] includeList, string[] excludeList)
        {
            if (string.IsNullOrEmpty(output))
                output = $"{Path.GetFileNameWithoutExtension(projectPath)}cover.xml";

            OpenCoverTask task = new OpenCoverTask()
                .TestExecutableArgs($"test {projectPath}")
                .Output(output)
                .UseDotNet()
                .AddExclude(excludeList);

            if (includeList == null || includeList.Length <= 0)
                task.IncludeAll();
            else
                task.AddInclude(includeList);

            return task;
        }
    }
}
