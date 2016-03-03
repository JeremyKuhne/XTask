// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Settings;
    using Systems.File;
    using Tasks;

    public class AssemblyResolver
    {
        public ResolveEventHandler AssemblyResolveFallback;

        protected HashSet<string> _resolutionPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        protected HashSet<string> _assembliesToResolve = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        protected IFileService FileService { get; private set; }

        protected AssemblyResolver(IFileService fileService)
        {
            FileService = fileService;
        }

        public static AssemblyResolver Create(IArgumentProvider arguments, IFileService fileService)
        {
            AssemblyResolver resolver = new AssemblyResolver(fileService);
            resolver.Initialize(arguments);
            return resolver;
        }

        protected void Initialize(IArgumentProvider arguments)
        {
            string assembliesToResolve = arguments.GetOption<string>(StandardOptions.AssembliesToResolve);
            if (!string.IsNullOrEmpty(assembliesToResolve))
            {
                foreach (string assemblyToResolve in assembliesToResolve.Split(';'))
                {
                    AddAssemblyToResolve(assemblyToResolve);
                }
            }

            string resolutionPaths = arguments.GetOption<string>(StandardOptions.AssemblyResolutionPaths);
            if (!string.IsNullOrEmpty(resolutionPaths))
            {
                foreach (string resolutionPath in resolutionPaths.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(resolutionPath))
                    {
                        string expandedResolutionPath = Environment.ExpandEnvironmentVariables(resolutionPath);
                        AddResolutionPath(expandedResolutionPath);
                    }
                }
            }
        }

        public void AddResolutionPath(string resolutionPath)
        {
            _resolutionPaths.Add(resolutionPath);
        }

        public void AddAssemblyToResolve(string assemblyPattern)
        {
            _assembliesToResolve.Add(assemblyPattern);
        }

        protected virtual Assembly LoadAssemblyFrom(string fullAssemblyPath)
        {
            return Assembly.LoadFrom(fullAssemblyPath);
        }

        protected virtual Uri GetToolLocation()
        {
            return new Uri(typeof(TaskExecution).Assembly.CodeBase);
        }

        public Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            AssemblyName assemblyName = new AssemblyName(args.Name);

            // Only load assemblies we know of
            if (!_assembliesToResolve.Any(assemblyName.Name.StartsWith))
            {
                return null;
            }

            string assemblyFileName = assemblyName.Name + ".dll";

            // CodeBase is the non shadow copied location of the assembly
            Uri toolLocation = GetToolLocation();

            foreach (string path in _resolutionPaths)
            {
                Uri fullAssemblyUri;
                if (Paths.IsPartiallyQualified(path))
                {
                    if (!Uri.TryCreate(toolLocation, path, out fullAssemblyUri)) continue;
                }
                else
                {
                    if (!Uri.TryCreate(path, UriKind.Absolute, out fullAssemblyUri)) continue;
                }

                if (!fullAssemblyUri.IsFile) continue;

                string fullAssemblyPath = Paths.Combine(fullAssemblyUri.LocalPath, assemblyFileName);
                if (FileService.FileExists(fullAssemblyPath))
                {
                    Assembly loadedAssembly = LoadAssemblyFrom(fullAssemblyPath);
                    return loadedAssembly;
                }
            }

            if (AssemblyResolveFallback != null)
            {
                foreach (ResolveEventHandler invoker in AssemblyResolveFallback.GetInvocationList())
                {
                    Assembly assembly = invoker.Invoke(this, args);
                    if (assembly != null)
                    {
                        return assembly;
                    }
                }
            }

            return null;
        }
    }
}
