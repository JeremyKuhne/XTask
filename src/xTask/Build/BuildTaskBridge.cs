// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using XTask.Systems.File;
using XTask.Settings;
using XTask.Tasks;
using MSBuildFramework = Microsoft.Build.Framework;

namespace XTask.Build
{
    /// <summary>
    ///  Core implementation of MSBuild support for tasks, derive and provide the task service and
    ///  any register any additional property viewers.
    /// </summary>
    public abstract class BuildTaskBridge : MSBuildFramework.ITask, ITaskOutputHandler
    {
        private readonly List<MSBuildFramework.ITaskItem> _output = new();
        private readonly IFileService _fileService;

        protected BuildTaskBridge(IFileService fileService)
        {
            _fileService = fileService;
            PropertyViewProvider = new PropertyViewProvider();
        }

        // BuildEngine and HostObject are set by MSBuild
        public MSBuildFramework.IBuildEngine BuildEngine { get; set; }
        public MSBuildFramework.ITaskHost HostObject { get; set; }

        protected IPropertyViewProvider PropertyViewProvider { get; private set; }

        /// <summary>
        ///  Name of the task (as registered in ITaskRegistry)
        /// </summary>
        [MSBuildFramework.Required]
        public string TaskName { get; set; }

        /// <summary>
        ///  Direct targets, if any. (Parameters that aren't options.)
        /// </summary>
        public string[] Targets { get; set; }

        /// <summary>
        ///  Options in XML format. Tags are option names, content is the value.
        /// </summary>
        public string Options { get; set; }

        /// <summary>
        ///  The exit code from the task.
        /// </summary>
        [MSBuildFramework.Output]
        public string ExitCode { get; private set; }

        /// <summary>
        ///  Output from the task, if any.
        /// </summary>
        [MSBuildFramework.Output]
        public MSBuildFramework.ITaskItem[] Output => _output.ToArray();

        public abstract ITaskService GetTaskService(ref IArgumentProvider argumentProvider);

        public bool Execute()
        {
            // The equivalent of Main() for console access
            ExitCode result = Tasks.ExitCode.GeneralFailure;

            IArgumentProvider argumentProvider = new BuildArgumentParser(TaskName, Targets, Options, _fileService);

            using (ITaskService taskService = GetTaskService(ref argumentProvider))
            {
                BuildTaskExecution execution = new(
                    buildEngine: BuildEngine,
                    outputHandler: this,
                    argumentProvider: argumentProvider,
                    taskRegistry: taskService.TaskRegistry);

                // We need to be on an STA thread to interact with the clipboard. We don't
                // control the main thread when executed by MSBuild, so fire off an STA
                // thread to do the actual work.
                Thread thread = new(() => result = execution.ExecuteTask());
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }

            ExitCode = ((int)result).ToString();
            return result == Tasks.ExitCode.Success;
        }

        public void HandleOutput(object value)
        {
            // Transform the output objects to MSBuild ITaskItems

            IPropertyView view = PropertyViewProvider.GetTypeView(value);

            MSBuildFramework.ITaskItem taskItem = new TaskItem { ItemSpec = view.ToString() };
            foreach (var property in view)
            {
                taskItem.SetMetadata(property.Name, property.Value.ToString());
            }

            _output.Add(taskItem);
        }
    }
}