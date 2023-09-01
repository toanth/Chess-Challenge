#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
#endif

using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Math;

namespace ChessChallenge.Example
{

    public class EvilBot : IChessBot
    {
        private Board board;

        private Timer timer;
        // TODO: By defining all methods inside Think, member variables become unnecessary

        public record struct TTEntry(
            ulong key,
            Move bestMove,
            short score,
            sbyte flag,
            sbyte depth
        );


        // 1 << 25 entries (without the pesto values, it would technically be possible to store exactly 1 << 26 Moves with 4 byte per Move)
        // the tt move ordering alone gained almost 400 elo
        //private Move[] ttMoves = new Move[0x200_0000];

        private TTEntry[] tt = new TTEntry[0x80_0000];

        private Move[] killers = new Move[65536];

        private int[,,] history/* = new int[2, 7, 64]*/;

        private Move bestRootMove;

#if PRINT_DEBUG_INFO
                long allNodeCtr;
                long nonQuiescentNodeCtr;
                long betaCutoffCtr;
                // node where remainingDepth is at least 2, so move ordering actually matters
                // (it also matters for quiescent nodes but it's difficult to count non-leaf quiescent nodes and they don't use the TT, would skew results)
                long parentOfInnerNodeCtr;
                long parentOfInnerNodeBetaCutoffCtr;
                long pvsTryCtr;
                long pvsRetryCtr;
                int lastDepth;
                int lastScore;

                void printPv(int remainingDepth = 15)
                {
                    var entry = tt[board.ZobristKey & 0x7f_ffff];
                    var move = entry.bestMove;
                    if (board.ZobristKey == entry.key && board.GetLegalMoves().Contains(move) && remainingDepth > 0)
                    {
                        Console.WriteLine(move);
                        board.MakeMove(move);
                        printPv(remainingDepth - 1);
                        board.UndoMove(move);
                    }
                }

                public BotInfo Info()
                {
                    return new BotInfo(lastDepth, lastScore);
                }

#endif

        #region compresto

#if NO_JOKE

            //private static ulong[] compresto =
            //{
            //        2531906049332683555, 1748981496244382085, 1097852895337720349, 879379754340921365, 733287618436800776,
            //        1676506906360749833, 957361353080644096, 2531906049332683555, 1400370699429487872, 7891921272903718197,
            //        12306085787436563023, 10705271422119415669, 8544333011004326513, 7968995920879187303, 7741846628066281825,
            //        7452158230270339349, 5357357457767159349, 2550318802336244280, 5798248685363885890, 5789790151167530830,
            //        6222952639246589772, 6657566409878495570, 6013263560801673558, 4407693923506736945, 8243364706457710951,
            //        8314078770487191394, 6306293301333023298, 3692787177354050607, 3480508800547106083, 2756844305966902810,
            //        18386335130924827, 3252248017965169204, 6871752429727068694, 7516062622759586586, 7737582523311005989,
            //        3688521973121554199, 3401675877915367465, 3981239439281566756, 3688238338080057871, 5375663681380401,
            //        5639385282757351424, 2601740525735067742, 3123043126030326072, 2104069582342139184, 1017836687573008400,
            //        2752300895699678003, 5281087483624900674, 5717642197576017202, 578721382704613384, 14100080608108000698,
            //        6654698745744944230, 1808489945494790184, 507499387321389333, 1973657882726156, 74881230395412501,
            //        578721382704613384, 10212557253393705, 3407899295075687242, 4201957831109070667, 5866904407588300370,
            //        5865785079031356753, 5570777287267344460, 3984647049929379641, 2535897457754910790, 219007409309353485,
            //        943238143453304595, 2241421631242834717, 2098155335031661592, 1303832920857255445, 870353785759930383,
            //        3397624511334669, 726780562173596164, 1809356472696839713, 1665231324524388639, 1229220018493528859,
            //        1590638277979871000, 651911504053672215, 291616928119591952, 1227524515678129678, 6763160767239691,
            //        4554615069702439202, 3119099418927382298, 3764532488529260823, 5720789117110010158, 4778967136330467097,
            //        3473748882448060443, 794625965904696341, 150601370378243850, 4129336036406339328, 6152322103641660222,
            //        6302355975661771604, 5576700317533364290, 4563097935526446648, 4706642459836630839, 4126790774883761967,
            //        2247925333337909269, 17213489408, 6352120424995714304, 982348882
            //    };

#else

        // modified pesto values to make the king lead the army (as he should)
        private static ulong[] compresto = { 2531906049332683555, 1748981496244382085, 1097852895337720349, 879379754340921365,
                733287618436800776, 1676506906360749833, 957361353080644096, 2531906049332683555, 1400370699429487872, 7891921272903718197,
                12306085787436563023, 10705271422119415669, 8544333011004326513, 7968995920879187303, 7741846628066281825, 7452158230270339349,
                5357357457767159349, 2550318802336244280, 5798248685363885890, 5789790151167530830, 6222952639246589772, 6657566409878495570,
                6013263560801673558, 4407693923506736945, 8243364706457710951, 8314078770487191394, 6306293301333023298, 3692787177354050607,
                3480508800547106083, 2756844305966902810, 18386335130924827, 3252248017965169204, 6871752429727068694, 7516062622759586586,
                7737582523311005989, 3688521973121554199, 3401675877915367465, 3981239439281566756, 3688238338080057871, 5375663681380401,
                18446744073709551615, 18446744073709551615, 18446744073709551615, 18085043209519168250, 14468034567615334600, 7234017283807667300,
                729075380852492810, 2220548423680, 578721382704613384, 14100080608108000698, 6654698745744944230, 1808489945494790184,
                507499387321389333, 1973657882726156, 74881230395412501, 578721382704613384, 10212557253393705, 3407899295075687242,
                4201957831109070667, 5866904407588300370, 5865785079031356753, 5570777287267344460, 3984647049929379641, 2535897457754910790,
                219007409309353485, 943238143453304595, 2241421631242834717, 2098155335031661592, 1303832920857255445, 870353785759930383,
                3397624511334669, 726780562173596164, 1809356472696839713, 1665231324524388639, 1229220018493528859, 1590638277979871000,
                651911504053672215, 291616928119591952, 1227524515678129678, 6763160767239691, 4554615069702439202, 3119099418927382298,
                3764532488529260823, 5720789117110010158, 4778967136330467097, 3473748882448060443, 794625965904696341, 150601370378243850,
                4129336036406339328, 6152322103641660222, 6302355975661771604, 5576700317533364290, 4563097935526446648, 4706642459836630839,
                4126790774883761967, 2247925333337909269, 17213489408, 6352191893251519744, 982348882
            };

#endif

        private byte[] pesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();

        #endregion // compresto


        bool shouldStopThinking() // TODO: Can we save tokens by using properties instead of methods?
        {
            // The / 32 makes the most sense when the game last for another 32 moves. Currently, poor bot performance causes unnecesarily
            // long games in winning positions but as we improve our engine we should see less time trouble overall.
            return timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 32;
        }


        public Move Think(Board theBoard, Timer theTimer)
        {
            board = theBoard;
            timer = theTimer;


            history = new int[2, 7, 64];

            //for (int stm = 0; stm <= 1; ++stm)
            //{
            //    for (int piece = 0; piece < 7; ++piece)
            //    {
            //        for (int square = 0; square < 64; ++square)
            //        {
            //            history[stm, piece, square] /= 8;
            //        }
            //    }
            //}
            // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
#if PRINT_DEBUG_INFO
                    for (int depth = 1; depth++ < 50 && !shouldStopThinking();)
                    {
                        int score = negamax(depth, -30_000, 30_000, 0);
                        lastDepth = depth;
                        if (score != 12345) lastScore = score;
                        Console.WriteLine("Depth {0}, score {1}, best move {2}", depth, lastScore, bestRootMove);
                    }
#else
            for (int depth = 1; depth++ < 50 && !shouldStopThinking();)
                negamax(depth, -30_000, 30_000, 0);
#endif

#if PRINT_DEBUG_INFO
                    Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " + betaCutoffCtr
                        + ", percent cutting (higher is better): " + (1.0 * betaCutoffCtr / allNodeCtr).ToString("P1")
                        + ", percent cutting for parents of inner nodes: " + (1.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("P1"));
                    Console.WriteLine("NPS: {0}k", (allNodeCtr / (double)timer.MillisecondsElapsedThisTurn).ToString("0.0"));
                    Console.WriteLine("Time:{0} of {1} ms, remaining {2}", timer.MillisecondsElapsedThisTurn, timer.GameStartTimeMilliseconds, timer.MillisecondsRemaining);
                    Console.WriteLine("PV: ");
                    printPv();
                    Console.WriteLine();
                    allNodeCtr = 0;
                    nonQuiescentNodeCtr = 0;
                    betaCutoffCtr = 0;
                    parentOfInnerNodeCtr = 0;
                    parentOfInnerNodeBetaCutoffCtr = 0;
#endif

            return bestRootMove;
        }


        int negamax(int remainingDepth, int alpha, int beta, int ply, bool allowNmp = true)
        {
#if PRINT_DEBUG_INFO
                    ++allNodeCtr;
                    if (remainingDepth > 0) ++nonQuiescentNodeCtr;
                    if (remainingDepth > 1) ++parentOfInnerNodeCtr;
#endif

            // Using stackalloc doesn't gain elo
            bool /*isRoot = ply == 0,*/
                inQsearch = remainingDepth <= 0,
                isPvNode = alpha + 1 < beta;
            var legalMoves = board.GetLegalMoves(inQsearch);
            int numMoves = legalMoves.Length,
                bestScore = -32_000,
                originalAlpha = alpha,
                standPat = eval(),
                moveIdx = 0,
                score;
            // calculating IsInCheck() before GetLegalMoves() loses very approx. 10 elo due to extra work
            bool inCheck = board.IsInCheck();

            // replacing those functions with legalMoves.Length == 0 checks (plus repetition detection, insufficient material) didn't gain elo, TODO: Retest eventually
            if (board.IsInCheckmate())
                return ply - 30_000; // being checkmated later is better (as is checkmating earlier)
            if (board.IsDraw())
                return 0;

            if (inQsearch)
            {
                bestScore = standPat;
                if (standPat >= beta) return standPat;
                if (alpha < standPat) alpha = standPat;
            }

            // Check Extensions
            if (inCheck) ++remainingDepth;

            // TODO: Use tt for stand pat score?
            ref TTEntry ttEntry = ref tt[board.ZobristKey & 0x7f_ffff];

            if (!isPvNode && ttEntry.depth >= remainingDepth && ttEntry.key == board.ZobristKey)
            {
                if (ttEntry.flag > -1) alpha = Max(alpha, ttEntry.score); // TODO: Also set bestScore?
                if (ttEntry.flag < 1) beta = Min(beta, ttEntry.score);
            }
            if (alpha >= beta) return alpha;


            // Reverse Futility Pruning
            if (!isPvNode && !inCheck && !inQsearch && remainingDepth < 5 && standPat >= beta + 64 * remainingDepth)
                return standPat;

            // Null Move Pruning. TODO: Avoid zugzwang by testing phase?
            if (!isPvNode && remainingDepth >= 4 && allowNmp && standPat >= beta && board.TrySkipTurn())
            {
                //int reduction = 3 + remainingDepth / 5;
                // changing the ply by a large number doesn't seem to gain elo, even though this should prevent overwriting killer moves
                score = -negamax(remainingDepth - 3 - remainingDepth / 5, -beta, -alpha, ply + 1, false);
                board.UndoSkipTurn();
                if (score >= beta)
                    return score;
            }

            // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending
            var scores = new int[numMoves];
            for (; moveIdx < numMoves;)
            {
                Move move = legalMoves[moveIdx];
                scores[moveIdx++] = move == ttEntry.bestMove ? -1_000_000_000 : move.IsCapture ? (int)move.MovePieceType - (int)move.CapturePieceType * 1_000_000 :
                    move == killers[2 * ply] || move == killers[2 * ply + 1] ? -100_000 :
                    -history[board.IsWhiteToMove ? 1 : 0, (int)move.MovePieceType, move.TargetSquare.Index];
            }
            Array.Sort(scores, legalMoves);

            Move localBestMove = default;
            for (moveIdx = 0; moveIdx < legalMoves.Length;)
            {
                Move move = legalMoves[moveIdx];
                int newDepth = remainingDepth - 1;
                board.MakeMove(move);
                if (moveIdx++ == 0) // pvs like this is -7 +- 20 elo after 1000 games; adding inQsearch || ... doesn't change that, nor does move == ttMove
                    score = -negamax(newDepth, -beta, -alpha, ply + 1);
                else
                {
                    // Late Move Reductions, needs further parameter tuning
                    // !isRoot seems to result in a small improvement, at least. So far, reducing pv nodes less seems to lose elo
                    int reduction = moveIdx >= (isPvNode ? 6 : 4)
                        && remainingDepth > 3
                        && !move.IsCapture
                        && !inCheck ?
                            //reduction = 3; // TODO: Once the engine is better, test with viri values: (int)(0.77 + Log(remainingDepth) * Log(i) / 2.36);
                            //reduction -= isPvNode ? 1 : 0;
                            Clamp(3 - (isPvNode ? 1 : 0), 0, remainingDepth - 2)
                            : 0;
                    score = -negamax(newDepth - reduction, -alpha - 1, -alpha, ply + 1);
                    if (alpha < score && score < beta)
                        score = -negamax(newDepth, -beta, -alpha, ply + 1);
                }

                board.UndoMove(move);

                if (shouldStopThinking())
                    return 12345; // the value won't be used, so use a canary to detect bugs

                if (score > bestScore)
                {
                    localBestMove = move;
                    alpha = Max(alpha, bestScore = score);
                    if (score >= beta)
                    {
#if PRINT_DEBUG_INFO
                                ++betaCutoffCtr;
                                if (remainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
#endif
                        // killer heuristic gives 27 +- 20 elo, using two entries gives 7.5 +- 14 elo 
                        // checking that move != killers[2 * ply] doesn't seem to gain elo at all after 4000 games
                        if (!move.IsCapture && move != killers[2 * ply])
                        {
                            killers[2 * ply + 1] = killers[2 * ply];
                            killers[2 * ply] = move;
                            // gravity didn't gain (TODO: Retest later when the engine is better), but history still gained quite a bit
                            history[board.IsWhiteToMove ? 1 : 0, (int)move.MovePieceType, move.TargetSquare.Index]
                                += remainingDepth * remainingDepth;
                        }

                        break;
                    }
                }
            }

            if (ply == 0) bestRootMove = localBestMove;
            // not updating the tt move in qsearch gives close to 20 elo (with close to 20 elo error bounds, but meassured two times with 1000 games each)
            if (!inQsearch)
                ttEntry = new(board.ZobristKey, localBestMove, (short)bestScore,
                    (sbyte)(bestScore <= originalAlpha ? -1 : bestScore >= beta ? 1 : 0), (sbyte)remainingDepth);
            //ttEntry.bestMove = localBestMove;

            return bestScore;
        }


        // for the time being, this is very closely based on JW's example bot (ie tier 2 bot)
        int eval()
        {
            int phase = 0, mg = 0, eg = 0;
            foreach (bool stm in new[] { true, false })
            {
                for (var p = PieceType.None; ++p <= PieceType.King;)
                {
                    int piece = (int)p - 1;
                    ulong mask = board.GetPieceBitboard(p, stm);
                    while (mask != 0)
                    {
                        phase += pesto[768 + piece];
                        int index = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0) + 64 * piece;
                        // The (47 << piece) trick doesn't really save all that much at the moment, but...
                        // ...TODO: By storing mg tables first, then eg tables, this code can be reused for mg and eg calculation, potentially saving a few tokens
                        //if (piece == 5 /*&& stm == us*/)
                        //{
                        //    int rank = square >> 3;
                        //    mg += 32 * (stm ? rank : 7 - rank);
                        //} else
                        mg += pesto[index] + (47 << piece) + pesto[piece + 776];
                        eg += pesto[index + 384] + (47 << piece) + pesto[piece + 782];

                        //                    // passed pawns detection, doesn't increase elo
                        //                    ulong fileMask = (((0x700ul << square % 8 >> 1) & 0xff00) * 0x1010101010101);
                        //                    ulong passedPawnMask = (stm ? fileMask << (square & 0xf8) : fileMask >> (8 + (square & 0xf8 ^ 56)));
                        //                    if (piece == 0 && (b.GetPieceBitboard(p, !stm) & passedPawnMask) == 0)
                        //                    {
                        //#if PRINT_DEBUG_INFO
                        //                        BitboardHelper.VisualizeBitboard(passedPawnMask);
                        //                        Console.WriteLine("Passed pawn: {0}", newSquare(square).ToString());
                        //#endif
                        //                        eg += (square ^ (stm ? 0 : 56)) / 8 * 20;
                        //                    }
                    }
                }

                // king safety: Is the king on a semi open file?
                //+ (((b.GetPieceBitboard(PieceType.Pawn, !stm) >> b.GetKingSquare(stm).File) & 0x0001_0101_0101_0101) == 0 ? 0 : 50)
                // king safety: Replace the king by a virtual queen and count the number of squares it can reach as a measure of how open the king is.
                // This is a very crude approximation, but doesn't require too many tokens. (Scale by negative amount for endgame?)
                //mg -= 7 * BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPieceAttacks(PieceType.Queen, b.GetKingSquare(stm), stm ? b.WhitePiecesBitboard : b.BlackPiecesBitboard, stm));
                mg = -mg;
                eg = -eg;
            }

            // mopup: Bring the king closer to the opponent's king when there are no more pawns and we are ahead in material. Doesn't gain a lot of elo.
            //Square whiteKing = board.GetKingSquare(true), blackKing = board.GetKingSquare(false);
            //if (phase < 7 && board.GetAllPieceLists()[0].Count + board.GetAllPieceLists()[6].Count == 0)
            //    eg += (12 - Abs(whiteKing.Rank - blackKing.Rank) + Abs(whiteKing.File - blackKing.File)) * (eg > 300 ? 12 : eg < -300 ? -12 : 0);
            // TODO: The bot still undervalues pawns in endgames, eg preferring a bishop to two pawns

            return (mg * phase + eg * (24 - phase)) / (board.IsWhiteToMove ? 24 : -24);

            // int res = (mg * phase + eg * (24 - phase)) / 24 * (b.IsWhiteToMove ? 1 : -1);
            // int expected = Pesto.originalPestoEval(b);
            // if (res != expected)
            // {
            //     Console.WriteLine("eval was {0}, should be {1}", res, expected);
            //     Console.WriteLine(b.CreateDiagram());
            //     Debug.Assert(false);
            // }
            // return res;
        }


        //        int lastEval;
        //        int lastDepth;

        //        public BotInfo Info()
        //        {
        //            return new BotInfo(lastDepth, lastEval);
        //        }


        //        Board board;
        //        Timer timer;
        //        Move bestmoveRoot = default;
        //        int currphase;
        //        struct TTEntry
        //        {
        //            public ulong key;
        //            public Move move;
        //            public int depth, score, bound;
        //            public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound)
        //            {
        //                key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
        //            }
        //        }
        //        TTEntry[] tt = new TTEntry[0x400000];
        //        int[] phase_weight = { 0, 1, 1, 2, 4, 0 };
        //        short[] pvm = { 82, 337, 365, 477, 1025, 0,
        //                            94, 281, 297, 512,  936, 0 };
        //        int[][] UnpackedPestoTables;
        //        Move[] Killer = new Move[2048];
        //        int[,,] HistoryHeuristics;
        //        int searchMaxTime;
        //#if INFO
        //            long nodes; // #DEBUG
        //#endif
        //        int Evaluate()
        //        {
        //            int mg = 0, eg = 0;
        //            currphase = 0;
        //            foreach (bool stm in new[] { true, false })
        //            {
        //                for (int piece = -1; ++piece < 6;)
        //                {
        //                    ulong bb = board.GetPieceBitboard((PieceType)(piece + 1), stm);
        //                    while (bb != 0)
        //                    {
        //                        int sq = BitboardHelper.ClearAndGetIndexOfLSB(ref bb) ^ (stm ? 56 : 0);
        //                        mg += UnpackedPestoTables[sq][piece];
        //                        eg += UnpackedPestoTables[sq][piece + 6];
        //                        currphase += phase_weight[piece];
        //                    }
        //                }

        //                mg = -mg;
        //                eg = -eg;
        //            }
        //            currphase = Math.Min(currphase, 24);
        //            return (mg * currphase + eg * (24 - currphase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
        //        }

        //        public int Search(int alpha, int beta, int depth, int ply, bool canNullMove = true)
        //        {
        //#if INFO
        //                nodes++; // #DEBUG
        //#endif
        //            ulong key = board.ZobristKey;
        //            bool qsearch = depth <= 0;
        //            bool notRoot = ply > 0;
        //            int best = -9999999;
        //            int stm = board.IsWhiteToMove ? 0 : 1;

        //            if (notRoot && board.IsRepeatedPosition())
        //                return 0;

        //            TTEntry entry = tt[key & 0x3FFFFF];

        //            if (notRoot && entry.key == key && entry.depth >= depth && (
        //                entry.bound == 3
        //                    || entry.bound == 2 && entry.score >= beta
        //                    || entry.bound == 1 && entry.score <= alpha
        //            )) return entry.score;

        //            int eval = Evaluate();

        //            if (qsearch)
        //            {
        //                best = eval;
        //                if (best >= beta) return best;
        //                alpha = Math.Max(alpha, best);
        //            }

        //            else if (beta - alpha == 1 && !board.IsInCheck())
        //            {
        //                int staticEval = Evaluate();
        //                if (depth <= 10 && staticEval - 94 * depth >= beta)
        //                    return staticEval;

        //                if (canNullMove && !board.IsInCheck())
        //                {
        //                    board.ForceSkipTurn();
        //                    int nullMoveEval = -Search(-beta, -beta + 1, depth - 3, ply + 1, false);
        //                    board.UndoSkipTurn();

        //                    if (beta <= nullMoveEval)
        //                        return nullMoveEval;
        //                }
        //                if (depth <= 3 && staticEval + 620 <= alpha)
        //                    depth--;
        //            }

        //            var moves = board.GetLegalMoves(qsearch).OrderByDescending(
        //                move =>
        //                    move == entry.move ? 1000000 :
        //                    move.IsCapture ? 10000 * (int)move.CapturePieceType - (int)move.MovePieceType :
        //                    Killer[ply] == move ? 900000 :
        //                    move.IsPromotion ? 100000 : HistoryHeuristics[stm, move.StartSquare.Index, move.TargetSquare.Index]
        //            ).ToArray();

        //            Move bestMove = Move.NullMove;
        //            int origAlpha = alpha;

        //            if (board.IsInCheck()) depth++;

        //            for (int i = 0; i < moves.Length; i++)
        //            {
        //                if (timer.MillisecondsElapsedThisTurn >= searchMaxTime)
        //                    return 9999999;

        //                Move move = moves[i];
        //                bool needFullSearch = true;
        //                int score = 0;

        //                board.MakeMove(move);
        //                if (i >= 3 && depth >= 3 && !move.IsCapture)
        //                {
        //                    score = -Search(-alpha - 1, -alpha, depth - 2, ply + 1);
        //                    needFullSearch = score > alpha;
        //                }
        //                if (needFullSearch)
        //                    score = -Search(-beta, -alpha, depth - 1, ply + 1);
        //                board.UndoMove(move);

        //                if (score > best)
        //                {
        //                    best = score;
        //                    bestMove = move;
        //                    if (ply == 0) bestmoveRoot = move;
        //                    alpha = Math.Max(alpha, score);
        //                    if (alpha >= beta)
        //                    {
        //                        if (!move.IsCapture)
        //                        {
        //                            HistoryHeuristics[stm, move.StartSquare.Index, move.TargetSquare.Index] += depth * depth;
        //                            Killer[ply] = move;
        //                        }
        //                        break;
        //                    }
        //                }
        //            }

        //            if (!qsearch && moves.Length == 0) return board.IsInCheck() ? ply - 999999 : 0;
        //            tt[key & 0x3FFFFF] = new TTEntry(key, bestMove, depth, best, best >= beta ? 2 : best > origAlpha ? 3 : 1);

        //            return best;
        //        }
        //        public Move Think(Board _board, Timer _timer)
        //        {
        //#if INFO
        //                Console.WriteLine(""); // #DEBUG
        //                nodes = 0; // #DEBUG
        //#endif
        //            board = _board;
        //            timer = _timer;
        //            HistoryHeuristics = new int[2, 64, 64];
        //            bestmoveRoot = default;
        //            searchMaxTime = timer.MillisecondsRemaining / 30;
        //            for (int depth = 2, eval, alpha = -9999999, beta = 9999999; ;)
        //            {
        //                eval = Search(alpha, beta, depth, 0);
        //                lastDepth = depth;
        //                lastEval = eval;
        //                if (timer.MillisecondsElapsedThisTurn >= searchMaxTime)
        //                    break;
        //                if (eval <= alpha)
        //                {
        //                    //beta = (alpha + beta) / 2;
        //                    alpha -= 14;
        //                }
        //                else if (eval >= beta)
        //                    beta += 14;
        //                else
        //                {
        //#if INFO
        //                        string evalWithMate = eval.ToString(); // #DEBUG
        //                        if (Math.Abs(eval) > 999900 && Math.Abs(eval) <= 999999) // #DEBUG
        //                        { // #DEBUG
        //                            evalWithMate = eval < 0 ? "-M" : "M"; // #DEBUG
        //                            evalWithMate += Math.Ceiling((double)(999999 - Math.Abs(eval)) / 2).ToString(); // #DEBUG
        //                        } // #DEBUG
        //                        int timeElapsed = timer.MillisecondsElapsedThisTurn; // #DEBUG
        //                        Console.WriteLine("Info: depth: {0, 2} || eval: {1, 6} || nodes: {2, 9} || nps: {3, 8} || time: {4, 5}ms || best move: {5}{6}", // #DEBUG
        //                                          depth, // #DEBUG
        //                                          evalWithMate, // #DEBUG
        //                                          nodes, // #DEBUG
        //                                          1000 * nodes / (timeElapsed + 1), // #DEBUG
        //                                          timeElapsed,
        //                                          bestmoveRoot.StartSquare.Name, // #DEBUG
        //                                          bestmoveRoot.TargetSquare.Name // #DEBUG
        //                        ); // #DEBUG
        //#endif
        //                    alpha = eval - 14; beta = eval + 14;
        //                    depth++;
        //                }
        //            }
        //            return bestmoveRoot;
        //        }
        //        public EvilBot()
        //        {
        //            UnpackedPestoTables = new[] {
        //                    63746705523041458768562654720m, 71818693703096985528394040064m, 75532537544690978830456252672m, 75536154932036771593352371712m, 76774085526445040292133284352m, 3110608541636285947269332480m, 936945638387574698250991104m, 75531285965747665584902616832m,
        //                    77047302762000299964198997571m, 3730792265775293618620982364m, 3121489077029470166123295018m, 3747712412930601838683035969m, 3763381335243474116535455791m, 8067176012614548496052660822m, 4977175895537975520060507415m, 2475894077091727551177487608m,
        //                    2458978764687427073924784380m, 3718684080556872886692423941m, 4959037324412353051075877138m, 3135972447545098299460234261m, 4371494653131335197311645996m, 9624249097030609585804826662m, 9301461106541282841985626641m, 2793818196182115168911564530m,
        //                    77683174186957799541255830262m, 4660418590176711545920359433m, 4971145620211324499469864196m, 5608211711321183125202150414m, 5617883191736004891949734160m, 7150801075091790966455611144m, 5619082524459738931006868492m, 649197923531967450704711664m,
        //                    75809334407291469990832437230m, 78322691297526401047122740223m, 4348529951871323093202439165m, 4990460191572192980035045640m, 5597312470813537077508379404m, 4980755617409140165251173636m, 1890741055734852330174483975m, 76772801025035254361275759599m,
        //                    75502243563200070682362835182m, 78896921543467230670583692029m, 2489164206166677455700101373m, 4338830174078735659125311481m, 4960199192571758553533648130m, 3420013420025511569771334658m, 1557077491473974933188251927m, 77376040767919248347203368440m,
        //                    73949978050619586491881614568m, 77043619187199676893167803647m, 1212557245150259869494540530m, 3081561358716686153294085872m, 3392217589357453836837847030m, 1219782446916489227407330320m, 78580145051212187267589731866m, 75798434925965430405537592305m,
        //                    68369566912511282590874449920m, 72396532057599326246617936384m, 75186737388538008131054524416m, 77027917484951889231108827392m, 73655004947793353634062267392m, 76417372019396591550492896512m, 74568981255592060493492515584m, 70529879645288096380279255040m,
        //                }.Select(packedTable =>
        //                new System.Numerics.BigInteger(packedTable).ToByteArray().Take(12)
        //                            .Select(square => (int)((sbyte)square * 1.461) + pvm[searchMaxTime++ % 12])
        //                        .ToArray()
        //            ).ToArray();
        //        }
    }
}