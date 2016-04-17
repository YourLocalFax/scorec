using System;
using System.Collections.Generic;
using System.IO;

namespace ScoreC
{
    using Compile.Analysis;
    using Compile.Logging;
    using Compile.Source;
    using Compile.SyntaxTree;

    class Project
    {
        public readonly ProjectData Data;

        public readonly Log Log = new Log();

        public readonly List<SourceMap> SourceMaps = new List<SourceMap>();
        private readonly Queue<SourceMap> mapsToParse = new Queue<SourceMap>();

        private SymbolTableBuilder symbolTableBuilder;
        public SymbolTable SymbolTable;

        private string sourceDir;

        public Project(string projectDir, ProjectData data)
        {
            sourceDir = Path.Combine(projectDir, "src");
            Data = data;
        }

        public void AddFile(string filePath)
        {
            if (!Data.IncludedFiles.Contains(filePath))
                Data.IncludedFiles.Add(filePath);
        }

        public void RemoveFile(string filePath, bool deleteFile)
        {
            if (Data.IncludedFiles.Contains(filePath))
            {
                Data.IncludedFiles.Remove(filePath);
                if (deleteFile)
                    File.Delete(Path.Combine(sourceDir, filePath));
            }
        }

        private bool hasFile(string fullPath) =>
            SourceMaps.Exists(file => file.FullPath == fullPath);

        private void LoadSource(string fullPath, Span errorLocation = null)
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

            SourceMaps.Add(sourceMap);
            mapsToParse.Enqueue(sourceMap);
        }

        public void LoadSource(string fromSourceFilePath, string loadPath)
        {
            string fullPath;
            if (!Path.IsPathRooted(loadPath))
            {
                var workingDirectory = Directory.GetParent(fromSourceFilePath).FullName;
                fullPath = Path.Combine(workingDirectory, loadPath);
            }
            else fullPath = loadPath;

            LoadSource(fullPath);
        }

        private void PrintAndClearLog()
        {
            Log.Print();
            // FIXME(kai): Save these??
            Log.Clear();
        }

        public bool BuildAndRun()
        {
            if (!Build())
                return false;
            // TODO(kai): run the program
            return true;
        }

        public bool Build()
        {
            Log.Clear();

            SourceMaps.Clear();
            mapsToParse.Clear();

            symbolTableBuilder = new SymbolTableBuilder();
            SymbolTable = null;

            foreach (var filePath in Data.IncludedFiles)
                LoadSource(Path.Combine(sourceDir, filePath));

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
                //Log.AddInfo(null, "Parsing {0}", file.FullPath);

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
