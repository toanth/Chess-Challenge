using System;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;
using Microsoft.CodeAnalysis.Text;

public class MyBot : IChessBot
{
    Board b;
    private bool isWhite;
    
    public Move Think(Board board, Timer timer)
    {
        b = board;
        isWhite = b.IsWhiteToMove;

        var moves = board.GetLegalMoves();
        var bestMove = moves[0];
        var bestScore = float.NegativeInfinity;
        foreach (var move in moves)
        {
            var result = deepen(move, 3, float.NegativeInfinity, float.PositiveInfinity);
            if (result > bestScore) bestMove = move;
        }
        
        Console.WriteLine("Score: " + bestScore);
        return bestMove;
    }

    float search_score(Move[] moves, int depth, float alpha, float beta)
    {
        foreach (var move in moves)
        {
            var result = deepen(move, depth, alpha, beta);
            if (result >= beta) return beta;
            alpha = Math.Max(alpha, result);
        }

        return alpha;
    }

    float deepen(Move move, int depth, float alpha, float beta)
    {
        var score = float.NegativeInfinity;
        b.MakeMove(move);
        if (depth <= 0)
        {
            if (depth > -3)
            {
                var captures = b.GetLegalMoves(true);
                if (captures.Length > 0) score = Math.Max(alpha, -search_score(captures, depth - 1, -beta, -alpha));
                else score = eval();
                
            }
            else score = eval();
        }
        else
        {
            if (b.GetLegalMoves().Length > 0) score = Math.Max(alpha, -search_score(b.GetLegalMoves(), depth - 1, -beta, -alpha));
            else if (!b.IsInCheck()) score = 0;
        }
        b.UndoMove(move);
        return score;
    }

    int eval()
    {
        return b.GetAllPieceLists().Select(pieceList =>
            pieceList.Count * (int)pieceList.TypeOfPieceInList * (pieceList.IsWhitePieceList ? 1 : -1)).Sum() * (b.IsWhiteToMove ? -1 : 1);
    }
}