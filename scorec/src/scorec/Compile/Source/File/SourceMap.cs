using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScoreC.Compile.Source
{
    using SyntaxTree;

    sealed class SourceMap
    {
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
        public static SourceMap FromFile(string filePath)
        {
            // TODO(kai): Validate that the path is something that could exist (a valid path).
            var fullPath = Path.GetFullPath(filePath);
            var source = File.ReadAllText(filePath);
            return new SourceMap(fullPath, source.Replace("\r\n", "\n"));
        }

        /*
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
        */

        public readonly string FullPath;
        
        /// <summary>
        /// The name of this SourceFile.
        /// </summary>
        public string Name { get; private set; }
        public string Source { get; private set; }

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

        public Span EndOfSourceSpan;

        // UGRGET(kai): doc this
        public Ast Ast = null;

        private SourceMap(string fullPath, string source)
        {
            FullPath = fullPath;
            Name = Util.MakeRelativePath(Environment.CurrentDirectory, fullPath);
            Source = source;
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
