using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntAICompetition.Models
{
    public class LogonResult
    {
        public string AuthToken { get; set; }
        public int GameId { get; set; }
        public DateTime GameStartTime { get; set; }
    }
}