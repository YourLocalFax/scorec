using ScoreC.Compile.Source;
using System;
using System.Collections.Generic;

namespace ScoreC.Compile.Logging
{
    sealed class Log
    {
        private List<Message> messages   = new List<Message>();

        public bool HasErrors { get; private set; }
        public bool HasWarnings { get; private set; }

        public void Clear()
        {
            HasErrors = false;
            HasWarnings = false;
            messages.Clear();
        }

        public void AddError(Span span, string description)
        {
            HasErrors = true;
            messages.Add(new Message(span, description));
        }

        public void AddError(Span span, string format, params object[] args)
        {
            HasErrors = true;
            messages.Add(new Message(span, string.Format(format, args)));
        }

        public void AddWarning(Span span, string description)
        {
            HasWarnings = true;
            messages.Add(new Message(span, description));
        }

        public void AddWarning(Span span, string format, params object[] args)
        {
            HasWarnings = true;
            messages.Add(new Message(span, string.Format(format, args)));
        }

        public void AddInfo(Span span, string description) =>
            messages.Add(new Message(span, description));

        public void AddInfo(Span span, string format, params object[] args) =>
            messages.Add(new Message(span, string.Format(format, args)));

        private void PrintFormatted(Message message)
        {
            // TODO(kai): support message formatting here.
            // TODO(kai): support printing this colored for fun.
            Console.WriteLine(message.ToString());
        }

        public void Print() =>
            messages.ForEach(m => PrintFormatted(m));
    }
}
