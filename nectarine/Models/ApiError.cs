using System.Collections.Generic;

namespace NectarineAPI.Models
{
    public class ApiError
    {
        public string? Message { get; set; }

        public IDictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}