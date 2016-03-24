using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScoreC.Compile.Source
{
    using SyntaxTree;

    sealed class SourceMap
    {
        // from http://stackoverflow.com/questions/703281/getting-path-relative-to-the-current-working-directory/703290#703290
        private static string MakeRelativePath(string workingDirectory, string fullPath)
        {
            string result = string.Empty;
            int offset;

            // this is the easy case.  The file is inside of the working directory.
            if (fullPath.StartsWith(workingDirectory))
            {
                return fullPath.Substring(workingDirectory.Length + 1);
            }

            // the hard case has to back out of the working directory
            string[] baseDirs = workingDirectory.Split(new char[] { ':', '\\', '/' });
            string[] fileDirs = fullPath.Split(new char[] { ':', '\\', '/' });

            // if we failed to split (empty strings?) or the drive letter does not match
            if (baseDirs.Length <= 0 || fileDirs.Length <= 0 || baseDirs[0] != fileDirs[0])
            {
                // can't create a relative path between separate harddrives/partitions.
                return fullPath;
            }

            // skip all leading directories that match
            for (offset = 1; offset < baseDirs.Length; offset++)
            {
                if (baseDirs[offset] != fileDirs[offset])
                    break;
            }

            // back out of the working directory
            for (int i = 0; i < (baseDirs.Length - offset); i++)
            {
                result += "..\\";
            }

            // step into the file path
            for (int i = offset; i < fileDirs.Length - 1; i++)
            {
                result += fileDirs[i] + "\\";
            }

            // append the file
            result += fileDirs[fileDirs.Length - 1];

            return result;
        }

        private static int unnamedCount = 0;

        /// <summary>
        /// Creates a new SourceFile pointing to the contents of the given file.
        /// 
        /// This method does not check if the file exists until the source is requested.
        /// Because of this it's okay to create a SourceFile that points to a file that does not exist,
        ///  so long as the source is not requested prior to the source file being created.
        /// 
        /// To request the source, simply access the Source property.
        /// When requested, the file is checked for existence and read then fully.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static SourceMap FromFile(string filePath, string name = null)
        {
            // TODO(kai): Validate that the path is something that could exist (a valid path).
            var absPath = Path.GetFullPath(filePath);
            var relPath = MakeRelativePath(Environment.CurrentDirectory, absPath);
            return new SourceMap(relPath, () =>
            {
                // FIXME(kai): Do sanity checking, probably
                var source = File.ReadAllText(filePath);
                return source.Replace("\r\n", "\n");
            });
        }

        /// <summary>
        /// Creates a new SourceFile pointing to the contents of the given stream.
        /// 
        /// Unlike the method that acts on a file, this method will read the stream
        ///  fully and store the resulting string.
        /// </summary>
        /// <exception cref="IOException">If reading from the stream throws an IOException.</exception>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static SourceMap FromStream(Stream stream, string name = null)
        {
            if (name == null)
                name = "<stream-" + (++unnamedCount) + ">";
            var reader = new StreamReader(stream);
            var source = reader.ReadToEnd().Replace("\r\n", "\n");
            return new SourceMap(name, () => source);
        }
        
        /// <summary>
        /// The name of this SourceFile.
        /// </summary>
        public string Name { get; private set; }
        private readonly Func<string> sourceGenerator;

        private string sourceCached = null;
        /// <summary>
        /// Requests and returns the source of this SourceFile.
        /// </summary>
        public string Source
        {
            get
            {
                if (sourceCached == null)
                    sourceCached = sourceGenerator();
                return sourceCached;
            }
        }

        private string[] sourceLinesCached = null;
        /// <summary>
        /// Requests this SourceFile's source and returns an array of all lines.
        /// </summary>
        public string[] SourceLines
        {
            get
            {
                if (sourceLinesCached == null)
                    sourceLinesCached = Source.Split('\n');
                return sourceLinesCached;
            }
        }

        // UGRGET(kai): doc this
        public List<Token> Tokens = null;

        // UGRGET(kai): doc this
        public Ast Ast = null;

        private SourceMap(string name, Func<string> sourceGenerator)
        {
            Name = name;
            this.sourceGenerator = sourceGenerator;
        }

        /// <summary>
        /// Returns all source the given span covers a 
        /// </summary>
        /// <exception cref="ArgumentException">If the span doesn't come from the SourceFile.</exception>
        /// <param name="span"></param>
        /// <returns></returns>
        public string GetSourceAtSpan(Span span)
        {
            if (span.Map != this)
                throw new ArgumentException("Span didn't come from this source file.");

            var lineCount = span.EndLine - span.Line + 1;
            if (lineCount == 1)
                return SourceLines[span.Line].Substring(span.Column, span.EndColumn - span.Column);
            else
            {
                var result = new string[lineCount];
                Array.Copy(SourceLines, span.Line, result, 0, lineCount);
                result[0] = result[0].Substring(span.Column);
                result[lineCount - 1] = result[lineCount - 1].Substring(0, span.EndColumn);
                return string.Join("\n", result);
            }
        }
    }
}
