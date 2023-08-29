#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
#endif

using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        private int[,,] history = new int[2, 7, 64];

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

        void printPvs()
        {
            //var move = ttMoves[board.ZobristKey & 0x1ff_ffff];
            //if (board.GetLegalMoves().Contains(move))
            //{
            //    Console.WriteLine(move);
            //    board.MakeMove(move);
            //    printPvs();
            //    board.UndoMove(move);
            //}
        }
#endif

        #region compresto

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

        private byte[] pesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();

        #endregion


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


            // TODO: use = new int[] to save tokens
            Array.Clear(history, 0, history.Length);
            //((IList)history).Clear(); // saves 2 tokens here but requires using System.Collection, so + 2 tokens overall

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
            for (int depth = 1; depth++ < 50 && !shouldStopThinking();)
            {
                int score = negamax(depth, -30_000, 30_000, 0, false);
#if PRINT_DEBUG_INFO
                Console.WriteLine("Depth {0}, score {1}, best move {2}", depth, score, bestRootMove);
#endif
            }

#if PRINT_DEBUG_INFO
            Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " + betaCutoffCtr
                + ", percent cutting (higher is better): " + (1.0 * betaCutoffCtr / allNodeCtr).ToString("P1")
                + ", percent cutting for parents of inner nodes: " + (1.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("P1"));
            Console.WriteLine("Tried PVS {0} times, retried {1} times ({2:P2})", pvsTryCtr, pvsRetryCtr, (double)pvsRetryCtr / pvsTryCtr);
            Console.WriteLine("NPS: {0}k", (allNodeCtr / (double)timer.MillisecondsElapsedThisTurn).ToString("0.0"));
            Console.WriteLine("Time:{0} of {1} ms, remaining {2}", timer.MillisecondsElapsedThisTurn, timer.GameStartTimeMilliseconds, timer.MillisecondsRemaining);
            Console.WriteLine("PV: ");
            Console.WriteLine();
            allNodeCtr = 0;
            nonQuiescentNodeCtr = 0;
            betaCutoffCtr = 0;
            parentOfInnerNodeCtr = 0;
            parentOfInnerNodeBetaCutoffCtr = 0;
            pvsTryCtr = 0;
            pvsRetryCtr = 0;
#endif

            return bestRootMove;
        }


        int negamax(int remainingDepth, int alpha, int beta, int ply, bool allowNmp)
        {
#if PRINT_DEBUG_INFO
            ++allNodeCtr;
            if (remainingDepth > 0) ++nonQuiescentNodeCtr;
            if (remainingDepth > 1) ++parentOfInnerNodeCtr;
#endif

            // Using stackalloc doesn't gain elo
            bool isRoot = ply == 0,
                inQsearch = remainingDepth <= 0,
                isPvNode = alpha + 1 < beta;
            var legalMoves = board.GetLegalMoves(inQsearch);
            int numMoves = legalMoves.Length,
                bestScore = -32_000,
                originalAlpha = alpha,
                standPat = eval(),
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

            // reverse futility probing
            int margin = 64 * remainingDepth;
            if (!isPvNode && !inCheck && !inQsearch && remainingDepth < 5 && standPat >= beta + margin)
            {
                return standPat;
            }

            // Null move pruning. TODO: Avoid zugzwang by testing phase?
            if (!isPvNode && remainingDepth >= 4 && allowNmp && standPat >= beta && board.TrySkipTurn())
            {
                int reduction = 3 + remainingDepth / 5;
                // changing the ply by a large number doesn't seem to gain elo, even though this should prevent overwriting killer moves
                int nullScore = -negamax(remainingDepth - reduction, -beta, -alpha, ply + 1, false);
                board.UndoSkipTurn();
                if (nullScore >= beta)
                {
                    return nullScore;
                }
            }

            //ref Move ttMove = ref ttMoves[board.ZobristKey & 0x1ff_ffff];
            ref TTEntry ttEntry = ref tt[board.ZobristKey & 0x7f_ffff];

            if ((ttEntry.score <= alpha && ttEntry.flag == -1
                || ttEntry.score >= beta && ttEntry.flag == 1 ||
                ttEntry.flag == 0) && ttEntry.depth >= remainingDepth && ttEntry.key == board.ZobristKey && !isPvNode)
            {
                return ttEntry.score;
            }

            // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending
            var scores = new int[numMoves];
            for (int i = 0; i < numMoves; i++)
            {
                Move move = legalMoves[i];
                scores[i] = move == ttEntry.bestMove ? -1_000_000_000 : move.IsCapture ? (int)move.MovePieceType - (int)move.CapturePieceType * 1_000_000 :
                    move == killers[2 * ply] || move == killers[2 * ply + 1] ? -100_000 :
                    -history[board.IsWhiteToMove ? 1 : 0, (int)move.MovePieceType, move.TargetSquare.Index];
            }
            Array.Sort(scores, legalMoves);

            Move localBestMove = Move.NullMove;
            for (int moveIdx = 0; moveIdx < legalMoves.Length; ++moveIdx)
            {
                Move move = legalMoves[moveIdx];
                int newDepth = remainingDepth - 1;
                board.MakeMove(move);
                if (moveIdx == 0) // pvs like this is -7 +- 20 elo after 1000 games; adding inQsearch || ... doesn't change that, nor does move == ttMove
                {
                    score = -negamax(newDepth + (board.IsInCheck() ? 1 : 0), -beta, -alpha, ply + 1, true);
                }
                else
                {
#if PRINT_DEBUG_INFO
                    ++pvsTryCtr;
#endif
                    // testing ongoing, most conditions seem like they make sense but don't add elo.
                    // !isRoot seems to result in a small improvement, at least. So far, reducing pv nodes less seems to lose elo
                    int reduction = 0;
                    if (moveIdx >= (isPvNode ? 5 : 3)
                        && remainingDepth > 3
                        && !move.IsCapture
                        && !inCheck)
                    {
                        reduction = 3; // TODO: Once the engine is better, test with viri values: (int)(0.77 + Math.Log(remainingDepth) * Math.Log(i) / 2.36);
                        reduction -= isPvNode ? 1 : 0;
                        reduction = Math.Clamp(reduction, 0, remainingDepth - 2);
                    }
                    score = -negamax(newDepth - reduction, -alpha - 1, -alpha, ply + 1, true);
                    if (alpha < score && score < beta)
                    {
#if PRINT_DEBUG_INFO
                        ++pvsRetryCtr;
#endif
                        score = -negamax(newDepth, -beta, -alpha, ply + 1, true);
                    }
                }

                board.UndoMove(move);

                if (shouldStopThinking())
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

            if (isRoot) bestRootMove = localBestMove;
            // not updating the tt move in qsearch gives close to 20 elo (with close to 20 elo error bounds, but meassured two times with 1000 games each)
            if (!inQsearch)
                ttEntry = new(board.ZobristKey, localBestMove, (short)bestScore, (sbyte)(bestScore <= originalAlpha ? -1 : bestScore >= beta ? 1 : 0), (sbyte)remainingDepth);
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
                    int piece = (int)p - 1, square, index; // square isn't necessary at this point, but useful for other evaluation features
                    ulong mask = board.GetPieceBitboard(p, stm);
                    while (mask != 0)
                    {
                        phase += pesto[768 + piece];
                        square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask);
                        index = square ^ (stm ? 56 : 0) + 64 * piece;
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
            //    eg += (12 - Math.Abs(whiteKing.Rank - blackKing.Rank) + Math.Abs(whiteKing.File - blackKing.File)) * (eg > 300 ? 12 : eg < -300 ? -12 : 0);
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
        //    bestmoveRoot = Move.NullMove;
        //    // https://www.chessprogramming.org/Iterative_Deepening
        //    for (int depth = 1; depth <= 50; depth++)
        //    {
        //        int score = Search(board, timer, -30000, 30000, depth, 0);

        //        // Out of time
        //        if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
        //            break;
        //    }
        //    return bestmoveRoot.IsNull ? board.GetLegalMoves()[0] : bestmoveRoot;
        //}
    }
}