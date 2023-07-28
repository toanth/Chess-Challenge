using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        Board b;
        // piece values from stockfish's middle game evaluation function. TODO: Add support for endgames
        private int[] pieceValues = { 0, 126, 781, 825, 1276, 2538, 0 };
        // maps a zobrist hash of a position to its score, the search depth at which the score was evaluated,
        // the score type (lower bound, exact, upper bound), and the best move
        record struct TTEntry // The size of a TTEntry should be 8 + 4 + 2 + 1 + 1 + no padding = 16 bytes
        (
            ulong key, // if we need more space, we could also just store the highest 32 bits since the lowest 23 bits are given by the index, leaving 9 bits unused
                       // we could also store only the move index, using 1 byte instead of 4, but needing more tokens later on.
                       // May be worth it if we also merge depth and type and use a 32bit key to get sizeof(TTEntry) down to 8
            Move bestMove, // this may not actually be the best move, but the first move wich was good enough to cause a beta cut
            short score,
            byte depth,
            sbyte type // -1: upper bound (ie real score may be worse), 0: exact: 1: lower bound
        );
        // TODO: Use a tuple instead of a struct? Should save some tokens
        // max heap usage is 256mb, so use 2^23 entries, which should consume 134mb for sizeof(TTEntry) == 16
        private TTEntry[] transpositionTable = new TTEntry[8_388_608];

        // For each depth, store 2 killer moves: A killer move is a non-capturing move that caused a beta cutoff
        // (ie early return due to score >= beta)  in a previous node. This is then used for move ordering
        private Move[] killerMoves = new Move[200]; // we're not searching more than 100 moves ahead, even with quiescent search

        // Each recursive call to negamax can (but doesn't have to) set this value; the toplevel call sets it.
        // returning (Move, int) from negamax() would be prettier but use more tokens
        private Move bestRootMove;

        private Timer timer;


        public Move Think(Board board, Timer theTimer)
        {
            // Ideas to try out: Better positional eval (based on piece square tables), removing both b.IsDraw() and b.IsInCheckmate() from the leaf code path to avoid
            // calling GetLegalMoves(), updating a materialDif variable instead of recalculating that from scratch in every eval() call,
            // (relative) history buffer, null move pruning, reverse futility pruning
            // Also, apparently LINQ is really slow for no good reason, so if we can spare the tokens we may want to use a manual for loop :(
            b = board;
            timer = theTimer;

            var moves = board.GetLegalMoves();
            // iterative deepening using the tranposition table for move ordering
            for (int depth = 1; depth < 6; ++depth) // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
            {
                // TODO: PVS, ie reuse score estimates to set alpha and beta?
                negamax(depth, -32_767, 32_767, 0);
                //Console.WriteLine("Score: " + negamax(depth, -32_767, 32_767, true));
            }
            //Console.WriteLine();
            return bestRootMove;
        }

        // return the score from the point of view of the player who can move now,
        // ie a high value means that the current playerlikes their position
        // also sets bestMove if this node is expanded (ie if the function calls itself recursively)
        int negamax(int remainingDepth, int alpha, int beta, int ply)
        {
            if (b.IsInCheckmate()) // TODO: Avoid (indirectly) calling GetLegalMoves in leafs, which is very slow apparently
                return -32_700 - remainingDepth; // being checkmated later (eval depth closer to 0) is better (as is checkmating earlier)
            if (b.IsDraw())
                return 0; // TODO: Use timer.OpponentMillisecondsRemaining()?

            bool isRoot = ply == 0;
            int killerIdx = ply * 2;
            bool quiescent = remainingDepth <= 0;
            var legalMoves = b.GetLegalMoves(quiescent).OrderByDescending(move =>
                // order promotions and captures first, according to how much more valuable the captured piece is compared to
                // the capturing (aka MVV-LVA), then killer moves, then normal moves
                move.IsPromotion ? (int)move.PromotionPieceType : move.IsCapture ? (int)move.CapturePieceType - (int)move.MovePieceType :
                    move == killerMoves[killerIdx] || move == killerMoves[killerIdx + 1] ? -10 : -100);
            if (legalMoves.Count() == 0) return eval(); // can only happen in quiescent search at the moment
            Move localBestMove = legalMoves.First();

            if (quiescent)
            {
                int staticEval = eval();
                if (staticEval >= beta) return beta; // TODO: Does it make a difference if we return staticEval here, ie fail soft?
                                                     // delta pruning, a version of futility pruning: If the current position is hopeless, abort
                                                     // technically, we should also check capturing promotions, but they are rare so we don't care
                if (staticEval + pieceValues[(int)localBestMove.CapturePieceType] + 500 < alpha) return alpha;
                // The following is the "correct" way of doing it, but doesn't seem to be stronger in practice yet uses more tokens
                // TODO: At some point in the future, test again if this gives better results (maybe it's not better now due to a problem elsewhere?)
                //if (staticEval + pieceValues[legalMoves.Select(move => (int)move.CapturePieceType).Max()] + 500 < alpha) return alpha;
                // The safety margin of 500 means we will consider trading the exchange, but there may be better values
                alpha = Math.Max(alpha, staticEval);

            }
            else
            {
                var lookupVal = transpositionTable[b.ZobristKey & 8_388_607];
                // reorder moves: First, we try the entry from the transposition table, then captures, then the rest
                if (lookupVal.key == b.ZobristKey)
                    if (lookupVal.depth >= remainingDepth && !isRoot // test for isRoot to make sure bestRootMove gets set
                        && (lookupVal.type == 0 || lookupVal.type < 0 && lookupVal.score <= alpha || lookupVal.type > 0 && lookupVal.score >= beta))
                        return lookupVal.score;
                    else // search the most promising move (as determined by previous searches) first, which creates great alpha beta bounds
                    {
                        localBestMove = lookupVal.bestMove;
                        legalMoves = legalMoves.OrderByDescending(move => move == localBestMove); // stable sorting, also works in case of a zobrist hash collision
                    }
            }

            int lowestAlpha = alpha;
            foreach (var move in legalMoves)
            {
                b.MakeMove(move);
                int score = -negamax(remainingDepth - 1, -beta, -alpha, ply + 1);
                b.UndoMove(move);
                if (score > alpha)
                {
                    alpha = score;
                    localBestMove = move;
                    if (alpha >= beta)
                    {
                        if (!move.IsCapture)
                        {
                            killerMoves[killerIdx + 1] = killerMoves[killerIdx];
                            killerMoves[killerIdx] = move;
                        }
                        break;
                    }
                }
                // testing this only in the Think function introduces too much variance into the time needed to calculate a move
                // TODO: Use timer.GameStartTimeMilliseconds() / 64 rather than hard-coded 750 ms maximum time
                if (isRoot && timer.MillisecondsElapsedThisTurn > Math.Max(750, timer.MillisecondsRemaining / 32)) break;
            }
            if (!quiescent)
                // always overwrite on hash table collisions (pure hash collisions should be pretty rare, but hash table collision frequent once the table is full)
                // this removes old entries that we don't care about any more at the cost of potentially throwing out useful high-depth results in favor of much
                // more frequent low-depth results
                transpositionTable[b.ZobristKey & 8_388_607]
                    = new(b.ZobristKey, localBestMove, (short)alpha, (byte)remainingDepth, (sbyte)(alpha <= lowestAlpha ? -1 : alpha >= beta ? 1 : 0));

            if (isRoot) bestRootMove = localBestMove;
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
}