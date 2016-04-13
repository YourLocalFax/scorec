using System;

using LLVMSharp;

using IniParser;

namespace ScoreC
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Compile;
    using IniParser.Model;

    static class Program
    {
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
   remove    Remove a source fil from the current project

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

            { "remove", new CommandData { Command = Cmd_Remove, Description = @"" }  },

            { "build", new CommandData { Command = Cmd_Build, Description = @"" }  },

            { "run", new CommandData { Command = Cmd_Run, Description = @"" }  },
        };

        public static void Execute(string[] args)
        {
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
                return;
            }
            else
            {
                if (args.Length == 1 && (args[0] == "-V" || args[0] == "--version"))
                {
                    PrintVersion();
                    return;
                }

                PrintHelp();
            }
        }

        private static void Cmd_New(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score new <name>");
                return;
            }

            var forceCreate = false;

            if (args.Length == 2)
            {
                if (args.First() == "-f" || args.First() == "--force")
                    forceCreate = true;
                else
                {
                    PrintHelp("score new <name>");
                    return;
                }
            }

            var projectName = args.Last();
            var projectDir = Path.GetFullPath(projectName);

            Project.CreateNew(projectDir, projectName, forceCreate);
        }

        private static void Cmd_Add(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score add [options] <file_or_folder>");
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

            var project = new Project(projectDir);
            var filePath = Path.Combine(project.SourceDirectory, fileName + ".score");

            Directory.CreateDirectory(Directory.GetParent(filePath).FullName);
            File.CreateText(filePath)?.Close();

            project.AddFile(fileName + ".score");
            project.UpdateSprojFile();
        }

        private static void Cmd_Remove(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp("score add [options] <file_or_folder>");
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

            var project = new Project(projectDir);
            var filePath = Path.Combine(project.SourceDirectory, fileName + ".score");

            project.RemoveFile(fileName + ".score", true);
            project.UpdateSprojFile();
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

            Build(projectDir);
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

            BuildAndRun(projectDir);
        }

        private static void Build(string projectDir)
        {
            var project = new Project(projectDir);
            project.Build();
        }

        private static void BuildAndRun(string projectDir)
        {
            var project = new Project(projectDir);
            project.Build();
            project.Run();
        }
    }
}
