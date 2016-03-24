using System.Diagnostics;
using System.Text;

namespace ScoreC.Compile.Logging
{
    using Source;

    sealed partial class Message
    {
        public readonly MessageCode Code;
        public readonly Span Location;
        public readonly string Description;

        private Message(MessageCode code, Span location, string description)
        {
#if DEBUG
            Debug.Assert(description != null);
#endif
            Code = code;
            Location = location;
            Description = description;
        }

        public override string ToString() =>
            ToString(new MessageFormat());

        public string ToString(MessageFormat format)
        {
            var buffer = new StringBuilder();

            if (format.ShowMessageCode)
            {
                buffer.Append(Code.ToCodeString());
                buffer.Append(": ");
            }

            if (format.ShowLocation)
            {
                // TODO(kai): Move the file name into Span and add it to the format.
                buffer.AppendFormat("{0} {1} - ", Location.Map.Name, Location.ToString(new SpanFormat(ommitLocationEnd: format.OmmitLocationEnd)));
            }

            buffer.Append(Description);

            return buffer.ToString();
        }
    }
}
