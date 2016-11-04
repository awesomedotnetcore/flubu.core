﻿using System.Collections.Generic;
using FlubuCore.Context;
using FlubuCore.IO;
using FlubuCore.Packaging;

namespace FlubuCore.Tasks.Packaging
{
    public class PackageTask : TaskBase
    {
        private List<SourcePackagingInfo> _sourcePackagingInfos;

        private FullPath _destinationRootDir;

        private string _zipFileName;

        public PackageTask(string destinationRootDir)
        {
            _sourcePackagingInfos = new List<SourcePackagingInfo>();
            _destinationRootDir = new FullPath(destinationRootDir);
        }

        private bool ShouldPackageBeZipped => !string.IsNullOrEmpty(_zipFileName);

        public PackageTask AddDirectoryToPackage(string sourceId, string sourceDirectoryPath, string destinationDirectory, bool recursive = false)
        {
            var directoryToPackage = new SourcePackagingInfo(sourceId, SourceType.Directory, sourceDirectoryPath, destinationDirectory);
            directoryToPackage.Recursive = recursive;
            _sourcePackagingInfos.Add(directoryToPackage);
            return this;
        }

        public PackageTask AddDirectoryToPackage(string sourceId, string sourceDirectoryPath, string destinationDirectory, bool recursive, params IFileFilter[] fileFilters)
        {
            var directoryToPackage = new SourcePackagingInfo(sourceId, SourceType.Directory, sourceDirectoryPath, destinationDirectory);
            directoryToPackage.Recursive = recursive;
            foreach (var filter in fileFilters)
            {
                directoryToPackage.FileFilters.Add(filter);
            }

            _sourcePackagingInfos.Add(directoryToPackage);
            return this;
        }

        public PackageTask AddFileToPackage(string sourceId, string sourceFilePath, string destinationDirectory)
        {
            _sourcePackagingInfos.Add(new SourcePackagingInfo(sourceId, SourceType.File, sourceFilePath, destinationDirectory));
            return this;
        }

        public PackageTask ZipPackage(string zipFileName)
        {
            _zipFileName = zipFileName;
            return this;
        }

        protected override int DoExecute(ITaskContext context)
        {
            if (_sourcePackagingInfos.Count == 0)
            {
                return 0;
            }

            ICopier copier = new Copier(context);
            IZipper zipper = new Zipper(context);
            IDirectoryFilesLister directoryFilesLister = new DirectoryFilesLister();
            StandardPackageDef packageDef = new StandardPackageDef();
            CopyProcessor copyProcessor = new CopyProcessor(
            context,
            copier,
            _destinationRootDir);
            List<string> sourceIds = new List<string>();
            foreach (var sourceToPackage in _sourcePackagingInfos)
            {
                if (sourceToPackage.SourceType == SourceType.Directory)
                {
                    DirectorySource directorySource = new DirectorySource(context, directoryFilesLister, sourceToPackage.SourceId, new FullPath(sourceToPackage.SourcePath), sourceToPackage.Recursive);
                    directorySource.SetFilter(sourceToPackage.FileFilters);
                    packageDef.AddFilesSource(directorySource);
                }
                else
                {
                    SingleFileSource fileSource = new SingleFileSource(sourceToPackage.SourceId, new FileFullPath(sourceToPackage.SourcePath));
                    packageDef.AddFilesSource(fileSource);
                }

                copyProcessor.AddTransformation(sourceToPackage.SourceId, sourceToPackage.DestinationPath);
                sourceIds.Add(sourceToPackage.SourceId);
            }

            IPackageDef copiedPackageDef = copyProcessor.Process(packageDef);

            if (ShouldPackageBeZipped)
            {
                ZipProcessor zipProcessor = new ZipProcessor(context, zipper, new FileFullPath(_zipFileName), _destinationRootDir, sourceIds);
                zipProcessor.Process(copiedPackageDef);
            }

            return 0;
        }
    }
}
