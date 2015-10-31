// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using Logging;
    using Services;
    using Settings;

    /// <summary>
    /// Base class for task interaction support
    /// </summary>
    public abstract class TaskInteraction : ITaskInteraction
    {
        private ITypedServiceProvider services;

        protected TaskInteraction(IArgumentProvider arguments, ITypedServiceProvider services)
        {
            this.Arguments = arguments;
            this.services = services;
        }

        public IArgumentProvider Arguments { get; private set; }
        public ILoggers Loggers { get { return this.GetService<ILoggers>(); } }

        protected abstract ILoggers GetDefaultLoggers();

        public virtual T GetService<T>() where T : class
        {
            T service = this.services?.GetService<T>() ?? FlexServiceProvider.Services.GetService<T>();
            if (service != null) return service;

            if (typeof(T) == typeof(ILoggers))
            {
                return (T)this.GetDefaultLoggers();
            }
            return null;
        }

        public virtual void Output(object value)
        {
            // Do nothing by default
        }
    }
}
