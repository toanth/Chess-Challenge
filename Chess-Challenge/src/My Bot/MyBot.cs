using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    Board b;
    int[] pieceValues = { 0, 126, 781, 825, 1276, 2538, 0 };
    Dictionary<ulong, float> scoreCache = new();

    public Move Think(Board board, Timer timer)
    {
        b = board;

        var moves = board.GetLegalMoves();
        var bestMove = moves[0];
        var bestScore = float.NegativeInfinity;
        var start = timer.MillisecondsRemaining;
        var depth = 3;
        
        // attempt at iterative deepening. No move ordering yet, and no cancelling of calculations that take too long
        while (timer.MillisecondsRemaining > start - 500)
        {
            scoreCache.Clear();
            foreach (var move in moves)
            {
                var result = -deepen(move, depth, float.NegativeInfinity, float.PositiveInfinity);
                if (result > bestScore)
                {
                    bestMove = move;
                    bestScore = result;
                }
            }
            depth++;
        }
        
        return bestMove;
    }

    // return the score from the point of view of the current player, i.e. a high value
    // means that the player who can choose between `moves` thinks they are winning
    float search_score(Move[] moves, int depth, float alpha, float beta)
    {
        foreach (var move in moves)
        {
            var result = -deepen(move, depth, alpha, beta);
            if (result >= beta) return beta;
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
        if (scoreCache.TryGetValue(b.ZobristKey, out var lookupScore))
        {
            b.UndoMove(move);
            return lookupScore;
        }

        if (b.IsInCheckmate()) {
        } else if (b.IsDraw()) {
            score = -1;
        } else if (depth <= 0) {
            /*if (depth > -3)
            {
                var captures = b.GetLegalMoves(true);
                if (captures.Length > 0) score = Math.Max(alpha, -search_score(captures, depth - 1, -beta, -alpha));
                else score = eval();
                
            }
            else*/ score = eval();
        } else {
            score = search_score(b.GetLegalMoves(), depth - 1, -beta, -alpha);
        }

        // TODO do I need to invert this for enemy moves?
        scoreCache[b.ZobristKey] = score;
        b.UndoMove(move);
        return score;
    }

    int eval() {
        return evalPlayer(b.IsWhiteToMove) - evalPlayer(!b.IsWhiteToMove);
    }

    int evalPlayer(bool color) {
        int material = b.GetAllPieceLists().Select(pieceList =>
                pieceList.Count * pieceValues[(int)pieceList.TypeOfPieceInList] * (pieceList.IsWhitePieceList == color ? 1 : 0)).Sum();
        int position = Math.Abs(b.GetKingSquare(color).Rank - 4) * (material - 2000) / 100 // total material 9310, so go to the center in the endgame
            + b.GetPieceList(PieceType.Knight, color).Select(
                knight => BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(knight.Square))).Sum() // how well are knights placed?
            + b.GetPieceList(PieceType.Pawn, color).Select(pawn => Math.Abs((color ? 7 : 0) - pawn.Square.Rank)).Sum() // advancing pawns is good
            + BitboardHelper.GetNumberOfSetBits((color ? b.WhitePiecesBitboard : b.BlackPiecesBitboard) & 0x00c3c3c3c300); // controlling the center is good
        for (int slidingPiece = 3; slidingPiece <= 5; ++slidingPiece)
        {
            position += b.GetPieceList((PieceType)slidingPiece, color).Select( // how well are sliding pieces placed?
                piece => BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks((PieceType)slidingPiece, piece.Square, b))).Sum();
        }
        // choosing custom factors (including for the different summmands of `position` may improve this evaluation, but this already seems relatively decent
        return material + position;
    }

}