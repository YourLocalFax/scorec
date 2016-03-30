namespace ScoreC.Compile.Logging
{
    class MessageFormat
    {
        public bool ShowLocation;
        public bool ShowFileName;
        public bool OmmitLocationEnd;

        public MessageFormat(bool showLocation = true, bool showFileName = true, bool ommitLocationEnd = true)
        {
            ShowLocation = showLocation;
            ShowFileName = showFileName;
            OmmitLocationEnd = ommitLocationEnd;
        }
    }
}
