namespace ScoreC.Compile.Source
{
    class SpanFormat
    {
        public bool ShowFileName;
        public bool OmmitLocationEnd;

        public SpanFormat(bool showFileName = true, bool ommitLocationEnd = true)
        {
            ShowFileName = showFileName;
            OmmitLocationEnd = ommitLocationEnd;
        }
    }
}
