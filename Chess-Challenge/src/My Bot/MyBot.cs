#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ChessChallenge.API;

/// <summary>
/// An alpha beta negamax bot. Current features:
/// - compressed PeSTO! (to be improved)
/// - transposition table
/// - iterative deepening
/// - move ordering:
/// -- TT
/// -- MVV/LVA
/// -- killer heuristic
/// - quiescent search
/// - delta pruning
/// - check extension
/// Features whih don't seem to be gaining elo right now (fix & retest!):
/// - principal variation search
/// - late move reductions
/// - null move pruning
/// </summary>
public class MyBot : IChessBot
{
    Board b;

    #region tier_2
    
    // int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };

    //// JW's compression (which could probably be improved, but evens the playing field when comparing against tier 2 bot
    //ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };


    //int getPstVal(int psq)
    //{
    //    return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
    //}
    //int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };

    #endregion


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
    private Move[] killerMoves = new Move[1024]; // nmp increases ply by 20 to prevent overwriting useful data, so accomodate for that

    // set by the toplevel call to negamax()
    // returning (Move, int) from negamax() would be prettier but use more tokens
    private Move bestRootMove;

    private Timer timer;

    // they are class members so that we can access their value outside of evaluate(), although this 
    // depends on evaluate being called first so that they are set correctly
    private int mg, eg, phase;

#if PRINT_DEBUG_INFO
    long allNodeCtr;
    long nonQuiescentNodeCtr;
    long betaCutoffCtr;
    // node where remainingDepth is at least 2, so move ordering actually matters
    // (it also matters for quiescent nodes but it's difficult to count non-leaf quiescent nodes and they don't use the TT, would skew results)
    long parentOfInnerNodeCtr;
    long parentOfInnerNodeBetaCutoffCtr;
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

    #region compresto

    private static ulong[] compresto =
    {
        2531906049332683555, 1748981496244382085, 1097852895337720349, 879379754340921365, 733287618436800776,
        1676506906360749833, 957361353080644096, 2531906049332683555, 1400370699429487872, 7891921272903718197,
        12306085787436563023, 10705271422119415669, 8544333011004326513, 7968995920879187303, 7741846628066281825,
        7452158230270339349, 5357357457767159349, 2550318802336244280, 5798248685363885890, 5789790151167530830,
        6222952639246589772, 6657566409878495570, 6013263560801673558, 4407693923506736945, 8243364706457710951,
        8314078770487191394, 6306293301333023298, 3692787177354050607, 3480508800547106083, 2756844305966902810,
        18386335130924827, 3252248017965169204, 6871752429727068694, 7516062622759586586, 7737582523311005989,
        3688521973121554199, 3401675877915367465, 3981239439281566756, 3688238338080057871, 5375663681380401,
        5639385282757351424, 2601740525735067742, 3123043126030326072, 2104069582342139184, 1017836687573008400,
        2752300895699678003, 5281087483624900674, 5717642197576017202, 578721382704613384, 14100080608108000698,
        6654698745744944230, 1808489945494790184, 507499387321389333, 1973657882726156, 74881230395412501,
        578721382704613384, 10212557253393705, 3407899295075687242, 4201957831109070667, 5866904407588300370,
        5865785079031356753, 5570777287267344460, 3984647049929379641, 2535897457754910790, 219007409309353485,
        943238143453304595, 2241421631242834717, 2098155335031661592, 1303832920857255445, 870353785759930383,
        3397624511334669, 726780562173596164, 1809356472696839713, 1665231324524388639, 1229220018493528859,
        1590638277979871000, 651911504053672215, 291616928119591952, 1227524515678129678, 6763160767239691,
        4554615069702439202, 3119099418927382298, 3764532488529260823, 5720789117110010158, 4778967136330467097,
        3473748882448060443, 794625965904696341, 150601370378243850, 4129336036406339328, 6152322103641660222,
        6302355975661771604, 5576700317533364290, 4563097935526446648, 4706642459836630839, 4126790774883761967,
        2247925333337909269, 17213489408, 6352120424995714304, 982348882
    };

    private byte[] pesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();

    #endregion

    #if PRINT_DEBUG_INFO
    bool shouldStopThinking() // TODO: Can we save tokens by using properties instead of methods?
    {
        // The / 32 makes the most sense when the game last for another 32 moves. Currently, poor bot performance causes unnecesarily
        // long games in winning positions but as we improve our engine we should see less time trouble overall.
        return timer.MillisecondsElapsedThisTurn > Math.Min(timer.GameStartTimeMilliseconds / 64, timer.MillisecondsRemaining / 32);
    }
    #endif


    public Move Think(Board board, Timer theTimer)
    {
        // Ideas to try out: Better positional eval (with better compressed psqts, passed pawns bit masks from Bits.cs, king safety by (semi) open file, ...),
        // removing both b.IsDraw() and b.IsInCheckmate() from the leaf code path to avoid calling GetLegalMoves() (but first update to 1.18),
        // updating a materialDif variable instead of recalculating that from scratch in every eval() call,
        // null move pruning, reverse futility pruning, recapture extension (by 1 ply, no need to limit number of extensions: Extend when prev moved captured same square),
        // depth replacement strategy for the TT (maybe by adding board.PlyCount (which is the actual position's ply count) so older entries get replaced),
        // using a simple form of cuckoo hashing for the TT where we take the lowest or highest bits of the zobrist key as index, maybe combined with
        // different replacement strategies (depth vs always overwrite) for both
        // Also, the NPS metric is really low. Since almost all nodes are due to quiescent search (as it should), that's where the optimization potential lies.
        // Also, apparently LINQ is really slow for no good reason, so if we can spare the tokens we may want to use a manual for loop :(
        b = board;
        timer = theTimer;
        
        var moves = board.GetLegalMoves();
        // iterative deepening using the transposition table for move ordering; without the bound on depth, it could exceed
        // 256 in case of forced checkmates, but that would overflow the TT entry and could potentially create problems
#if PRINT_DEBUG_INFO
        for (int depth = 1; depth++ < 50 && !shouldStopThinking(); ) // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
        { // comment out `&& !stopThinking()` to save some tokens at the cost of slightly less readable debug output
            int score = negamax(depth, -30_000, 30_000, 0, false);

            Console.WriteLine("Score: " + score + ", best move: " + bestRootMove.ToString());
            var ttEntry = transpositionTable[board.ZobristKey & 8_388_607];
            Console.WriteLine("Current TT entry: " + ttEntry.ToString());
            // might fail if there is a zobrist hash collision (not just a table collision!) between the current position and a non-quiescent
            // position reached during this position's eval, and the time is up. But that's very unlikely, so the assertion stays.
            Debug.Assert(ttEntry.bestMove == bestRootMove || ttEntry.key != b.ZobristKey);
            Debug.Assert(ttEntry.score != 12345); // the canary value from cancelled searches, would require +5 queens to be computed normally
        }
#else
        for (int depth = 0; depth++ < 50; )
            negamax(depth, -30_000, 30_000, 0, false);
#endif

#if PRINT_DEBUG_INFO
        Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " + betaCutoffCtr
            + ", percent cutting (higher is better): " + (100.0 * betaCutoffCtr / allNodeCtr).ToString("0.0")
            + ", percent cutting for parents of inner nodes: " + (100.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("0.0")
            + ", TT occupancy in percent: " + (100.0 * numTTEntries / transpositionTable.Length).ToString("0.0")
            + ", TT collisions: " + numTTCollisions + ", num transpositions: " + numTranspositions + ", num TT writes: " + numTTWrites);
        Console.WriteLine("NPS: {0}k", (allNodeCtr / (double)timer.MillisecondsElapsedThisTurn).ToString("0.0"));
        Console.WriteLine("Time:{0} of {1} ms, remaining {2}", timer.MillisecondsElapsedThisTurn, timer.GameStartTimeMilliseconds, timer.MillisecondsRemaining);
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
    int negamax(int remainingDepth, int alpha, int beta, int ply, bool allowNMP) // TODO: store remainingDepth and ply, maybe alpha and beta, as class members to save tokens
    {
#if PRINT_DEBUG_INFO
        ++allNodeCtr;
        if (remainingDepth > 0) ++nonQuiescentNodeCtr;
        if (remainingDepth > 1) ++parentOfInnerNodeCtr;
#endif

        if (b.IsInCheckmate()) // TODO: Avoid (indirectly) calling GetLegalMoves in leafs, which is very slow apparently
            return ply - 30_000; // being checkmated later is better (as is checkmating earlier); save
        if (b.IsDraw())
            // time-based contempt(tm): if we have more time than our opponent, try to cause timeouts.
            // This really isn't the best way to calculate the contempt factor but it's relatively token efficient.
            // Disabled for now because it doesn't seem to improve elo beyond the margin of doubt at this point, maybe a better implementation could?
            // return (timer.MillisecondsRemaining - timer.OpponentMillisecondsRemaining) * (ply % 2 * 200 - 100) / timer.GameStartTimeMilliseconds;
            return 0;

        bool isRoot = ply == 0,
            inQsearch = remainingDepth <= 0,
            maybePvNode = alpha + 1 < beta,
            doPvs; // moved here from the loop over moves to save the `bool` token. 
        int bestScore = -32_000,
            originalAlpha = alpha,
            killerIdx = ply * 2,
            score, // moved here from the loop over moves to save the `int` token.
            newDepth, // moved here from the loop over moves to save the `int` token.
            nmpScore; // moved here from the nmp if statement to save the `int` token
        ulong ttIdx = b.ZobristKey & 8_388_607;
        
        var legalMoves = b.GetLegalMoves(inQsearch).OrderByDescending(move =>
            // order promotions and captures first (sorted by the value of the captured piece and then the captured, aka MVV/LVA),
            // then killer moves, then normal ("quiet" non-killer) moves
            move.IsPromotion ? (int)move.PromotionPieceType * 100 : move.IsCapture ? (int)move.CapturePieceType * 100 - (int)move.MovePieceType :
                move == killerMoves[killerIdx] || move == killerMoves[killerIdx + 1] ? -1000 : -1001);
        if (!legalMoves.Any()) return eval(); // can only happen in quiescent search at the moment

        if (inQsearch)
        {
            // updating bestScore not only saves tokens compared to using a new standPat variable, it also increases playing strength by 100 elo compared to not
            // updating bestScore. This is because the standPat score is often a more reliable estimate than forcibly taking a possible capture, so it should be
            // returned if all captures fail low.
            bestScore = eval();
            if (bestScore >= beta) return bestScore;
            // delta pruning, a version of futility pruning: If the current position is hopeless, abort
            // technically, we should also check capturing promotions, but they are rare so we don't care
            // TODO: At some point in the future, test against this to see if the token saving potential is worth it
            if (bestScore + 1150 < alpha) return alpha;
            // The following is the "correct" way of doing it, but may not be stronger now (retest,also tune parameters)
            // This doesn't work at all any more for psqt adjusted piece values
            // if (bestScore + mgPieceValues[legalMoves.Select(move => (int)move.CapturePieceType).Max()] + 300 < alpha) return bestScore; // TODO: This only has a very small effect
            alpha = Math.Max(alpha, bestScore); // TODO: If statement should use fewer tokens
        }
        else // TODO: Also do a table lookup during quiescent search? Test performance and tokens
        {
            var lookupVal = transpositionTable[ttIdx];
            // reorder moves: First, we try the entry from the transposition table, then captures, then the rest
            if (lookupVal.key == b.ZobristKey)
                // TODO: There's some token saving potential here
                // don't do TT cuts in pv nodes (which includes the root) because tt entries can sometimes be wrong
                // because chess isn't markovian (eg 3 fold repetition)
                // the Math.Abs(lookupVal.score) < 29_000 is a crude (but working) way to not do TT cutoffs for mate scores, TODO: Test if necessary (probably not)
                if (lookupVal.depth >= remainingDepth && Math.Abs(lookupVal.score) < 29_000 && !maybePvNode
                    && (lookupVal.type == 0 || lookupVal.type < 0 && lookupVal.score <= alpha || lookupVal.type > 0 && lookupVal.score >= beta))
                    return lookupVal.score;
                else // search the most promising move (as determined by previous searches) first, which creates great alpha beta bounds
                    legalMoves = legalMoves.OrderByDescending(move => move == lookupVal.bestMove); // stable sorting, also works in case of a zobrist hash collision
        }
        // nmp, TODO: tune R, actually make sure phase is correct instead of possibly from a sibling or parent (which shouldn't hurt much but is still incorrect)
        if (allowNMP && !maybePvNode && remainingDepth > 3 && phase > 0 && b.TrySkipTurn())
        {
            // if we have pvs, we can use pvs here as well (but for now it stays normal negamax)
            // we can't reuse killerIdx because C# basically captures by reference, so we need to create a new variable
            // increase ply by 20 to prevent clashes with normal search for killer moves
            nmpScore = -negamax(remainingDepth - 3, -beta, -alpha, ply + 20, false);
            b.UndoSkipTurn();
            if (nmpScore > beta) return nmpScore;
            alpha = Math.Max(alpha, nmpScore);
            bestScore = Math.Max(bestScore, nmpScore); // TODO: Optimize tokens by folding into nmpScore declaration
        }

        // fail soft: Instead of only updating alpha, maintain an additional bestScore variable. This might be useful later on
        // but also means the TT entries can have lower upper bounds, potentially leading to a few more cuts
        Move localBestMove = legalMoves.First();
        foreach (var move in legalMoves)
        {
            // check extension: extend depth by 1 (ie don't reduce by 1) for checks -- this has no effect for the quiescent search
            // However, this causes subsequent TT entries to have the same depth as their ancestors, which seems like it might lead to bugs
            newDepth = remainingDepth - (b.IsInCheck() ? 0 : 1);
            // pvs: If we already increased alpha (which should happen in the first node), do a zero window search to confirm the best move so far is
            // actually the best move in this position (which is more likely to be true the better move ordering works), zws should fail faster so use less time
            // TODO: Maybe actually check for not being the first child instead of bestScore > alpha since that doesn't work for all-nodes (ie fail-low nodes)?
            doPvs = bestScore > originalAlpha && !inQsearch; // TODO: Also do in qsearch? Probably not (cause no tt for qsearch atm) but meassure
            b.MakeMove(move);
            // lmr: If not in qsearch and not the first node (aka doPvs), reduce by one unless in a non-quiet position
            score = -negamax(newDepth - (doPvs && !move.IsCapture && !b.IsInCheck() ? 1 : 0), doPvs ? -alpha - 1 : -beta, -alpha, ply + 1, true);
            if (alpha < score && score < beta && doPvs)
                // zero window search failed, so research with full window
                score = -negamax(newDepth, -beta, -alpha, ply + 1, true);
            b.UndoMove(move);

            // testing this only in the Think function introduces too much variance into the time needed to calculate a move,
            // but (at least on Linux? T0D0: Test on windows!) getting the elapsed time is apparently (?) super expensive, but
            // adding !quiescent doesn't improve the bot (depth < 2 even makes it worse because it looses on time)
            if (timer.MillisecondsElapsedThisTurn > Math.Min(timer.GameStartTimeMilliseconds / 64, timer.MillisecondsRemaining / 32))
                return 12345; // the value won't be used, so use a canary to detect bugs

            if (score > bestScore)
            {
                bestScore = score;
                localBestMove = move;
                alpha = Math.Max(alpha, score);
                if (score >= beta)
                {
#if PRINT_DEBUG_INFO
                    ++betaCutoffCtr;
                    if (remainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
#endif
                    // TODO: This quiet move detection considers promotions to be quiet, but the mvvlva code doesn't.
                    // Use this definition also for the move ordering code?
                    if (!move.IsCapture && move != killerMoves[killerIdx])  // TODO: enabling the move != killerMoves[killerIdx] check looses like 30 elo ?!?
                    {
                        killerMoves[killerIdx + 1] = killerMoves[killerIdx];
                        killerMoves[killerIdx] = move;
                    }
                    break;
                }
            }
        }
#if PRINT_DEBUG_INFO
        if (!inQsearch) // don't fold into actual !quiescent test because then we'd need {}, adding an extra token
        {
            ++numTTWrites;
            if (transpositionTable[ttIdx].key == 0) ++numTTEntries; // this counter doesn't get reset every move
            else if (transpositionTable[ttIdx].key == b.ZobristKey) ++numTranspositions;
            else ++numTTCollisions;
        }
#endif
        if (!inQsearch)
            // always overwrite on hash table collisions (pure hash collisions should be pretty rare, but hash table collision frequent once the table is full)
            // this removes old entries that we don't care about any more at the cost of potentially throwing out useful high-depth results in favor of much
            // more frequent low-depth results, but doesn't use too many tokens
            // A problem of this approach is that the more nodes it searches (for example, on longer time controls), the less likely it is that useful high-depth
            // positions remain in the table, althoug it seems to work fine for usual 1 minute games
            transpositionTable[ttIdx]
                = new(b.ZobristKey, localBestMove, (short)bestScore, (byte)remainingDepth, (sbyte)(bestScore <= originalAlpha ? -1 : bestScore >= beta ? 1 : 0));

        if (isRoot) bestRootMove = localBestMove;
        return bestScore;
    }


    // for the time being, this is very closely based on JW's example bot (ie tier 2 bot)
    int eval()
    {
        phase = mg = eg = 0;
        foreach (bool stm in new[] { true, false })
        {
            for (var p = PieceType.None; ++p <= PieceType.King; )
            {
                int piece = (int)p - 1, ind;
                ulong mask = b.GetPieceBitboard(p, stm);
                while (mask != 0)
                {
                    phase += pesto[768 + piece];
                    ind = 64 * piece + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                    // The (47 << piece) trick doesn't really save all that much at the moment, but...
                    // ...TODO: By storing mg tables first, then eg tables, this code can be reused for mg and eg calculation, potentially saving a few tokens
                    mg += pesto[ind] + (47 << piece) + pesto[piece + 776];
                    eg += pesto[ind + 384] + (47 << piece) + pesto[piece + 782];
                }
            }

            mg = -mg;
            eg = -eg;
        }

        return (mg * phase + eg * (24 - phase)) / (b.IsWhiteToMove ? 24 : -24);
        // int res = (mg * phase + eg * (24 - phase)) / 24 * (b.IsWhiteToMove ? 1 : -1);
        // int expected = Pesto.originalPestoEval(b);
        // if (res != expected)
        // {
        //     Console.WriteLine("eval was {0}, should be {1}", res, expected);
        //     Debug.Assert(false);
        // }
        // return res;
    }
}