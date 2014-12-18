using System.Collections.Generic;

namespace SampleAgent.ApiDTOs
{
    public class UpdateRequest
    {
        public string AuthToken { get; set; }
        public int GameId { get; set; }
        public List<MoveAntRequest> MoveAntRequests { get; set; }
    }
}
