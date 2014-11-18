using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntAICompetition.Models
{
    public class TurnStatus
    {
        public int Turn { get; set; }
        public long MillisecondsUntilNextTurn { get; set; }
    }
}