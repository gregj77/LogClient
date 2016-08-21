
using System;
using System.Globalization;
using System.Linq;

namespace LogClient
{
    /// <summary>
    /// Class with trace data.
    /// </summary>
    public class TraceData
    {
        /// <summary>
        /// Array with data fields.
        /// </summary>
        private string[] fields;

        /// <summary>
        /// Initializes a new instance of the TraceData class.
        /// </summary>
        /// <param name="parts">String array with data chunks.</param>
        public TraceData(string[] parts)
        {
            DateTime currentTime;
            this.fields = parts;
            if (parts.Length < 1)
            {
                return;
            }

            this.Id = parts[0];

            if (parts.Length < 2)
            {
                return;
            }

            this.Category = parts[1];

            if (parts.Length < 3)
            {
                return;
            }

            this.Severity = parts[2].ToUpperInvariant().Trim();

            if (parts.Length < 4)
            {
                return;
            }

            this.Timestamp = DateTime.TryParse(parts[3], CultureInfo.CurrentUICulture, DateTimeStyles.None, out currentTime) ? 
                currentTime : DateTime.Now;

            if (parts.Length < 5)
            {
                return;
            }

            this.Title = parts[4];

            if (parts.Length < 6)
            {
                return;
            }

            this.Message = parts[5];

            if (parts.Length < 7)
            {
                return;
            }

            this.TraceId = parts[6];
        }

        /// <summary>
        /// Gets data Id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets category name.
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Gets severity level.
        /// </summary>
        public string Severity { get; private set; }

        /// <summary>
        /// Gets data timestamp.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Gets data title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets data message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets a trace id.
        /// </summary>
        public string TraceId { get; private set; }

        /// <summary>
        /// Filter out fields.
        /// </summary>
        /// <param name="filter">Filter string.</param>
        /// <returns>Returns true if any field contains filter text.</returns>
        public bool MatchesFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }
            return this.fields.Any(p => p.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}