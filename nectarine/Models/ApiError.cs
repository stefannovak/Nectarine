using System.Collections.Generic;

namespace NectarineAPI.Models
{
    public class ApiError
    {
        public ApiError(string message)
        {
            Message = message;
        }

        public ApiError(string message, IDictionary<string, string> errors)
        {
            Message = message;
            Errors = errors;
        }

        public string? Message { get; set; }

        public IDictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}