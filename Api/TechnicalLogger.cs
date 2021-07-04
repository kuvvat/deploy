using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace Api
{
    /// <summary>
    /// Technical logger
    /// </summary>
    public class TechnicalLogger : ITechnicalLogger
    {
        private const string _type = "Type";
        private const string _typeTechnical = "technical";
        private const string _typePerformance = "performance";
        private const string _durationPropertyName = "Duration";
        private const string _userPropertyName = "User";

        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpContextAccessor"></param>
        public TechnicalLogger(ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Error
        /// </summary>
        public void Error(string message, Exception exception = null!, params KeyValuePair<string, object>[] parameters)
        {
            WriteTechnicalLog(message, LogEventLevel.Error, exception, parameters);
        }

        /// <summary>
        /// Information
        /// </summary>
        public void Information(string message, params KeyValuePair<string, object>[] parameters)
        {
            WriteTechnicalLog(message, LogEventLevel.Information, null, parameters);
        }

        /// <summary>
        /// Warning
        /// </summary>
        public void Warning(string message, params KeyValuePair<string, object>[] parameters)
        {
            WriteTechnicalLog(message, LogEventLevel.Warning, null, parameters);
        }

        /// <summary>
        /// Performance
        /// </summary>
        public void Performance(string message, double duration, params KeyValuePair<string, object>[] parameters)
        {
            if (duration <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, $"{nameof(duration)}  must be positive");
            }

            var properties = new List<LogEventProperty>
            {
                new LogEventProperty(_type, new ScalarValue(_typePerformance)),
                new LogEventProperty(_durationPropertyName, new ScalarValue(duration))
            };

            properties.AddRange(parameters.Select(parameter => new LogEventProperty(parameter.Key, new ScalarValue(parameter.Value))));
            properties.AddRange(GetHttpEventProperties());

            WriteLog(message, LogEventLevel.Information, properties);
        }

        private IEnumerable<LogEventProperty> GetHttpEventProperties()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null)
            {
                return Array.Empty<LogEventProperty>();
            }

            var user = context.User?.Identity?.Name;

            return string.IsNullOrEmpty(user) ? Array.Empty<LogEventProperty>() : new[] {new LogEventProperty(_userPropertyName, new ScalarValue(user))};
        }

        private void WriteTechnicalLog(string message, LogEventLevel level, Exception? exception, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var properties = new List<LogEventProperty> {new LogEventProperty(_type, new ScalarValue(_typeTechnical))};

            properties.AddRange(parameters.Select(parameter => new LogEventProperty(parameter.Key, new ScalarValue(parameter.Value))));
            properties.AddRange(GetHttpEventProperties());

            WriteLog(message, level, properties, exception);
        }

        private void WriteLog(string message, LogEventLevel level, IEnumerable<LogEventProperty> properties, Exception? exception = null)
        {
            var messageTemplate = new MessageTemplateParser().Parse(message ?? string.Empty);
            var logEvent = new LogEvent(DateTimeOffset.Now, level, exception, messageTemplate, properties);
            _logger.Write(logEvent);
        }
    }
}
