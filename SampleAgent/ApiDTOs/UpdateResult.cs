using System.Collections.Generic;

namespace SampleAgent.ApiDTOs
{
    public class UpdateResult
    {
        public long TimeToNextTurn { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public List<string> ErrorList { get; set; }
    }
}
