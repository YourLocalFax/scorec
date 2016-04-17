using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScoreC
{
    using NDesk.Options;

    static class Program
    {
        struct NoCommandArgData
        {
            public bool PrintVersion;
            public bool GetHelp;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            // There must be at least one argument:
            CommandData command;
            if (commands.TryGetValue(args[0], out command))
            {
                var newArgs = new string[args.Length - 1];
                Array.Copy(args, 1, newArgs, 0, newArgs.Length);

                command.Command(newArgs);
            }
            else
            {
                var data = new NoCommandArgData();

                var opts = new OptionSet()
                {
                    { "V|version", b => data.PrintVersion = b != null },
                    { "h|help", b => data.GetHelp = b != null },
                };

                var extra = opts.Parse(args);
                var isValid = extra.Count == 0 && (data.PrintVersion != data.GetHelp);

                if (isValid && data.PrintVersion)
                    PrintVersion();
                else PrintHelp();
            }

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
#endif
        }

        const uint MAJOR = 0;
        const uint MINOR = 1;
        const uint REVISION = 0;

        const string BUILD = "nightly";

        static string VERSION_STRING = MAJOR + "." + MINOR + "." + REVISION + " " + BUILD;

        const string DESC = "Score's project manager";
        const string HELP = @"Usage:
   score <command> [<args> ...]
   score [options]

Options:
   -h, --help        Display this message
   -V, --version     Print version information
   -v, --verbose     Use verbose output
   -q, --quiet       Disable output

Common commands:
   new       Create a new score project
   build     Compile the current project
   run       Compile and execute the current project
   add       Add a new source file to the current project
   include   Include an existing source file to the current project
   remove    Remove a source file from the current project without deleting it
   delete    Delete a source file in the current project

See `score help <command>` for more information on a specific command.";

        struct CommandData
        {
            public CommandFunction Command;
            public string Description;
        }

        delegate void CommandFunction(string[] args);
        static Dictionary<string, CommandData> commands = new Dictionary<string, CommandData>()
        {
            { "new", new CommandData { Command = Cmd_New, Description = @"" } },

            { "add", new CommandData { Command = Cmd_Add, Description = @"" }  },

            { "include", new CommandData { Command = Cmd_Include, Description = @"" }  },

            { "remove", new CommandData { Command = Cmd_Remove, Description = @"" }  },

            { "delete", new CommandData { Command = Cmd_Delete, Description = @"" }  },

            { "build", new CommandData { Command = Cmd_Build, Description = @"" }  },

            { "run", new CommandData { Command = Cmd_Run, Description = @"" }  },
        };

        const string MAIN_TEMPLATE = @"
proc main() {
}
";

        public static bool CreateNewProject(out string message, string projectDir, string projectName, bool forceCreate)
        {
            message = null;

            //Console.WriteLine(projectDir);
            //Console.WriteLine(projectName);

            var existed = Directory.Exists(projectDir);

            // Check if we can actually create a new project
            if (existed &&
                // Check that it's not empty
                Directory.EnumerateFileSystemEntries(projectDir).Any())
            {
                if (forceCreate)
                {
                    try
                    {
                        Directory.Delete(projectDir, true);
                    }
                    catch (Exception e)
                    {
                        message = "Cannot create new project `" + projectName + "`, " + e.Message;
                        return false;
                    }
                }
                else
                {
                    message = "Cannot create new project `" + projectName + "`, directory already exists and is not empty.";
                    return false;
                }
            }

            try
            {
                // Make sure the project directory exists
                Directory.CreateDirectory(projectDir);

                // Create the source directory
                var sourceDir = Path.Combine(projectDir, "src");
                Directory.CreateDirectory(sourceDir);

                // Create the `main` source file
                var mainFile = Path.Combine(sourceDir, "main.score");
                File.WriteAllText(mainFile, MAIN_TEMPLATE);

                // Create the `.sproj` project file
                var projectFile = Path.Combine(projectDir, ".sproj");
                var projectData = new ProjectData();

                projectData.Name = projectName;
                projectData.SourceDir = "src";
                projectData.IncludedFiles.Add("main.score");

                ProjectData.WriteToFile(projectData, projectDir);
            }
            catch (Exception e)
            {
                if (!existed)
                {
                    try
                    {
                        Directory.Delete(projectDir, true);
                    }
                    catch (Exception) { }
                }
                message = "Cannot create new project `" + projectName + "`, " + e.Message;
                return false;
            }

            // Success!
            return true;
        }

        private static ProjectData GetProjectData(out string message, ref string searchDir)
        {
            message = null;

            if (!Directory.Exists(searchDir))
            {
                message = "Failed to locate project directory `" + searchDir + "`, cannot build.";
                return null;
            }

            var currentDir = new DirectoryInfo(searchDir);
            do
            {
                var projectFile = Path.Combine(currentDir.FullName, ".sproj");
                if (File.Exists(projectFile))
                {
                    searchDir = currentDir.FullName;
                    return ProjectData.Parse(projectFile);
                }
                currentDir = currentDir.Parent;
            }
            while (currentDir.Root.Name != currentDir.Name);

            message = "`" + searchDir + "` does not contain an .sproj file, cannot build.";
            return null;
        }

        public static bool Build(out string message, string projectDir)
        {
            var projectData = GetProjectData(out message, ref projectDir);
            if (projectData == null)
                return false;
            var project = new Project(projectDir, projectData);

            message = "Failed to build project!";
            return project.Build();
        }

        public static bool Run(out string message, string projectDir)
        {
            var projectData = GetProjectData(out message, ref projectDir);
            if (projectData == null)
                return false;
            var project = new Project(projectDir, projectData);

            message = "Failed to run project!";
            return project.BuildAndRun();
        }

        public static bool AddNewFile(out string message, Cmd_AddArgData data)
        {
            var projectData = GetProjectData(out message, ref data.ProjectDir);
            if (projectData == null)
                return false;

            message = null;

            var newFilePath = Path.Combine(data.ProjectDir, "src", data.FileName + ".score");
            var newFileDirectory = Directory.GetParent(newFilePath).FullName;

            if (!Directory.Exists(newFileDirectory))
            {
                try
                {
                    Directory.CreateDirectory(newFileDirectory);
                    File.CreateText(newFilePath);
                }
                catch (Exception e)
                {
                    message = e.Message;
                    return false;
                }
            }
            else
            {
                if (File.Exists(newFilePath))
                {
                    if (!data.ForceCreateNew)
                    {
                        message = "File `" + newFilePath + "` already exists, cannot create new file.";
                        return false;
                    }
                    File.Delete(newFilePath);
                }
                File.CreateText(newFilePath);
            }

            projectData.IncludedFiles.Add(data.FileName + ".score");
            ProjectData.WriteToFile(projectData, data.ProjectDir);

            return true;
        }

        public static bool IncludeFile(out string message, Cmd_IncludeArgData data)
        {
            var projectData = GetProjectData(out message, ref data.ProjectDir);
            if (projectData == null)
                return false;

            message = null;

            var dirPath = Path.Combine(data.ProjectDir, "src", data.FileName);
            if (Directory.Exists(dirPath))
            {
                var sourceDir = Path.Combine(data.ProjectDir, "src");

                Action<string> IncludeAll = null;
                IncludeAll = directory =>
                {
                    if (!Directory.Exists(directory))
                        return;
                    var files = Directory.EnumerateFileSystemEntries(directory);
                    foreach (var file in files)
                    {
                        if (Directory.Exists(file) && data.Recursive)
                            IncludeAll(file);
                        else if (Path.GetExtension(file) == ".score")
                        {
                            var relPath = file.Substring(sourceDir.Length + 1);
                            if (!projectData.IncludedFiles.Contains(relPath))
                                projectData.IncludedFiles.Add(relPath);
                        }
                    }
                };

                IncludeAll(dirPath);
            }
            else
            {
                var filePath = dirPath + ".score";

                if (!File.Exists(filePath))
                {
                    message = "Could not find a file at `" + filePath + "`, cannot include it.";
                    return false;
                }

                projectData.IncludedFiles.Add(data.FileName + ".score");
            }

            ProjectData.WriteToFile(projectData, data.ProjectDir);
            return true;
        }

        public static bool DeleteFile(out string message, string projectDir, string fileName)
        {
            var projectData = GetProjectData(out message, ref projectDir);
            if (projectData == null)
                return false;

            message = null;

            var filePath = Path.Combine(projectDir, "src", fileName + ".score");

            if (!File.Exists(filePath))
                return true;

            File.Delete(filePath);

            projectData.IncludedFiles.Remove(fileName + ".score");
            ProjectData.WriteToFile(projectData, projectDir);

            return true;
        }

        public static bool RemoveFile(out string message, string projectDir, string fileName)
        {
            var projectData = GetProjectData(out message, ref projectDir);
            if (projectData == null)
                return false;

            message = null;

            projectData.IncludedFiles.Remove(fileName + ".score");
            ProjectData.WriteToFile(projectData, projectDir);

            return true;
        }









        public static void PrintHelp(string message = DESC)
        {
            Console.WriteLine(message);
            Console.WriteLine();
            Console.WriteLine(HELP);
        }

        public static void PrintVersion()
        {
            Console.WriteLine("score " + VERSION_STRING);
        }

        public struct Cmd_NewArgData
        {
            public bool ForceCreateNew;
        }

        private static void Cmd_New(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score new <name>");
                return;
            }

            var data = new Cmd_NewArgData();

            var opts = new OptionSet()
            {
                { "f|force", b => data.ForceCreateNew = b != null }
                // TODO(kai): bin/lib/gui?/etc.
            };

            var extra = opts.Parse(args);

            if (extra.Count != 1)
            {
                PrintHelp("score new [options] <name>");
                return;
            }

            var projectName = extra.Single();
            var projectDir = Path.GetFullPath(projectName);

            string message;
            if (!CreateNewProject(out message, projectDir, projectName, data.ForceCreateNew))
            {
                Console.WriteLine(message);
                return;
            }
        }

        public struct Cmd_AddArgData
        {
            public string ProjectDir;
            public string FileName;

            public bool ForceCreateNew;
        }

        private static void Cmd_Add(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score add [-f|--force] <file_or_folder>");
                return;
            }

            var data = new Cmd_AddArgData();

            var opts = new OptionSet()
            {
                { "f|force", b => data.ForceCreateNew = b != null }
                // TODO(kai): bin/lib/gui?/etc.
            };

            var extra = opts.Parse(args);

            if (extra.Count != 1)
            {
                PrintHelp("score add [-f|--force] <file_or_folder>");
                return;
            }

            data.ProjectDir = Path.GetFullPath("./");
            data.FileName = extra.Single();

            string message;
            if (!AddNewFile(out message, data))
            {
                Console.WriteLine(message);
                return;
            }
        }

        public struct Cmd_IncludeArgData
        {
            public string ProjectDir;
            public string FileName;

            public bool Recursive;
        }

        private static void Cmd_Include(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score include [-r|--recurse] <file_or_folder>");
                return;
            }

            var data = new Cmd_IncludeArgData();

            var opts = new OptionSet()
            {
                { "r|recurse", b => data.Recursive = b != null }
                // TODO(kai): bin/lib/gui?/etc.
            };

            var extra = opts.Parse(args);

            if (extra.Count != 1)
            {
                PrintHelp("score include [-r|--recurse] <file_or_folder>");
                return;
            }

            data.ProjectDir = Path.GetFullPath("./");
            data.FileName = extra.Single();

            string message;
            if (!IncludeFile(out message, data))
            {
                Console.WriteLine(message);
                return;
            }
        }

        private static void Cmd_Delete(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score delete [options] <file_or_folder>");
                return;
            }

            string projectDir;
            if (args.Length == 2)
            {
                try
                {
                    projectDir = Path.GetFullPath(args.Last());
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not find the path specified.");
                    return;
                }
            }
            else projectDir = Path.GetFullPath("./");

            var fileName = args.First();

            string message;
            if (!DeleteFile(out message, projectDir, fileName))
            {
                Console.WriteLine(message);
                return;
            }
        }

        private static void Cmd_Remove(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score remove [options] <file_or_folder>");
                return;
            }

            string projectDir;
            if (args.Length == 2)
            {
                try
                {
                    projectDir = Path.GetFullPath(args.Last());
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not find the path specified.");
                    return;
                }
            }
            else projectDir = Path.GetFullPath("./");

            var fileName = args.First();

            string message;
            if (!RemoveFile(out message, projectDir, fileName))
            {
                Console.WriteLine(message);
                return;
            }
        }

        private static void Cmd_Build(string[] args)
        {
            if (args.Length > 1)
            {
                PrintHelp("score build [<project_path>]");
                return;
            }

            string projectDir;
            if (args.Length == 1)
            {
                try
                {
                    projectDir = Path.GetFullPath(args.Single());
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not find the path specified.");
                    return;
                }
            }
            else projectDir = Path.GetFullPath("./");

            string message;
            if (!Build(out message, projectDir))
                Console.WriteLine(message);
        }

        private static void Cmd_Run(string[] args)
        {
            if (args.Length > 1)
            {
                PrintHelp("score run [<project_path>]");
                return;
            }

            string projectDir;
            if (args.Length == 1)
            {
                try
                {
                    projectDir = Path.GetFullPath(args.Single());
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not find the path specified.");
                    return;
                }
            }
            else projectDir = Path.GetFullPath("./");

            string message;
            if (!Run(out message, projectDir))
                Console.WriteLine(message);
        }
    }
}
