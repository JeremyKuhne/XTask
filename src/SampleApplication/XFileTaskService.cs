// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using XFile.Tasks;

namespace XFile;

/// <summary>
///  Example task service.
/// </summary>
public class XFileTaskService : TaskService
{
    private XFileTaskService()
        : base (XFileStrings.HelpGeneral)
    {
    }

    public static ITaskService Create()
    {
        XFileTaskService taskService = new();
        taskService.Initialize();
        return taskService;
    }

    public override void Initialize()
    {
        SimpleTaskRegistry registry = GetTaskRegistry();

        // Debugger.Launch();

        registry.RegisterTask(() => new DirectoryTask(), "directory", "dir", "ls");
        registry.RegisterTask(() => new ChangeDirectoryTask(), "changedirectory", "chdir", "cd");
        registry.RegisterTask(() => new PrintCurrentDirectoryTask(), "printcurrentdirectory", "pwd", "pcd");
        registry.RegisterTask(() => new MakeDirectoryTask(), "makedirectory", "mkdir", "md");
        registry.RegisterTask(() => new RemoveDirectoryTask(), "removedirectory", "rmdir", "rd");
        registry.RegisterTask(() => new FileInfoTask(), "fileinfo", "fi");
        registry.RegisterTask(() => new ListStreamsTask(), "liststreams", "streams");
        registry.RegisterTask(() => new TypeTask(), "type");
        registry.RegisterTask(() => new EchoTask(), "echo");
        registry.RegisterTask(() => new CopyTask(), "copy", "cp");
        registry.RegisterTask(() => new FullPathTask(), "fullpath", "fp");
        registry.RegisterTask(() => new LongPathTask(), "longpath", "lp");
        registry.RegisterTask(() => new ShortPathTask(), "shortpath", "sp");
        registry.RegisterTask(() => new FinalPathTask(), "finalpath", "final");
        registry.RegisterTask(() => new MountPointTask(), "volumepathname", "vpn");
        registry.RegisterTask(() => new VolumeNameTask(), "volumename", "vn");
        registry.RegisterTask(() => new VolumeMountPointsTask(), "volumemountpoints", "mountpoints", "mp");
        registry.RegisterTask(() => new VolumeInformationTask(), "volumeinformation", "volumeinfo", "vi");
        registry.RegisterTask(() => new DosAliasTask(), "dosalias", "da");
        registry.RegisterTask(() => new LogicalDriveStringsTask(), "logicaldrivestrings", "lds");
        registry.RegisterTask(() => new TestTask(), "test");

        base.Initialize();
    }
}
