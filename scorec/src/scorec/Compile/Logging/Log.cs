using ScoreC.Compile.Source;
using System;
using System.Collections.Generic;

namespace ScoreC.Compile.Logging
{
    sealed class Log
    {
        private List<Message> errors   = new List<Message>();
        private List<Message> warnings = new List<Message>();
        private List<Message> info = new List<Message>();

        public bool HasErrors => errors.Count > 0;
        public bool HasWarnings => warnings.Count > 0;

        public void Clear()
        {
            errors.Clear();
            warnings.Clear();
            info.Clear();
        }

        public void AddError(Span span, string description) =>
            errors.Add(new Message(span, description));

        public void AddError(Span span, string format, params object[] args) =>
            errors.Add(new Message(span, string.Format(format, args)));

        public void AddWarning(Span span, string description) =>
            warnings.Add(new Message(span, description));

        public void AddWarning(Span span, string format, params object[] args) =>
            warnings.Add(new Message(span, string.Format(format, args)));

        public void AddInfo(Span span, string description) =>
            info.Add(new Message(span, description));

        public void AddInfo(Span span, string format, params object[] args) =>
            info.Add(new Message(span, string.Format(format, args)));

        private void PrintFormatted(Message message)
        {
            // TODO(kai): support message formatting here.
            // TODO(kai): support printing this colored for fun.
            Console.WriteLine(message.ToString());
        }

        public void PrintErrors() =>
            errors.ForEach(m => PrintFormatted(m));

        public void PrintWarnings() =>
            warnings.ForEach(m => PrintFormatted(m));

        public void PrintInfo() =>
            info.ForEach(m => PrintFormatted(m));
    }
}
