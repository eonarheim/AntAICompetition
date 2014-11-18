using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAgent
{
    
    public class MoveRequest
    {
        public int AntId { get; set; }
        // possible directions are up, down, left, and right
        public string Direction { get; set; }
    }
}
