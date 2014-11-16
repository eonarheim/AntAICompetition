using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntAICompetition.Models
{
    public class UpdateResult
    {
        public int TimeToNextTurn { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public List<string> ErrorList { get; set; }
    }
}