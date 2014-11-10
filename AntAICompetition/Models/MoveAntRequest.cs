using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AntAICompetition.Models
{
    public class MoveAntRequest
    {
        public int AntId { get; set; }
        public string Directions { get; set; }
    }
}