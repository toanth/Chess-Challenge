using System;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;
using Microsoft.CodeAnalysis.Text;

public class MyBot : IChessBot
{
    Board b;
    
    public Move Think(Board board, Timer timer)
    {
        b = board;

        var moves = board.GetLegalMoves();
        var bestMove = moves[0];
        var bestScore = float.NegativeInfinity;
        foreach (var move in moves)
        {
            var result = -deepen(move, 3, float.NegativeInfinity, float.PositiveInfinity);
            if (result > bestScore)
            {
                bestMove = move;
                bestScore = result;
                Console.WriteLine("move: " + bestMove.ToString());
            }
        }
        
        Console.WriteLine("Score: " + bestScore);
        return bestMove;
    }

    // return the score from the point of view of the current player, i.e. a high value
    // means that the player who can choose between moves thinks they are winning
    float search_score(Move[] moves, int depth, float alpha, float beta)
    {
        alpha = float.NegativeInfinity;
        foreach (var move in moves)
        {
            var result = -deepen(move, depth, alpha, beta);
            //if (result >= beta) return beta;
            alpha = Math.Max(alpha, result);
        }

        return alpha;
    }

    // return the score from the point of view of the player who can move after `move` has been made,
    // ie a high value means that `move` was probably a bad move because the opponent likes their position
    float deepen(Move move, int depth, float alpha, float beta)
    {
        var score = float.NegativeInfinity;
        b.MakeMove(move);

        if (b.GetLegalMoves().Length == 0) {
            if (!b.IsInCheck()) score = 0;
        } else if (depth <= 0) {
            /*if (depth > -3)
            {
                var captures = b.GetLegalMoves(true);
                if (captures.Length > 0) score = Math.Max(alpha, -search_score(captures, depth - 1, -beta, -alpha));
                else score = eval();
                
            }
            else*/ score = eval();
        } else {
            //if (b.GetLegalMoves().Length > 0) score = Math.Max(alpha, -search_score(b.GetLegalMoves(), depth - 1, -beta, -alpha));
            score = search_score(b.GetLegalMoves(), depth - 1, -beta, -alpha);
        }
        b.UndoMove(move);
        return score;
    }

    int eval() {
        return b.GetAllPieceLists().Select(pieceList =>
            pieceList.Count * (int)pieceList.TypeOfPieceInList * (pieceList.IsWhitePieceList == b.IsWhiteToMove ? 1 : -1)).Sum();
    }
}