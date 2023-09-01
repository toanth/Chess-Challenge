
namespace ChessChallenge.API
{
    public struct BotInfo
    {

        public BotInfo(int depth = -1, int evaluation = 0)
        {
            Depth = depth;
            Evaluation = evaluation;
        }

        public int Depth = -1;
        public int Evaluation = 0;

        // Everything else you want your Engine to report

        public bool IsValid => Depth >= 0;
    }
}
