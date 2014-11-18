using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAgent
{
    public class UpdateRequest
    {
        public string AuthToken { get; set; }
        public int GameId { get; set; }
        public List<MoveAntRequest> MoveAntRequests { get; set; }
    }
}
