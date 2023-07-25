using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ChessChallenge.API;
using static System.Formats.Asn1.AsnWriter;

public class MyBot : IChessBot
{
    Board b;
    // piece values from stockfish's middle game evaluation function. Our bot won't make it to even endgames
    private int[] pieceValues = { 0, 126, 781, 825, 1276, 2538, 0 };
    // maps a zobrist hash of a position to its score, the search depth at which the score was evaluated,
    // the score type (lower bound, exact, upper bound), and the best move
    record struct TTEntry // The size of a TTEntry should be 8 + 4 + 2 + 1 + 1 + no padding = 16 bytes
    (
        ulong key, // if we need more space, we could also just store the highest 32 bits since the lowest 23 bits are given by the index, leaving 9 bits unused
        Move bestMove, // this may not actually be the best move, but the first move wich was good enough to cause a beta cut
        short score,
        byte depth,
        sbyte type // -1: upper bound (ie real score may be worse), 0: exact: 1: lower bound
    );
    //Dictionary<ulong, (ulong, short, byte, sbyte, Move)> transpositionTable = new(); // TODO: Use this uple? Should save some tokens
    // max heap usage is 256mb, so use 2^23 entries, which should consume 134mb for sizeof(TTEntry) == 16
    TTEntry[] transpositionTable = new TTEntry[8_388_608];


    public Move Think(Board board, Timer timer)
    {
        b = board;

        var moves = board.GetLegalMoves();
        var bestMove = moves[0];
        for (int depth = 1; depth < 4; ++depth) // TODO: Actually use iterative deepening for something; start with 0?
        {
            var bestScore = -32_767;
            foreach (var move in moves)
            {
                b.MakeMove(move);
                var result = -negamax(depth, -32_767, -bestScore);
                b.UndoMove(move);
                if (result > bestScore)
                {
                    bestMove = move;
                    bestScore = result;
                }
                if (timer.MillisecondsElapsedThisTurn * 8 > timer.MillisecondsRemaining) return bestMove;
            }
        }
        //Console.WriteLine("TT size: " + transpositionTable.Count);
        return bestMove;
    }

    // return the score from the point of view of the player who can move now,
    // ie a high value means that the current playerlikes their position
    int negamax(int depth, int alpha, int beta)
    {

        if (b.IsInCheckmate())
            return -32_767;
        if (b.IsDraw())
            return 0;
        if (depth <= 0)
            /*if (depth > -3)
            {
                var captures = b.GetLegalMoves(true);
                if (captures.Length > 0) score = Math.Max(alpha, -search_score(captures, depth - 1, -beta, -alpha));
                else score = eval();
            }
            else*/
            return eval();
        var legalMoves = b.GetLegalMoves();
        var lookupVal = transpositionTable[b.ZobristKey & 8_388_607];
        if (lookupVal.key == b.ZobristKey)
            if (lookupVal.depth >= depth
                && (lookupVal.type == 0 || lookupVal.type == -1 && lookupVal.score <= alpha || lookupVal.type == 1 && lookupVal.score >= beta))
                return lookupVal.score;
            else // search the most promising move first, which causes great alpha beta bounds (duplicating this move shouldn't really matter)
                legalMoves.Prepend(lookupVal.bestMove); // maybe an O(n) operation? The list should be relatively short, though
        var bestMove = legalMoves[0];
        var lowestAlpha = alpha;
        Debug.Assert(b.GetLegalMoves().Contains(bestMove)); // TODO: This may be false when a zobrist hash collision occurs
        foreach (var move in legalMoves)
        {
            b.MakeMove(move);
            var score = -negamax(depth - 1, -beta, -alpha);
            b.UndoMove(move);
            if (score > alpha)
            {
                alpha = score;
                bestMove = move;
                if (alpha >= beta) break;
            }
        }
        // always overwrite on hash table collisions (pure hash collisions should be pretty rare, but hash table collision frequent once the table is full)
        // this removes old entries that we don't care about any more at the cost of potentially throwing out useful high-depth results in favor of much
        // more frequent low-depth results
        transpositionTable[b.ZobristKey & 8_388_607]
            = new(b.ZobristKey, bestMove, (short)alpha, (byte)depth, (sbyte)(alpha <= lowestAlpha? -1 : alpha >= beta ? 1 : 0));
        return alpha;
    }

    int eval()
    {
        return evalPlayer(b.IsWhiteToMove) - evalPlayer(!b.IsWhiteToMove);
    }


    int evalPlayer(bool color)
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
            position += b.GetPieceList((PieceType)slidingPiece, color).Select(piece => // how well are sliding pieces placed?
                BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks((PieceType)slidingPiece, piece.Square, b))).Sum();
        }

        // choosing custom factors (including for the different summmands of `position`) may improve this evaluation, but this already seems relatively decent
        return material + position;
    }

}