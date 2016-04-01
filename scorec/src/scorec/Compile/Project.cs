using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ScoreC.Compile
{
    using Logging;
    using Source;
    using SyntaxTree;

    static class Project
    {
        public static readonly Log Log = new Log();

        private static readonly Dictionary<string, SourceMap> loadedFiles = new Dictionary<string, SourceMap>();

        private static bool IsFileLoaded(string fullPath) =>
            loadedFiles.ContainsKey(fullPath);

        private static bool ParseFile(SourceMap map)
        {
            Log.AddInfo(null, "Parsing {0}", map.FullPath);

            Lexer.Lex(map);
            if (Log.HasErrors)
                return false;

            Parser.Parse(map);
            if (Log.HasErrors)
                return false;

            return true;
        }

        public static void Create(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            LoadFile(fullPath);
            Log.Print();
        }

        private static void LoadFile(string fullPath, Span errorLocation = null)
        {
            if (IsFileLoaded(fullPath))
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

            loadedFiles[sourceMap.FullPath] = sourceMap;

            ParseFile(sourceMap);
        }

        public static void LoadFile(string fromSourceFilePath, string loadPath)
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
