using ChessChallenge.API;
using System;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
     
        //                     .  P    K    B    R    Q    K
        int[] kPieceValues = { 0, 100, 300, 310, 500, 900, 10000 };
        int kMassiveNum = 99999999;

        int mDepth;
        Move mBestMove;

        public Move Think(Board board, Timer timer)
        {
            Move[] legalMoves = board.GetLegalMoves();
            mDepth = 3;

            EvaluateBoardNegaMax(board, mDepth, -kMassiveNum, kMassiveNum, board.IsWhiteToMove ? 1 : -1);

            return mBestMove;
        }

        int EvaluateBoardNegaMax(Board board, int depth, int alpha, int beta, int color)
        {
            Move[] legalMoves;

            if (board.IsDraw())
                return 0;

            if (depth == 0 || (legalMoves = board.GetLegalMoves()).Length == 0)
            {
                // EVALUATE
                int sum = 0;

                if (board.IsInCheckmate())
                    return -9999999;

                for (int i = 0; ++i < 7;)
                    sum += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * kPieceValues[i];
                // EVALUATE

                return color * sum;
            }

            // TREE SEARCH
            int recordEval = int.MinValue;
            foreach (Move move in legalMoves)
            {
                board.MakeMove(move);
                int evaluation = -EvaluateBoardNegaMax(board, depth - 1, -beta, -alpha, -color);
                board.UndoMove(move);

                if (recordEval < evaluation)
                {
                    recordEval = evaluation;
                    if (depth == mDepth)
                        mBestMove = move;
                }
                alpha = Math.Max(alpha, recordEval);
                if (alpha >= beta) break;
            }
            // TREE SEARCH

            return recordEval;
        }
        //Board b;

        //public Move Think(Board board, Timer timer)
        //{
        //    b = board;

        //    var moves = board.GetLegalMoves();
        //    var bestMove = moves[0];
        //    var bestScore = float.NegativeInfinity;
        //    foreach (var move in moves)
        //    {
        //        var result = -deepen(move, 3, float.NegativeInfinity, float.PositiveInfinity);
        //        if (result > bestScore)
        //        {
        //            bestMove = move;
        //            bestScore = result;
        //            Console.WriteLine("move: " + bestMove.ToString());
        //        }
        //    }

        //    Console.WriteLine("Score: " + bestScore);
        //    return bestMove;
        //}

        //// return the score from the point of view of the current player, i.e. a high value
        //// means that the player who can choose between `moves` thinks they are winning
        //float search_score(Move[] moves, int depth, float alpha, float beta)
        //{
        //    foreach (var move in moves)
        //    {
        //        var result = -deepen(move, depth, alpha, beta);
        //        if (result >= beta) return beta;
        //        alpha = Math.Max(alpha, result);
        //    }

        //    return alpha;
        //}

        //// return the score from the point of view of the player who can move after `move` has been made,
        //// ie a high value means that `move` was probably a bad move because the opponent likes their position
        //float deepen(Move move, int depth, float alpha, float beta)
        //{
        //    var score = float.NegativeInfinity;
        //    b.MakeMove(move);

        //    if (b.GetLegalMoves().Length == 0)
        //    {
        //        if (!b.IsInCheck()) score = 0;
        //    }
        //    else if (depth <= 0)
        //    {
        //        /*if (depth > -3)
        //        {
        //            var captures = b.GetLegalMoves(true);
        //            if (captures.Length > 0) score = Math.Max(alpha, -search_score(captures, depth - 1, -beta, -alpha));
        //            else score = eval();

        //        }
        //        else*/
        //        score = eval();
        //    }
        //    else
        //    {
        //        score = search_score(b.GetLegalMoves(), depth - 1, -beta, -alpha);
        //    }
        //    b.UndoMove(move);
        //    return score;
        //}

        //int eval()
        //{
        //    return b.GetAllPieceLists().Select(pieceList =>
        //        pieceList.Count * (int)pieceList.TypeOfPieceInList * (pieceList.IsWhitePieceList == b.IsWhiteToMove ? 1 : -1)).Sum();
        //}
    }
}