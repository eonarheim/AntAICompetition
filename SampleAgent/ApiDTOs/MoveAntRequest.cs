namespace SampleAgent.ApiDTOs
{
    
    public class MoveAntRequest
    {
        public int AntId { get; set; }
        // possible directions are up, down, left, and right
        public string Direction { get; set; }
    }
}
