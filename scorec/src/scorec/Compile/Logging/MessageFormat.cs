namespace ScoreC.Compile.Logging
{
    class MessageFormat
    {
        public bool ShowMessageCode;
        public bool ShowLocation;
        public bool ShowFileName;
        public bool OmmitLocationEnd;

        public MessageFormat(bool showMessageCode = true, bool showLocation = true, bool showFileName = true, bool ommitLocationEnd = true)
        {
            ShowMessageCode = showMessageCode;
            ShowLocation = showLocation;
            ShowFileName = showFileName;
            OmmitLocationEnd = ommitLocationEnd;
        }
    }
}
