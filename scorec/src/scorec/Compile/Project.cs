using System;
using System.Collections.Generic;
using System.IO;

namespace ScoreC.Compile
{
    using Analysis;
    using Logging;
    using Source;
    using SyntaxTree;

    class Project
    {
        public readonly Log Log = new Log();

        public readonly List<SourceMap> Files = new List<SourceMap>();
        private readonly Queue<SourceMap> mapsToParse = new Queue<SourceMap>();

        private bool hasFile(string fullPath) =>
            Files.Exists(file => file.FullPath == fullPath);

        private SymbolTableBuilder symbolTableBuilder = new SymbolTableBuilder();
        public SymbolTable SymbolTable;

        public Project(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            LoadFile(fullPath);
        }

        private void PrintAndClearLog()
        {
            Log.Print();
            // FIXME(kai): Save these??
            Log.Clear();
        }

        public bool Process()
        {
            // defer PrintAndClearLog() /# plz
            // or actually:

            /*
            defer {
                Log.Print();
                Log.Clear();
            }
            */

            // that eliminates the extra function <3

            var steps = new Func<bool>[]
            {
                () => Parse(),
                () => Validate(),
                () => Compile(),
            };

            foreach (var step in steps)
            {
                if (step())
                {
                    PrintAndClearLog();
                    return false;
                }
            }

            PrintAndClearLog();

            Console.WriteLine();
            Console.WriteLine(SymbolTable);

            Console.WriteLine();

            return true;
        }

        // returns false on succes, true on failure
        private bool Parse()
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

            return Log.HasErrors;
        }

        // returns false on succes, true on failure
        private bool Validate()
        {
            SemanticAnalyzer.Analyze(this);
            SymbolicResolver.Resolve(this);

            return Log.HasErrors;
        }

        // returns false on succes, true on failure
        private bool Compile()
        {
            return Log.HasErrors;
        }

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
    }
}
