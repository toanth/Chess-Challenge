using System;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    Board b;
    // piece values from stockfish's middle game evaluation function. Our bot won't make it to even endgames
    private int[] pieceValues = { 0, 126, 781, 825, 1276, 2538, 0 };


    public Move Think(Board board, Timer timer)
    {
        b = board;

        var moves = board.GetLegalMoves();
        var bestMove = moves[0];
        var bestScore = -1_000_000;
        //for (int depth = 1; depth < 4; ++depth) // TODO: Actually use iterative deepening for something
        //{
            var depth = 3;
            foreach (var move in moves)
            {
                b.MakeMove(move);
                var result = -negamax(depth, -1_000_000, -bestScore);
                b.UndoMove(move);
                if (result > bestScore)
                {
                    bestMove = move;
                    bestScore = result;
                }
                if (timer.MillisecondsElapsedThisTurn * 8 > timer.MillisecondsRemaining) return bestMove;
            }   
        //}
        return bestMove;
    }

    // return the score from the point of view of the player who can move now,
    // ie a high value means that the current playerlikes their position
    int negamax(int depth, int alpha, int beta)
    {

        if (b.IsInCheckmate())
            return -1_000_000;
        else if (b.IsDraw())
            return 0;
        else if (depth <= 0)
            return eval();
        foreach (var move in b.GetLegalMoves())
        {
            b.MakeMove(move);
            alpha = Math.Max(alpha, -negamax(depth - 1, -beta, -alpha));
            b.UndoMove(move);
            if (alpha >= beta) return alpha;
        }
        return alpha;
    }

    int eval()
    {
        return evalPlayer(b.IsWhiteToMove) - evalPlayer(!b.IsWhiteToMove);
    }


    int evalPlayer(bool color, bool debug = false)
    {
        int material = b.GetAllPieceLists().Select(pieceList =>
                pieceList.Count * pieceValues[(int)pieceList.TypeOfPieceInList] * (pieceList.IsWhitePieceList == color ? 1 : 0)).Sum();

        int position = Math.Abs(b.GetKingSquare(color).Rank - 4) * (material - 2000) / 100 // total material is 9310, so go to the center in the endgame
            + b.GetPieceList(PieceType.Knight, color).Select(
                knight => BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(knight.Square))).Sum() // how well are knights placed?
            + b.GetPieceList(PieceType.Pawn, color).Select(pawn => Math.Abs((color ? 7 : 0) - pawn.Square.Rank)).Sum() // advancing pawns is good
            // controlling the center is good, as is having pieces slightly forward
            + BitboardHelper.GetNumberOfSetBits(color ? b.WhitePiecesBitboard & 0x003c_3c3c_3c3c_0000 : b.BlackPiecesBitboard & 0x0000_3c3c_3c3c_3c00);

        for (int slidingPiece = 3; slidingPiece <= 5; ++slidingPiece)
        {
            position += b.GetPieceList((PieceType)slidingPiece, color).Select( piece => // how well are sliding pieces placed?
                BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks((PieceType)slidingPiece, piece.Square, b))).Sum();
        }
        if (debug)
        {
            Console.WriteLine("material: " + material.ToString() + ", position: " + position);
        }

        // choosing custom factors (including for the different summmands of `position`) may improve this evaluation, but this already seems relatively decent
        return material + position;
    }

}