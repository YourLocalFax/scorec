using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using IniParser;
using IniParser.Model;

namespace ScoreC
{
    using System.Diagnostics;
    using Compile.Analysis;
    using Compile.Logging;
    using Compile.Source;
    using Compile.SyntaxTree;

    class Project
    {
        private readonly IniData data;

        public readonly string RootDirectory, Name;

        public readonly Log Log = new Log();

        public readonly List<SourceMap> Files = new List<SourceMap>();
        private readonly Queue<SourceMap> mapsToParse = new Queue<SourceMap>();

        private SymbolTableBuilder symbolTableBuilder;
        public SymbolTable SymbolTable;

        public string MainFile
        {
            get
            {
                string result;
                if (data.TryGetKey("main_file", out result))
                    return Path.Combine(SourceDirectory, result);
                else throw new ArgumentException(Name + " does not specify a main file.");
            }
            set { data.Global.AddKey("main_file", value); }
        }

        public string SourceDirectory => Path.Combine(RootDirectory, "src");

        public Project(string projectDir)
        {
            RootDirectory = projectDir;

            var sprojFiles = Directory.GetFiles(projectDir, "*.sproj");
            if (sprojFiles.Length > 1)
                throw new ArgumentException(projectDir + " contains multiple score project (.sproj) files.");

            string sprojFile;
            if (sprojFiles.Length == 1)
                sprojFile = sprojFiles.Single();
            else
            {;
                sprojFile = Path.Combine(projectDir, Path.GetFileName(projectDir) + ".sproj");
                File.CreateText(sprojFile).Close();
            }
            Name = Path.GetFileNameWithoutExtension(sprojFile);

            data = new FileIniDataParser().ReadFile(sprojFile);
        }

        public void AddFile(string filePath)
        {
            string fileData;
            if (data.TryGetKey("files", out fileData))
                fileData = "";
            if (string.IsNullOrWhiteSpace(fileData))
                fileData = filePath;
            else fileData = fileData + Path.PathSeparator + filePath;
            data.Global.AddKey("files", fileData);
        }

        public void RemoveFile(string filePath, bool deleteFile)
        {
            string fileData;
            if (!data.TryGetKey("files", out fileData))
                return;
            List<string> files = new List<string>(fileData.Split(Path.PathSeparator));
            files.Remove(filePath);
            data.Global.RemoveKey("files");
            var newFileData = string.Join(Path.PathSeparator.ToString(), files);
            if (!string.IsNullOrWhiteSpace(newFileData))
                data.Global.AddKey("files", newFileData);
            if (deleteFile)
                File.Delete(Path.Combine(SourceDirectory, filePath));
        }

        public void UpdateSprojFile()
        {
            var iniData = data.ToString();
            File.WriteAllText(Path.Combine(RootDirectory, Name + ".sproj"), iniData);
        }

        private bool hasFile(string fullPath) =>
            Files.Exists(file => file.FullPath == fullPath);

        private void LoadFile(string fullPath, Span errorLocation = null)
        {
            if (hasFile(fullPath))
                return;

            // TODO(kai): Determine if the file exists!

            SourceMap sourceMap;
            try
            {
                sourceMap = SourceMap.FromFile(fullPath);
            }
            catch (IOException e)
            {
                Log.AddError(errorLocation, e.Message);
                return;
            }

            Files.Add(sourceMap);
            mapsToParse.Enqueue(sourceMap);
        }

        public void LoadFile(string fromSourceFilePath, string loadPath)
        {
            string fullPath;
            if (!Path.IsPathRooted(loadPath))
            {
                var workingDirectory = Directory.GetParent(fromSourceFilePath).FullName;
                fullPath = Path.Combine(workingDirectory, loadPath);
            }
            else fullPath = loadPath;

            LoadFile(fullPath);
        }

        private void PrintAndClearLog()
        {
            Log.Print();
            // FIXME(kai): Save these??
            Log.Clear();
        }

        public bool Build()
        {
            Log.Clear();

            Files.Clear();
            mapsToParse.Clear();

            symbolTableBuilder = new SymbolTableBuilder();
            SymbolTable = null;

            LoadFile(MainFile);

            string fileData;
            if (data.TryGetKey("files", out fileData))
            {
                foreach (var filePath in fileData.Split(Path.PathSeparator))
                    LoadFile(Path.Combine(SourceDirectory, filePath));
            }

            var steps = new Action[]
            {
                Parse, Validate, Compile,
            };

            foreach (var step in steps)
            {
                step();
                if (Log.HasErrors)
                {
                    PrintAndClearLog();
                    return false;
                }
            }

            PrintAndClearLog();
            return true;
        }

        private void Parse()
        {
            while (mapsToParse.Count > 0)
            {
                var file = mapsToParse.Dequeue();
                Log.AddInfo(null, "Parsing {0}", file.FullPath);

                Lexer.Lex(Log, file);
                if (Log.HasErrors)
                    continue;

                Parser.Parse(this, file);
                if (Log.HasErrors)
                    continue;
            }
        }

        private void Validate()
        {
            SemanticAnalyzer.Analyze(this);
            Console.WriteLine(SymbolTable);
            SymbolicResolver.Resolve(this);
        }

        private void Compile()
        {
        }

        public void Run()
        {
        }
    }
}
