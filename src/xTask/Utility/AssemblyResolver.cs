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
    using XTask.Systems.File;
    using XTask.Systems.File.Concrete.Flex;
    using XTask.Settings;
    using XTask.Tasks;

    public class AssemblyResolver
    {
        public ResolveEventHandler AssemblyResolveFallback;

        protected HashSet<string> resolutionPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        protected HashSet<string> assembliesToResolve = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        protected IFileService FileService { get; set; }

        protected AssemblyResolver()
        {
            this.FileService = new FileService();
        }

        public static AssemblyResolver Create(IArgumentProvider arguments)
        {
            AssemblyResolver resolver = new AssemblyResolver();
            resolver.Initialize(arguments);
            return resolver;
        }

        protected void Initialize(IArgumentProvider arguments)
        {
            string assembliesToResolve = arguments.GetOption<string>(StandardOptions.AssembliesToResolve);
            if (!String.IsNullOrEmpty(assembliesToResolve))
            {
                foreach (string assemblyToResolve in assembliesToResolve.Split(';'))
                {
                    this.AddAssemblyToResolve(assemblyToResolve);
                }
            }

            string resolutionPaths = arguments.GetOption<string>(StandardOptions.AssemblyResolutionPaths);
            if (!String.IsNullOrEmpty(resolutionPaths))
            {
                foreach (string resolutionPath in resolutionPaths.Split(';'))
                {
                    if (!String.IsNullOrWhiteSpace(resolutionPath))
                    {
                        string expandedResolutionPath = Environment.ExpandEnvironmentVariables(resolutionPath);
                        this.AddResolutionPath(expandedResolutionPath);
                    }
                }
            }
        }

        public void AddResolutionPath(string resolutionPath)
        {
            this.resolutionPaths.Add(resolutionPath);
        }

        public void AddAssemblyToResolve(string assemblyPattern)
        {
            this.assembliesToResolve.Add(assemblyPattern);
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
            if (String.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            AssemblyName assemblyName = new AssemblyName(args.Name);

            // Only load assemblies we know of
            if (!this.assembliesToResolve.Any(assemblyName.Name.StartsWith))
            {
                return null;
            }

            string assemblyFileName = assemblyName.Name + ".dll";

            // CodeBase is the non shadow copied location of the assembly
            Uri toolLocation = this.GetToolLocation();

            foreach (string path in this.resolutionPaths)
            {
                Uri fullAssemblyUri;
                if (Paths.IsPathRelative(path))
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
                    Assembly loadedAssembly = this.LoadAssemblyFrom(fullAssemblyPath);
                    return loadedAssembly;
                }
            }

            if (this.AssemblyResolveFallback != null)
            {
                foreach (ResolveEventHandler invoker in this.AssemblyResolveFallback.GetInvocationList())
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
