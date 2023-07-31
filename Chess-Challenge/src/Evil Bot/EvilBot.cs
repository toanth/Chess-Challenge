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

        // piece values from PeSTO, see https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
        private int[] mgPieceValues = { 0, 82, 337, 365, 477, 1025, 0 };
        private int[] egPieceValues = { 0, 94, 281, 297, 512, 936, 0 };

        int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };

        // Each value encodes two seeds for the C# default PRNG, causing it to approximate 6 PeSTO values per seed
        ulong[] compressedPesto = { 6520592218297469586, 8482085021998488797, 313390007612757208, 2252732680713342633, 2925824399342927830, 7488136143760056370, 3262156790743221366, 8508678682509519297, 8876465218372161471, 677335457148353258, 3741400828778734912, 8975413477562298583, 2904889299511345059, 6087228928159392027, 4688959786833574720, 4716818757098577016, 7023457546842568703, 179940199461641160, 4270698656518574082, 5977270381053457415, 4081790345140221927, 6279986261450464933, 23609238681544594, 4909576089970585300, 8086413049099477623, 3303256491344861777, 8017942003509933625, 3045752169384206299, 8671206576398430491, 5346553071976938091, 3825569484231864525, 5013874612154634018, 7951213101334170686, 7920266985982688926, 4822585101256149179, 8564088513960288943, 2093659733650236066, 7220554320958609243, 8859510234402964837, 2176235602340373179, 6397670207307705640, 4415160624754945640, 3480441179429774139, 6864307048309467343, 1554303478677175384, 5629281188478270691, 8655223854542940312, 2130982693209818021, 6427222714187782579, 1162246339267495032, 8284871759444934889, 7257219422479456951, 5088534659631353214, 126110209024226324, 3915189590944783574, 9171622771817239526, 9147550055270922755, 4629007076750182041, 6072550072943827928, 1886390297413561104, 1521354504315415771, 5130596408850789355, 8842271880519837663, 8801233451296143240 };
        // the aray of pesto values
        int[] pesto = new int[768];


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


        public EvilBot()
        {

            //int[] keys = { 818678418, 1518193683, 853646557, 1974889315, 803886296, 72966797, 131403433, 524505200, 503414742, 681221578, 513073202, 1743467558, 1239467126, 759530065, 1256925633, 1981081134, 707710911, 2066713110, 1964686058, 157704450, 565536064, 871112763, 173715671, 2089751297, 1496645539, 676347245, 1768694043, 1417293429, 1011776320, 1091733525, 918723704, 1098219947, 1318483967, 1635276141, 1975081928, 41895592, 210178050, 994349517, 489141255, 1391691710, 1370732519, 950365873, 1679204005, 1462173243, 1023928210, 5496954, 410169044, 1143099761, 313025143, 1882764755, 144513617, 769099335, 1961145913, 1866822597, 33906651, 709144438, 1883402523, 2018922608, 334854763, 1244841393, 1530327245, 890709805, 634621730, 1167383653, 913607742, 1851286064, 1103134366, 1844080860, 863422651, 1122845593, 60498607, 1993982241, 1623384738, 487468143, 371611483, 1681166310, 1434895717, 2062765470, 151937723, 506694336, 1085074728, 1489573672, 416247400, 1027984690, 26059579, 810353360, 1269967055, 1598221028, 1605696600, 361889479, 1812544739, 1310669162, 92596376, 2015201341, 323404709, 496158072, 1937512883, 1496454401, 1099531384, 270606563, 1466007785, 1928972024, 1173625527, 1689703069, 244780414, 1184766800, 594572308, 29362321, 2089950422, 911576112, 1343017958, 2135434833, 965163523, 2129829967, 678872729, 1077774697, 446094296, 1413875742, 554969872, 439209467, 1999187163, 354217948, 863122411, 1194560064, 1305438175, 2058751853, 1774892936, 2049196849 };

            //List<ulong> res;
            //for (int i = 0; i < keys.Length; i += 2)
            //{
            //    ulong key = (((ulong)keys[i + 1]) << 32) + (ulong)keys[i];
            //    Console.Write(key + ", ");
            //}
            //Console.WriteLine("========");

            //Random test = new((int)(compressedPesto[(1 / 2)] >> (1 % 2 * 32)));
            //for (int i = 0; i < 6; ++i)
            //{
            //    Console.WriteLine(test.Next(-167, 187));
            //}
            // Uncompress the rng-compressed pesto values. Big thanks to https://github.com/Selenaut for doing all the hard work.
            for (int keyIdx = 0; keyIdx < 128; ++keyIdx)
            {
                Random rng = new((int)(compressedPesto[keyIdx / 2] >> (keyIdx % 2 * 32)));
                for (int i = 0; i < 6; i++)
                { // TODO: Concatenate seeds differently to make this formula use fewer tokens
                  //int val = rng.Next(-167, 187);
                  //int idx = keyIdx / 2 + keyIdx % 2 * 384 + 64 * i;
                  //pesto[idx] = val;
                  //Console.WriteLine(val + ", " + idx);
                    pesto[keyIdx / 2 + keyIdx % 2 * 384 + 64 * i] = rng.Next(-167, 187);
                }
            }
            //for (int i = 0; i < 12; ++i) {
            //    for (int e = 0; e < 64; e++)
            //    {
            //        Console.Write(pesto[e + 64 * i] + " ");
            //    }
            //    Console.WriteLine();
            //}
        }

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
                // TODO: Uncomment            return (timer.MillisecondsRemaining - timer.OpponentMillisecondsRemaining) * (ply % 2 * 200 - 100) / timer.GameStartTimeMilliseconds;
                return 0;

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
                // TODO: At some point in the future, test against this to see if the token saving potential is worth it
                //if (staticEval + pieceValues[(int)localBestMove.CapturePieceType] + 500 < alpha) return alpha;
                // The following is the "correct" way of doing it, but may not be stronger now (retest,also tune parameters)
                if (bestScore + mgPieceValues[legalMoves.Select(move => (int)move.CapturePieceType).Max()] + 300 < alpha) return alpha;
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


        // for the time being, this is very closely based on JW's example bot (ie tier 2 bot)
        int eval()
        {
            int mg = 0, eg = 0, phase = 0;

            foreach (bool stm in new[] { true, false })
            {
                for (var p = PieceType.Pawn; p <= PieceType.King; p++)
                {
                    int piece = (int)p, ind;
                    ulong mask = b.GetPieceBitboard(p, stm);
                    while (mask != 0)
                    {
                        phase += piecePhase[piece];
                        ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
                        mg += pesto[ind] + mgPieceValues[piece];
                        eg += pesto[ind + 64] + egPieceValues[piece];
                    }
                }

                mg = -mg;
                eg = -eg;
            }

            return (mg * phase + eg * (24 - phase)) / 24 * (b.IsWhiteToMove ? 1 : -1);
        }

        // JW's example bot, aka tier 2 bot
        //Move bestmoveRoot = Move.NullMove;

        //// https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
        //int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };
        //int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
        //ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };

        //// https://www.chessprogramming.org/Transposition_Table
        //struct TTEntry
        //{
        //    public ulong key;
        //    public Move move;
        //    public int depth, score, bound;
        //    public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound)
        //    {
        //        key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
        //    }
        //}

        //const int entries = (1 << 20);
        //TTEntry[] tt = new TTEntry[entries];

        //public int getPstVal(int psq)
        //{
        //    return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
        //}

        //public int Evaluate(Board board)
        //{
        //    int mg = 0, eg = 0, phase = 0;

        //    foreach (bool stm in new[] { true, false })
        //    {
        //        for (var p = PieceType.Pawn; p <= PieceType.King; p++)
        //        {
        //            int piece = (int)p, ind;
        //            ulong mask = board.GetPieceBitboard(p, stm);
        //            while (mask != 0)
        //            {
        //                phase += piecePhase[piece];
        //                ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
        //                mg += getPstVal(ind) + pieceVal[piece];
        //                eg += getPstVal(ind + 64) + pieceVal[piece];
        //            }
        //        }

        //        mg = -mg;
        //        eg = -eg;
        //    }

        //    return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
        //}

        //// https://www.chessprogramming.org/Negamax
        //// https://www.chessprogramming.org/Quiescence_Search
        //public int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply)
        //{
        //    ulong key = board.ZobristKey;
        //    bool qsearch = depth <= 0;
        //    bool notRoot = ply > 0;
        //    int best = -30000;

        //    // Check for repetition (this is much more important than material and 50 move rule draws)
        //    if (notRoot && board.IsRepeatedPosition())
        //        return 0;

        //    TTEntry entry = tt[key % entries];

        //    // TT cutoffs
        //    if (notRoot && entry.key == key && entry.depth >= depth && (
        //        entry.bound == 3 // exact score
        //            || entry.bound == 2 && entry.score >= beta // lower bound, fail high
        //            || entry.bound == 1 && entry.score <= alpha // upper bound, fail low
        //    )) return entry.score;

        //    int eval = Evaluate(board);

        //    // Quiescence search is in the same function as negamax to save tokens
        //    if (qsearch)
        //    {
        //        best = eval;
        //        if (best >= beta) return best;
        //        alpha = Math.Max(alpha, best);
        //    }

        //    // Generate moves, only captures in qsearch
        //    Move[] moves = board.GetLegalMoves(qsearch);
        //    int[] scores = new int[moves.Length];

        //    // Score moves
        //    for (int i = 0; i < moves.Length; i++)
        //    {
        //        Move move = moves[i];
        //        // TT move
        //        if (move == entry.move) scores[i] = 1000000;
        //        // https://www.chessprogramming.org/MVV-LVA
        //        else if (move.IsCapture) scores[i] = 100 * (int)move.CapturePieceType - (int)move.MovePieceType;
        //    }

        //    Move bestMove = Move.NullMove;
        //    int origAlpha = alpha;

        //    // Search moves
        //    for (int i = 0; i < moves.Length; i++)
        //    {
        //        if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 30000;

        //        // Incrementally sort moves
        //        for (int j = i + 1; j < moves.Length; j++)
        //        {
        //            if (scores[j] > scores[i])
        //                (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
        //        }

        //        Move move = moves[i];
        //        board.MakeMove(move);
        //        int score = -Search(board, timer, -beta, -alpha, depth - 1, ply + 1);
        //        board.UndoMove(move);

        //        // New best move
        //        if (score > best)
        //        {
        //            best = score;
        //            bestMove = move;
        //            if (ply == 0) bestmoveRoot = move;

        //            // Improve alpha
        //            alpha = Math.Max(alpha, score);

        //            // Fail-high
        //            if (alpha >= beta) break;

        //        }
        //    }

        //    // (Check/Stale)mate
        //    if (!qsearch && moves.Length == 0) return board.IsInCheck() ? -30000 + ply : 0;

        //    // Did we fail high/low or get an exact score?
        //    int bound = best >= beta ? 2 : best > origAlpha ? 3 : 1;

        //    // Push to TT
        //    tt[key % entries] = new TTEntry(key, bestMove, depth, best, bound);

        //    return best;
        //}

        //public Move Think(Board board, Timer timer)
        //{
        //    // https://www.chessprogramming.org/Iterative_Deepening
        //    for (int depth = 1; depth <= 50; depth++)
        //    {
        //        int score = Search(board, timer, -30000, 30000, depth, 0);

        //        // Out of time
        //        if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
        //            break;
        //    }
        //    return bestmoveRoot;
        //}
    }
}