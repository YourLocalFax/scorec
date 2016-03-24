using System;
using System.Collections.Generic;

namespace ScoreC.Compile.Logging
{
    sealed class Log
    {
        private List<Message> errors   = new List<Message>();
        private List<Message> warnings = new List<Message>();
        private List<Message> messages = new List<Message>();

        public bool HasErrors => errors.Count > 0;
        public bool HasWarnings => warnings.Count > 0;

        public void Clear()
        {
            errors.Clear();
            warnings.Clear();
            messages.Clear();
        }

        public void AddError(Message error) =>
            errors.Add(error);

        public void AddWarning(Message warning) =>
            warnings.Add(warning);

        public void AddMessage(Message message) =>
            messages.Add(message);

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

        public void PrintMessages() =>
            messages.ForEach(m => PrintFormatted(m));
    }
}
