using System;

namespace SampleAgent.ApiDTOs
{
    public class LogonResult
    {
        public string AuthToken { get; set; }
        public int GameId { get; set; }
        public DateTime GameStartTime { get; set; }
    }
}
