using System.Collections.Generic;

namespace nectarineAPI.Models
{
    public class ApiError
    {
        public string? Message { get; set; }

        public IDictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}