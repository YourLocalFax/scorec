using System;
using System.Globalization;
using System.Text;

namespace ScoreC.Compile.Source
{
    struct Span
    {
        /// <summary>
        /// The source file this Span describes.
        /// </summary>
        public SourceMap Map;

        /// <summary>
        /// The line this span starts on.
        /// </summary>
        public int Line;
        /// <summary>
        /// The column this span starts on.
        /// </summary>
        public int Column;

        /// <summary>
        /// If this is -1, this span represents a single character.
        /// </summary>
        private int endLine;
        /// <summary>
        /// The line this span ends on.
        /// </summary>
        public int EndLine => endLine >= 0 ? endLine : Line;
        /// <summary>
        /// If this is -1, this span represents a single character.
        /// </summary>
        private int endColumn;
        /// <summary>
        /// The column this span ends on.
        /// </summary>
        public int EndColumn => endColumn >= 0 ? endColumn : Column + 1;

        public Span(SourceMap map, int line, int column, int endLine = -1, int endColumn = -1)
        {
            Map = map;

            Line = line;
            Column = column;

            this.endLine = endLine;
            this.endColumn = endColumn;

            // Simple sanity checks plz
            if (endLine != -1)
            {
                if (EndLine < Line)
                    throw new ArgumentException("End line cannot be less than start line.");
                else if (EndLine == Line && (endColumn != -1 && EndColumn < Column))
                    throw new ArgumentException("End column cannot be less than start column on the same line.");
            }
        }

        public override string ToString() =>
            ToString(new SpanFormat());

        public string ToString(SpanFormat format)
        {
            if (endLine != -1 && !format.OmmitLocationEnd)
                return string.Format(CultureInfo.CurrentCulture, "{0}, {1} : {2}, {3}", Line + 1, Column + 1, EndLine + 1, EndColumn + 1);
            return string.Format(CultureInfo.CurrentCulture, "{0}, {1}", Line + 1, Column + 1);
        }
    }
}
