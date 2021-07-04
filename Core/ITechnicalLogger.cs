using System;
using System.Collections.Generic;

namespace Core
{
    public interface ITechnicalLogger
    {
        void Information(string message, params KeyValuePair<string, object>[] parameters);

        void Warning(string message, params KeyValuePair<string, object>[] parameters);

        void Error(string message, Exception exception, params KeyValuePair<string, object>[] parameters);

        void Performance(string message, double duration, params KeyValuePair<string, object>[] parameters);
    }
}
