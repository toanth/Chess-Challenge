using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

/// <summary>
/// An alpha beta negamax bot. Current features:
/// - hand crafted evaluation function (to be improved!)
/// - transposition table
/// - iterative deepening
/// - move ordering (TT and MVV/LVA)
/// - quiescent search
/// - delta pruning
/// - killer heuristic
/// - check extension
/// - "time-based contempt"
/// </summary>
public class MyBot : IChessBot
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
    private Move[] killerMoves = new Move[256]; // we're not searching more than 128 moves ahead, even with quiescent search

    // set by the toplevel call to negamax()
    // returning (Move, int) from negamax() would be prettier but use more tokens
    private Move bestRootMove;

    private Timer timer;

#if DEBUG
    int allNodeCtr;
    int nonQuiescentNodeCtr;
    int betaCutoffCtr;
    // node where remainingDepth is at least 2, so move ordering actually matters
    // (it also matters for quiescent nodes but it's difficult to count non-leaf quiescent nodes and they don't use the TT, would skew results)
    int parentOfInnerNodeCtr;
    int parentOfInnerNodeBetaCutoffCtr;
    int numTTEntries;
    int numTTCollisions;
    int numTranspositions;
    int numTTWrites;

    void printPv(int depth)
    {
        if (depth <= 0) return;
        var ttEntry = transpositionTable[b.ZobristKey & 8_388_607];
        if (ttEntry.key != b.ZobristKey)
        {
            Console.WriteLine("Position not in TT!");
            return;
        }
        var move = ttEntry.bestMove;
        if (!b.GetLegalMoves().Contains(move))
        {
            Console.WriteLine("Zobrist hash collision detected!");
            return;
        }
        Console.WriteLine(move.ToString() + ", score " + ttEntry.score + ", type " + ttEntry.type + ", depth " + ttEntry.depth);
        b.MakeMove(move);
        printPv(depth - 1);
        b.UndoMove(move);
    }
#endif


    bool stopThinking() // TODO: Can we save tokens by using properties instead of methods?
    {
        // The / 32 makes the most sense when the game last for another 32 moves. Currently, poor bot performance causes unnecesarily
        // long games in winning positions but as we improve our engine we should see less time trouble overall.
        return timer.MillisecondsElapsedThisTurn > Math.Min(timer.GameStartTimeMilliseconds / 64, timer.MillisecondsRemaining / 32);
    }


    public Move Think(Board board, Timer theTimer)
    {
        // Ideas to try out: Better positional eval (based on piece square tables), removing both b.IsDraw() and b.IsInCheckmate() from the leaf code path to avoid
        // calling GetLegalMoves(), updating a materialDif variable instead of recalculating that from scratch in every eval() call,
        // null move pruning, reverse futility pruning, recapture extension (by 1 ply, no need to limit number of extensions: Extend when prev moved captured same square),
        // depth replacement strategy for the TT (maybe by adding board.PlyCount (which is the actual position's ply count) so older entries get replaced),
        // using a simple form of cuckoo hashing for the TT where we take the lowest or highest bits of the zobrist key as index, maybe combined with
        // different replacement strategies (depth vs always overwrite) for both
        // Also, apparently LINQ is really slow for no good reason, so if we can spare the tokens we may want to use a manual for loop :(
        // Also, ideally we would have something like a pipeline where we compare our bot's move against stockfish and see if we blundered to spot potential bugs
        b = board;
        timer = theTimer;

        var moves = board.GetLegalMoves();
        // iterative deepening using the tranposition table for move ordering; without the bounded depth  the depth could exceed
        // 256 in case of forced checkmates, but that would overflow the TT entry and could potentially create problems
        for (int depth = 1; depth < 50 && !stopThinking(); ++depth) // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
#if DEBUG
        { // comment out `&& !stopThinking()` to save some tokens at the cost of slightly less readable debug output
            int score = negamax(depth, -30_000, 30_000, 0);

            Console.WriteLine("Score: " + score + ", best move: " + bestRootMove.ToString());
            var ttEntry = transpositionTable[board.ZobristKey & 8_388_607];
            Console.WriteLine("Current TT entry: " + ttEntry.ToString());
            // might fail if there is a zobrist hash collision (not just a table collision!) between the current position and a non-quiescent
            // position reached during this position's eval, and the time is up. But that's very unlikely, so the assertion stays.
            Debug.Assert(ttEntry.bestMove == bestRootMove || ttEntry.key != b.ZobristKey);
            Debug.Assert(ttEntry.score != 12345); // the canary value from cancelled searches, would require +5 queens to be computed normally
        }
#else
            // TODO: PVS?
            negamax(depth, -30_000, 30_000, 0);
#endif
#if DEBUG
        Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " + betaCutoffCtr
            + ", percent cutting (higher is better): " + (100.0 * betaCutoffCtr / allNodeCtr).ToString("0.0")
            + ", percent cutting for parents of inner nodes: " + (100.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("0.0")
            + ", TT occupancy in percent: " + (100.0 * numTTEntries / transpositionTable.Length).ToString("0.0")
            + ", TT collisions: " + numTTCollisions + ", num transpositions: " + numTranspositions + ", num TT writes: " + numTTWrites);
        Console.WriteLine("PV: ");
        printPv(8);
        Console.WriteLine();
        allNodeCtr = 0;
        nonQuiescentNodeCtr = 0;
        betaCutoffCtr = 0;
        parentOfInnerNodeCtr = 0;
        parentOfInnerNodeBetaCutoffCtr = 0;
        numTTCollisions = 0;
        numTranspositions = 0;
        numTTWrites = 0;
#endif
        return bestRootMove;
    }

    // return the score from the point of view of the player who can move now,
    // ie a high value means that the current player likes their position.
    // also sets bestRootMove if ply is zero (assuming remainingDepth > 0 and at least one legal move)
    // searched depth may be larger than minRemainingDepth due to move extensions and quiescent search
    // This function can deal with fail soft values from fail high scenarios but not from fail low ones.
    int negamax(int minRemainingDepth, int alpha, int beta, int ply) // TODO: store remainingDepth and ply, maybe alpha and beta, as class members to save tokens
    {
#if DEBUG
        ++allNodeCtr;
        if (minRemainingDepth > 0) ++nonQuiescentNodeCtr;
        if (minRemainingDepth > 1) ++parentOfInnerNodeCtr;
#endif

        if (b.IsInCheckmate()) // TODO: Avoid (indirectly) calling GetLegalMoves in leafs, which is very slow apparently
            return -30_000 + ply; // being checkmated later is better (as is checkmating earlier)
        if (b.IsDraw())
            // time-based contempt(tm): if we have more time than our opponent, try to cause timeouts.
            // This really isn't the best way to calculate the contempt factor but it's relatively token efficient.
            return (timer.MillisecondsRemaining - timer.OpponentMillisecondsRemaining) * (ply % 2 * 200 - 100) / timer.GameStartTimeMilliseconds;
        //return 0;

        bool isRoot = ply == 0;
        int killerIdx = ply * 2;
        bool quiescent = minRemainingDepth <= 0;
        var legalMoves = b.GetLegalMoves(quiescent).OrderByDescending(move =>
            // order promotions and captures first (sorted by the value of the captured piece and then the captured, aka MVV/LVA),
            // then killer moves, then normal ("quiet" non-killer) moves
            move.IsPromotion ? (int)move.PromotionPieceType * 100 : move.IsCapture ? (int)move.CapturePieceType * 100 - (int)move.MovePieceType :
                move == killerMoves[killerIdx] || move == killerMoves[killerIdx + 1] ? -1000 : -1001);
        if (!legalMoves.Any()) return eval(); // can only happen in quiescent search at the moment


        int bestScore = -32_000;
        int originalAlpha = alpha;
        if (quiescent)
        {
            bestScore = eval();
            if (bestScore >= beta) return bestScore;
            // delta pruning, a version of futility pruning: If the current position is hopeless, abort
            // technically, we should also check capturing promotions, but they are rare so we don't care
            //if (staticEval + pieceValues[(int)localBestMove.CapturePieceType] + 500 < alpha) return alpha;
            // The following is the "correct" way of doing it, but doesn't seem to be stronger in practice yet uses more tokens
            // TODO: At some point in the future, test again if this gives better results (maybe it's not better now due to a problem elsewhere?)
            if (bestScore + pieceValues[legalMoves.Select(move => (int)move.CapturePieceType).Max()] + 500 < alpha) return alpha;
            // The safety margin of 500 means we will consider trading the exchange, but there may be better values
            alpha = Math.Max(alpha, bestScore);
        }
        else
        {
            var lookupVal = transpositionTable[b.ZobristKey & 8_388_607];
            // reorder moves: First, we try the entry from the transposition table, then captures, then the rest
            if (lookupVal.key == b.ZobristKey)
                // TODO: There's some token saving potential here
                if (lookupVal.depth >= minRemainingDepth && !isRoot // test for isRoot to make sure bestRootMove gets set
                    && (lookupVal.type == 0 || lookupVal.type < 0 && lookupVal.score <= alpha || lookupVal.type > 0 && lookupVal.score >= beta))
                    // TODO: Why is it necessary to use Math.Max? Valus that are lower than alpha should cause beta cutoffs in the parent just like alpha,
                    // and the function should handle the fail soft fail high behavior of the parent. Maybe it doesn't actually?
                    //return Math.Max(alpha, lookupVal.score); // TODO: Returning a value less than alpha shouldn't lead to bugs
                    // The problem might be that fail soft scores retured like this are sometimes asumed to be exact by an ancestor, in which case
                    // a wrong score is written to the ancestor's tt entry
                    return lookupVal.score;
                else // search the most promising move (as determined by previous searches) first, which creates great alpha beta bounds
                    legalMoves = legalMoves.OrderByDescending(move => move == lookupVal.bestMove); // stable sorting, also works in case of a zobrist hash collision
        }

        // fail soft: Instead of only updating alpha, maintain an additional bestScore variable. This might be useful later on
        // but also means the TT entries can have lower upper bounds, potentially leading to a few more cuts
        Move localBestMove = legalMoves.First();
        foreach (var move in legalMoves)
        {
            b.MakeMove(move);
            // check extension: extend depth by 1 (ie don't reduce by 1) for checks -- this has no effect for the quiescent search
            // However, this causes subsequent TT entries to have the same depth as their ancestors, which seems like it might lead to bugs
            int score = -negamax(minRemainingDepth - (b.IsInCheck() ? 0 : 1), -beta, -alpha, ply + 1);
            b.UndoMove(move);

            // testing this only in the Think function introduces too much variance into the time needed to calculate a move
            if (stopThinking()) return 12345; // the value won't be used, so use a canary to detect bugs

            if (score > bestScore)
            {
                bestScore = score;
                localBestMove = move;
                alpha = Math.Max(alpha, score);
                if (score >= beta)
                {
#if DEBUG
                    ++betaCutoffCtr;
                    if (minRemainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
#endif
                    if (!move.IsCapture)
                    {
                        killerMoves[killerIdx + 1] = killerMoves[killerIdx];
                        killerMoves[killerIdx] = move;
                    }
                    break;
                }
            }
        }
#if DEBUG
        if (!quiescent) // don't fold into actual !quiescent test because then we'd need {}, adding an extra token
        {
            ++numTTWrites;
            if (transpositionTable[b.ZobristKey & 8_388_607].key == 0) ++numTTEntries; // this counter doesn't get reset every move
            else if (transpositionTable[b.ZobristKey & 8_388_607].key == b.ZobristKey) ++numTranspositions;
            else ++numTTCollisions;
        }
#endif
        if (!quiescent)
            // always overwrite on hash table collisions (pure hash collisions should be pretty rare, but hash table collision frequent once the table is full)
            // this removes old entries that we don't care about any more at the cost of potentially throwing out useful high-depth results in favor of much
            // more frequent low-depth results, but doesn't use too many tokens
            // A problem of this approach is that the more nodes it searches (for example, on longer time controls), the less likely it is that useful hihg-depth
            // positions remain in the table, althoug it seems to work fine for usual 1 minute games
            transpositionTable[b.ZobristKey & 8_388_607]
                = new(b.ZobristKey, localBestMove, (short)bestScore, (byte)minRemainingDepth, (sbyte)(bestScore <= originalAlpha ? -1 : bestScore >= beta ? 1 : 0));

        if (isRoot) bestRootMove = localBestMove;
        return bestScore;
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