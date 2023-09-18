#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
#define GUI_INFO
#endif

using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // TODO: Not necesdsary except for decompression
using static System.Math;
using static System.Convert;

namespace ChessChallenge.Example
{

    public class EvilBot : IChessBot
    {

    // public static int[] piecePhase = new int[]{ 0, 1, 1, 2, 4, 0 };
    //     
    // public static int[] pestoPieceValues = new int[]{82, 337, 365, 477, 1025, 100,  94, 281, 297, 512, 936, 100};
    //
    // public static int[][] pestoPsqts =
    //     new int[][]{
    //         new int[]
    //         {
    //             // pawn mg
    //             0, 0, 0, 0, 0, 0, 0, 0,
    //             98, 134, 61, 95, 68, 126, 34, -11,
    //             -6, 7, 26, 31, 65, 56, 25, -20,
    //             -14, 13, 6, 21, 23, 12, 17, -23,
    //             -27, -2, -5, 12, 17, 6, 10, -25,
    //             -26, -4, -4, -10, 3, 3, 33, -12,
    //             -35, -1, -20, -23, -15, 24, 38, -22,
    //             0, 0, 0, 0, 0, 0, 0, 0
    //         },
    //         new int[]
    //         {
    //             // knight mg
    //             -167, -89, -34, -49, 61, -97, -15, -107,
    //             -73, -41, 72, 36, 23, 62, 7, -17,
    //             -47, 60, 37, 65, 84, 129, 73, 44,
    //             -9, 17, 19, 53, 37, 69, 18, 22,
    //             -13, 4, 16, 13, 28, 19, 21, -8,
    //             -23, -9, 12, 10, 19, 17, 25, -16,
    //             -29, -53, -12, -3, -1, 18, -14, -19,
    //             -105, -21, -58, -33, -17, -28, -19, -23
    //         },
    //         new int[]
    //         {
    //             // bishop mg
    //             -29, 4, -82, -37, -25, -42, 7, -8,
    //             -26, 16, -18, -13, 30, 59, 18, -47,
    //             -16, 37, 43, 40, 35, 50, 37, -2,
    //             -4, 5, 19, 50, 37, 37, 7, -2,
    //             -6, 13, 13, 26, 34, 12, 10, 4,
    //             0, 15, 15, 15, 14, 27, 18, 10,
    //             4, 15, 16, 0, 7, 21, 33, 1,
    //             -33, -3, -14, -21, -13, -12, -39, -21
    //         },
    //         new int[]
    //         {
    //             // rook mg
    //             32, 42, 32, 51, 63, 9, 31, 43,
    //             27, 32, 58, 62, 80, 67, 26, 44,
    //             -5, 19, 26, 36, 17, 45, 61, 16,
    //             -24, -11, 7, 26, 24, 35, -8, -20,
    //             -36, -26, -12, -1, 9, -7, 6, -23,
    //             -45, -25, -16, -17, 3, 0, -5, -33,
    //             -44, -16, -20, -9, -1, 11, -6, -71,
    //             -19, -13, 1, 17, 16, 7, -37, -26
    //         },
    //         new int[]
    //         {
    //             // queen mg
    //             -28, 0, 29, 12, 59, 44, 43, 45,
    //             -24, -39, -5, 1, -16, 57, 28, 54,
    //             -13, -17, 7, 8, 29, 56, 47, 57,
    //             -27, -27, -16, -16, -1, 17, -2, 1,
    //             -9, -26, -9, -10, -2, -4, 3, -3,
    //             -14, 2, -11, -2, -5, 2, 14, 5,
    //             -35, -8, 11, 2, 8, 15, -3, 1,
    //             -1, -18, -9, 10, -15, -25, -31, -50
    //         },
    //         //new int[]
    //         //{
    //         //    // king mg
    //         //    -65, 23, 16, -15, -56, -34, 2, 13,
    //         //    29, -1, -20, -7, -8, -4, -38, -29,
    //         //    -9, 24, 2, -16, -20, 6, 22, -22,
    //         //    -17, -20, -12, -27, -30, -25, -14, -36,
    //         //    -49, -1, -27, -39, -46, -44, -33, -51,
    //         //    -14, -14, -22, -46, -44, -30, -15, -27,
    //         //    1, 7, -8, -64, -43, -16, 9, 8,
    //         //    -15, 36, 12, -54, 8, -28, 24, 14
    //         //},
    //         // new int[]
    //         // {
    //         //     // king mg, King on the Hill
    //         //     0,  0,  0,  0,  0,  0,  0,  0,
    //         //     0,  32, 64, 64, 64, 64, 32, 0,
    //         //     0,  32, 120,128,128,120,32, 0,
    //         //     0,  32, 128,255,255,128,32, 0,
    //         //     0,  32, 128,255,255,128,32, 0,
    //         //     0,  32, 120,128,128,120,32, 0,
    //         //     32, 32, 64, 64, 64, 64, 32, 32,
    //         //     0,  16, 16, 0,  16, 0,  16, 16
    //         // },
    //         new int[]
    //         {
    //             // king mg, King on the Hill
    //             7,  7,  7,  7,  7,  7,  7,  7,
    //             6,  25, 50, 50, 50, 50, 25, 6,
    //             5,  25, 85 ,150,150,85 ,25, 5,
    //             4,  25, 100,200,200,100,25, 4,
    //             3,  25, 100,200,200,100,25, 3,
    //             2,  25, 75, 100,100,75 ,25, 2,
    //             1,  25, 50, 50, 50, 50, 25, 1,
    //             0,  0,  8,  0,  0,  0,  8,  0
    //         },
    //         // new int[]
    //         // {
    //         //     // king mg, Gᴀᴍʙᴏᴛ
    //         //     255, 255, 255, 255, 255, 255, 255, 255,
    //         //     255, 255, 255, 255, 255, 255, 255, 255,
    //         //     255, 255, 255, 255, 255, 255, 255, 255,
    //         //     250, 250, 250, 250, 250, 250, 250, 250,
    //         //     200, 200, 200, 200, 200, 200, 200, 200,
    //         //     100, 100, 100, 100, 100, 100, 100, 100,
    //         //     10, 10, 50, 50, 50, 50, 30, 10,
    //         //     0, 0, 0, 3, 5, 2, 0, 0
    //         // },
    //         new int[]
    //         {
    //             // pawn eg
    //             0, 0, 0, 0, 0, 0, 0, 0,
    //             178, 173, 158, 134, 147, 132, 165, 187,
    //             94, 100, 85, 67, 56, 53, 82, 84,
    //             32, 24, 13, 5, -2, 4, 17, 17,
    //             13, 9, -3, -7, -7, -8, 3, -1,
    //             4, 7, -6, 1, 0, -5, -1, -8,
    //             13, 8, 8, 10, 13, 0, 2, -7,
    //             0, 0, 0, 0, 0, 0, 0, 0,
    //         },
    //         new int[]
    //         {
    //             // knight eg
    //             -58, -38, -13, -28, -31, -27, -63, -99,
    //             -25, -8, -25, -2, -9, -25, -24, -52,
    //             -24, -20, 10, 9, -1, -9, -19, -41,
    //             -17, 3, 22, 22, 22, 11, 8, -18,
    //             -18, -6, 16, 25, 16, 17, 4, -18,
    //             -23, -3, -1, 15, 10, -3, -20, -22,
    //             -42, -20, -10, -5, -2, -20, -23, -44,
    //             -29, -51, -23, -15, -22, -18, -50, -64
    //         },
    //         new int[]
    //         {
    //             // bishop eg
    //             -14, -21, -11, -8, -7, -9, -17, -24,
    //             -8, -4, 7, -12, -3, -13, -4, -14,
    //             2, -8, 0, -1, -2, 6, 0, 4,
    //             -3, 9, 12, 9, 14, 10, 3, 2,
    //             -6, 3, 13, 19, 7, 10, -3, -9,
    //             -12, -3, 8, 10, 13, 3, -7, -15,
    //             -14, -18, -7, -1, 4, -9, -15, -27,
    //             -23, -9, -23, -5, -9, -16, -5, -17
    //         },
    //         new int[]
    //         {
    //             // rook eg
    //             13, 10, 18, 15, 12, 12, 8, 5,
    //             11, 13, 13, 11, -3, 3, 8, 3,
    //             7, 7, 7, 5, 4, -3, -5, -3,
    //             4, 3, 13, 1, 2, 1, -1, 2,
    //             3, 5, 8, 4, -5, -6, -8, -11,
    //             -4, 0, -5, -1, -7, -12, -8, -16,
    //             -6, -6, 0, 2, -9, -9, -11, -3,
    //             -9, 2, 3, -1, -5, -13, 4, -20
    //         },
    //         new int[]
    //         {
    //             // queen eg
    //             -9, 22, 22, 27, 27, 19, 10, 20,
    //             -17, 20, 32, 41, 58, 25, 30, 0,
    //             -20, 6, 9, 49, 47, 35, 19, 9,
    //             3, 22, 24, 45, 57, 40, 57, 36,
    //             -18, 28, 19, 47, 31, 34, 39, 23,
    //             -16, -27, 15, 6, 9, 17, 10, 5,
    //             -22, -23, -30, -16, -16, -23, -36, -32,
    //             -33, -28, -22, -43, -5, -32, -20, -41
    //         },
    //         new int[]
    //         {
    //             // king eg
    //             -74, -35, -18, -18, -11, 15, 4, -17,
    //             -12, 17, 14, 17, 17, 38, 23, 11,
    //             10, 17, 23, 15, 20, 45, 44, 13,
    //             -8, 22, 24, 27, 26, 33, 26, 3,
    //             -18, -4, 21, 24, 27, 23, 9, -11,
    //             -19, -3, 11, 21, 23, 16, 7, -9,
    //             -27, -11, 4, 13, 14, 4, -5, -17,
    //             -53, -34, -21, -11, -28, -14, -24, -43
    //         },
    //     };

    
    // TODO: Better TM
    // TODO: FP, LMP
    
    public record struct TTEntry (
        ulong key,
        Move bestMove,
        short score,
        byte flag, // 1 = upper, > 1 = exact, 0 = lower
        sbyte depth
    );

    // 1 << 25 entries (without the pesto values, it would technically be possible to store exactly 1 << 26 Moves with 4 byte per Move)
    // the tt move ordering alone gained almost 400 elo
    //private Move[] ttMoves = new Move[0x200_0000];

    private TTEntry[] tt = new TTEntry[0x80_0000];
        
    private Move bestRootMove, chosenMove;

    private bool stmColor;

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
    long awRetryCtr;

#endif
    
#if GUI_INFO

    int lastDepth;
    int lastScore;

    public BotInfo Info()
    {
        return new BotInfo(lastDepth, lastScore);
    }

#endif

    #region compresto

    // TODO: Can Decimal to string conversion be used to get more than 96 bits out of a Decimal?
    
#if NO_JOKE

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

    // values are from pesto for now, with a modified king middle game table unless NO_JOKE is defined
    private byte[] pesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();

    #endregion // compresto



    public Move Think(Board board, Timer timer)
    {
        // bool shouldStopThinking() => // TODO: Inline this to save tokens
            // The / 32 makes the most sense when the game last for another 32 moves. Currently, poor bot performance causes unnecesarily
            // long games in winning positions but as we improve our engine we should see less time trouble overall.
            // timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 32;

        var history = new int[2, 7, 64];
        var killers = new Move[256];
        
        // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
        for (int depth = 1, alpha = -30_000, beta = 30_000; depth < 64 && timer.MillisecondsElapsedThisTurn <= timer.MillisecondsRemaining / 32;)
        {
            // TODO: This should be bugged when out of time when the last score failed low on the asp window
            int score = negamax(depth, alpha, beta, 0, false);
            // excluding checkmate scores was inconclusive after 6000 games, so likely not worth the tokens
            if (score <= alpha) alpha += score - alpha;
            else if (score >= beta) {
                beta += score - beta;
                chosenMove = bestRootMove; 
            }
            else
            {
#if PRINT_DEBUG_INFO
                Console.WriteLine("Depth {0}, score {1}, best {2}, nodes {3}k, time {4}, nps {5}k",
                    depth, score, bestRootMove, Round(allNodeCtr / 1000.0), timer.MillisecondsElapsedThisTurn,
                    Round(allNodeCtr / (double)timer.MillisecondsElapsedThisTurn, 1));
#endif
#if GUI_INFO
                lastDepth = depth;
                if (score != 12345) lastScore = score;
#endif
                alpha = beta = score;
                chosenMove = bestRootMove;
                ++depth;
            }

#if PRINT_DEBUG_INFO
            ++awRetryCtr;
#endif
            // tested values: 8, 15, 20, 30 (15 being the second best)
            alpha -= 20;
            beta += 20;
        }

#if PRINT_DEBUG_INFO
        Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " +
                          betaCutoffCtr
                          + ", percent cutting (higher is better): " + (1.0 * betaCutoffCtr / allNodeCtr).ToString("P1")
                          + ", percent cutting for parents of inner nodes: " +
                          (1.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("P1")
                          + ", aspiration window retries: " + (awRetryCtr - lastDepth));
        Console.WriteLine("NPS: {0}k", (allNodeCtr / (double)timer.MillisecondsElapsedThisTurn).ToString("0.0"));
        Console.WriteLine("Time:{0} of {1} ms, remaining {2}", timer.MillisecondsElapsedThisTurn,
            timer.GameStartTimeMilliseconds, timer.MillisecondsRemaining);
        Console.Write("PV: ");
        printPv();
        Console.WriteLine();
        allNodeCtr = 0;
        nonQuiescentNodeCtr = 0;
        betaCutoffCtr = 0;
        parentOfInnerNodeCtr = 0;
        parentOfInnerNodeBetaCutoffCtr = 0;
        awRetryCtr = 0;
        
    void printPv(int remainingDepth = 15)
    {
        var entry = tt[board.ZobristKey & 0x7f_ffff];
        var move = entry.bestMove;
        if (board.ZobristKey == entry.key && board.GetLegalMoves().Contains(move) && remainingDepth > 0)
        {
            Console.Write(move + " ");
            board.MakeMove(move);
            printPv(remainingDepth - 1);
            board.UndoMove(move);
        }
    }
#endif

        return chosenMove;

        // halfPly (ie quarter move) instead of ply to save tokens when accessing killers
        int negamax(int remainingDepth, int alpha, int beta, int halfPly, bool allowNmp)
        {
#if PRINT_DEBUG_INFO
            ++allNodeCtr;
            if (remainingDepth > 0) ++nonQuiescentNodeCtr;
            if (remainingDepth > 1) ++parentOfInnerNodeCtr;
#endif

            // Using stackalloc doesn't gain elo
            bool inQsearch = remainingDepth <= 0,
                isNotPvNode = alpha + 1 >= beta,
                inCheck = board.IsInCheck(),
                canPrune = isNotPvNode && !inCheck;
            stmColor = board.IsWhiteToMove;

            int bestScore = -32_000,
                // originalAlpha = alpha,
                standPat = eval(),
                moveIdx = 0,
                score;

            byte flag = 1;

            if (halfPly > 0 && board.IsRepeatedPosition())
                return 0;

            if (inQsearch)
            {
                if (standPat >= beta) return standPat;
                alpha = Max(alpha, bestScore = standPat);
            }
            
            // Check Extensions
            if (inCheck) ++remainingDepth; // TODO: Do this before setting qsearch to disallow dropping to qsearch in check? Probably unimportant

            // TODO: Use tt for stand pat score?
            // TODO: Use tuple as TT entries
            ref TTEntry ttEntry = ref tt[board.ZobristKey & 0x7f_ffff];

            if (isNotPvNode && ttEntry.depth >= remainingDepth && ttEntry.key == board.ZobristKey
                    && ttEntry.flag != 0 | ttEntry.score >= beta // Flag cut-off condition by Broxholme
                    && ttEntry.flag != 1 | ttEntry.score <= alpha)
                return ttEntry.score;

            int search(int minusNewAlpha, int reduction = 1, bool allowNullMovePruning = true) =>
                score = -negamax(remainingDepth - reduction, -minusNewAlpha, -alpha, halfPly + 2, allowNullMovePruning);

            if (canPrune)
            {
                // Reverse Futility Pruning (RFP) // TODO: Don't do in check? Increase depth?
                if (!inQsearch && remainingDepth < 5 && standPat >= beta + 64 * remainingDepth)
                    return standPat;

                // Null Move Pruning (NMP). TODO: Avoid zugzwang by testing phase? Probably not worth the tokens
                if (remainingDepth >= 4 && allowNmp && standPat >= beta)
                {
                    board.ForceSkipTurn();
                    //int reduction = 3 + remainingDepth / 5;
                    // changing the ply by a large number doesn't seem to gain elo, even though this should prevent overwriting killer moves
                    search(beta, 3 + remainingDepth / 5, false);
                    board.UndoSkipTurn();
                    if (score >= beta)
                        return score;
                }
            }
            
            // the following is 13 tokens for a slight (not passing SPRT after 10k games) elo improvement
            // killers[halfPly + 2] = killers[halfPly + 3] = default;

            // generate moves
            var legalMoves = board.GetLegalMoves(inQsearch);

            // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending
            var scores = new int[legalMoves.Length];
            foreach (Move move in legalMoves)
            {
                scores[moveIdx++] = -(move == ttEntry.bestMove ? 1_000_000_000 :
                    move.IsCapture ? (int)move.CapturePieceType * 1_048_576  - (int)move.MovePieceType :
                    // Giving the first killer a higher score doesn't seem to gain after 10k games
                    move == killers[halfPly] || move == killers[halfPly + 1] ? 1_000_000 :
                    history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]);
            }

            Array.Sort(scores, legalMoves);

            Move localBestMove = ttEntry.bestMove;
            moveIdx = 0;
            foreach (Move move in legalMoves)
            {
                // TODO: Better Futility Pruning (FP) / Late Move Pruning (LMP)
                // if (remainingDepth <= 5 && bestScore > -29_000 && canPrune
                //     && moveIdx > remainingDepth * remainingDepth + 4 && scores[moveIdx] < 1_000_000 /*|| standPat + 500 + 128 * remainingDepth < alpha*/) break;
                // Futility Pruning (FP). Probably needs more tuning
                if (remainingDepth <= 5 && bestScore > -29_000 && canPrune
                    && scores[moveIdx] > -1_000_000 && standPat + 300 + 64 * remainingDepth < alpha) break;
                board.MakeMove(move);
                // pvs like this is -7 +- 20 elo after 1000 games; adding inQsearch || ... doesn't change that, nor does move == ttMove
                if (moveIdx++ == 0)
                    search(beta);
                else
                {
                    // Late Move Reductions (LMR), needs further parameter tuning. `reduction` is R + 1 to save tokens
                    // TODO: Once the engine is better, test with viri values: (int)(0.77 + Log(remainingDepth) * Log(i) / 2.36);
                    search(alpha + 1, 
                        moveIdx >= (isNotPvNode ? 3 : 4)
                        && remainingDepth > 3
                        && !move.IsCapture
                            ?  // Clamp(3 + ToInt32(isNotPvNode), 1, remainingDepth - 1)
                            Clamp((int)(0.77 + Log(remainingDepth) * Log(moveIdx) / 2.36) + 1 - ToInt32(isNotPvNode), 1, remainingDepth - 1)
                            : 1
                        );
                    // search(alpha + 1, // !! TODO: Test if this is better!
                    //     moveIdx >= (isNotPvNode ? 4 : 6)
                    //                 && remainingDepth > 3
                    //                 && !move.IsCapture
                    //                 && !inCheck
                    //     ?
                    //     Clamp(3 + ToInt32(isNotPvNode), 1, remainingDepth - 1)
                    //     : 1
                    //     );
                    if (alpha < score && score < beta)
                        search(beta);
                }

                board.UndoMove(move);

                if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 32)
                    return 12345; // the value won't be used, so use a canary to detect bugs

                bestScore = Max(score, bestScore);
                if (score > alpha)
                {
                    localBestMove = move;
                    if (halfPly == 0) bestRootMove = localBestMove; // updating here (instead of at the end) together with the aw fix is better, now testing without the aw fix
                    alpha = score;
                    ++flag;
                    if (score >= beta)
                    {
#if PRINT_DEBUG_INFO
                    ++betaCutoffCtr;
                    if (remainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
#endif
                        if (!move.IsCapture)
                        {
                            if (move != killers[halfPly]) // TODO: Test using only 1 killer move
                            {
                                killers[halfPly + 1] = killers[halfPly];
                                killers[halfPly] = move;
                            }

                            // gravity didn't gain (TODO: Retest later when the engine is better), but history still gained quite a bit
                            // TODO: Test from-to instead of stm-piece-to
                            history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]
                                += remainingDepth * remainingDepth;
                        }

                        flag = 0;

                        break;
                    }
                }
            }

            if (moveIdx == 0)
                return inQsearch ? bestScore : inCheck ? halfPly - 30_000 : 0; // being checkmated later is better (as is checkmating earlier)

            ttEntry = new(board.ZobristKey, localBestMove, (short)bestScore,
                flag, (sbyte)remainingDepth);

            return bestScore;
        }
        
        // // Eval loosely based on JW's example bot (ie tier 2 bot)
        // int eval()
        // {
        // //     bool stmColor = board.IsWhiteToMove;
        //     int phase = 0, mg = 7, eg = 7;
        //     foreach (bool isWhite in new[] { stmColor, !stmColor })
        //     {
        //         for (int piece = 6; --piece >= 0;)
        //         {
        //             ulong mask = board.GetPieceBitboard((PieceType)piece + 1, isWhite);
        //             while (mask != 0)
        //             {
        //                 phase += piecePhase[piece];
        //                 int psqtIndex = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^
        //                                 56 * ToInt32(isWhite);
        //                 // TODO: Use pesto[^piece] for endgame piece values (requires reversing the order of eg piece values)
        //                 mg += pestoPsqts[piece][psqtIndex] +  pestoPieceValues[piece];
        //                 eg += pestoPsqts[piece + 6][psqtIndex] + pestoPieceValues[piece + 6];
        //             }
        //         }
        //         mg = -mg;
        //         eg = -eg;
        //     }
        //     return (mg * phase + eg * (24 - phase)) / 24;
        // }
        
        // Eval loosely based on JW's example bot (ie tier 2 bot)
        int eval()
        {
            // bool stmColor = board.IsWhiteToMove;
            int phase = 0, mg = 7, eg = 7;
            foreach (bool isWhite in new[] { stmColor, !stmColor })
            {
                for (int piece = 6; --piece >= 0;)
                {
                    ulong mask = board.GetPieceBitboard((PieceType)piece + 1, isWhite);
                    while (mask != 0)
                    {
                        phase += pesto[768 + piece];
                        int psqtIndex = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^
                                        56 * ToInt32(isWhite) + 64 * piece;
                        // TODO: Use pesto[^piece] for endgame piece values (requires reversing the order of eg piece values)
                        mg += pesto[psqtIndex] + (47 << piece) + pesto[piece + 776];
                        eg += pesto[psqtIndex + 384] + (47 << piece) + pesto[piece + 782];
                    }
                }
                mg = -mg;
                eg = -eg;
            }
            return (mg * phase + eg * (24 - phase)) / 24;
        }
    }
        
        
        
//
//         Board b;
//
//     #region tier_2
//     
//     // int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
//
//     //// JW's compression (which could probably be improved, but evens the playing field when comparing against tier 2 bot
//     //ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };
//
//
//     //int getPstVal(int psq)
//     //{
//     //    return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
//     //}
//     //int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };
//
//     #endregion
//
//
//     // maps a zobrist hash of a position to its score, the search depth at which the score was evaluated,
//     // the score type (lower bound, exact, upper bound), and the best move
//     record struct TTEntry // The size of a TTEntry should be 8 + 4 + 2 + 1 + 1 + no padding = 16 bytes
//     (
//         ulong key, // if we need more space, we could also just store the highest 32 bits since the lowest 23 bits are given by the index, leaving 9 bits unused
//                    // we could also store only the move index, using 1 byte instead of 4, but needing more tokens later on.
//                    // May be worth it if we also merge depth and type and use a 32bit key to get sizeof(TTEntry) down to 8
//         Move bestMove, // this may not actually be the best move, but the first move wich was good enough to cause a beta cut
//         short score,
//         byte depth,
//         sbyte type // -1: upper bound (ie real score may be worse), 0: exact: 1: lower bound
//     );
//
//     // TODO: Use a tuple instead of a struct? Should save some tokens
//     // max heap usage is 256mb, so use 2^23 entries, which should consume 134mb for sizeof(TTEntry) == 16
//     private TTEntry[] transpositionTable = new TTEntry[8_388_608];
//
//     // For each depth, store 2 killer moves: A killer move is a non-capturing move that caused a beta cutoff
//     // (ie early return due to score >= beta)  in a previous node. This is then used for move ordering
//     private Move[] killerMoves = new Move[1024]; // nmp increases ply by 20 to prevent overwriting useful data, so accomodate for that
//
//     // set by the toplevel call to negamax()
//     // returning (Move, int) from negamax() would be prettier but use more tokens
//     private Move bestRootMove;
//
//     private Timer timer;
//
//     // they are class members so that we can access their value outside of evaluate(), although this 
//     // depends on evaluate being called first so that they are set correctly
//     private int mg, eg, phase;
//
// #if PRINT_DEBUG_INFO
//     long allNodeCtr;
//     long nonQuiescentNodeCtr;
//     long betaCutoffCtr;
//     // node where remainingDepth is at least 2, so move ordering actually matters
//     // (it also matters for quiescent nodes but it's difficult to count non-leaf quiescent nodes and they don't use the TT, would skew results)
//     long parentOfInnerNodeCtr;
//     long parentOfInnerNodeBetaCutoffCtr;
//     int numTTEntries;
//     int numTTCollisions;
//     int numTranspositions;
//     int numTTWrites;
//
//     void printPv(int depth)
//     {
//         if (depth <= 0) return;
//         var ttEntry = transpositionTable[b.ZobristKey & 8_388_607];
//         if (ttEntry.key != b.ZobristKey)
//         {
//             Console.WriteLine("Position not in TT!");
//             return;
//         }
//         var move = ttEntry.bestMove;
//         if (!b.GetLegalMoves().Contains(move))
//         {
//             Console.WriteLine("Zobrist hash collision detected!");
//             return;
//         }
//         Console.WriteLine(move.ToString() + ", score " + ttEntry.score + ", type " + ttEntry.type + ", depth " + ttEntry.depth);
//         b.MakeMove(move);
//         printPv(depth - 1);
//         b.UndoMove(move);
//     }
// #endif
//
//     #region compresto
//
//     private static ulong[] compresto =
//     {
//         2531906049332683555, 1748981496244382085, 1097852895337720349, 879379754340921365, 733287618436800776,
//         1676506906360749833, 957361353080644096, 2531906049332683555, 1400370699429487872, 7891921272903718197,
//         12306085787436563023, 10705271422119415669, 8544333011004326513, 7968995920879187303, 7741846628066281825,
//         7452158230270339349, 5357357457767159349, 2550318802336244280, 5798248685363885890, 5789790151167530830,
//         6222952639246589772, 6657566409878495570, 6013263560801673558, 4407693923506736945, 8243364706457710951,
//         8314078770487191394, 6306293301333023298, 3692787177354050607, 3480508800547106083, 2756844305966902810,
//         18386335130924827, 3252248017965169204, 6871752429727068694, 7516062622759586586, 7737582523311005989,
//         3688521973121554199, 3401675877915367465, 3981239439281566756, 3688238338080057871, 5375663681380401,
//         5639385282757351424, 2601740525735067742, 3123043126030326072, 2104069582342139184, 1017836687573008400,
//         2752300895699678003, 5281087483624900674, 5717642197576017202, 578721382704613384, 14100080608108000698,
//         6654698745744944230, 1808489945494790184, 507499387321389333, 1973657882726156, 74881230395412501,
//         578721382704613384, 10212557253393705, 3407899295075687242, 4201957831109070667, 5866904407588300370,
//         5865785079031356753, 5570777287267344460, 3984647049929379641, 2535897457754910790, 219007409309353485,
//         943238143453304595, 2241421631242834717, 2098155335031661592, 1303832920857255445, 870353785759930383,
//         3397624511334669, 726780562173596164, 1809356472696839713, 1665231324524388639, 1229220018493528859,
//         1590638277979871000, 651911504053672215, 291616928119591952, 1227524515678129678, 6763160767239691,
//         4554615069702439202, 3119099418927382298, 3764532488529260823, 5720789117110010158, 4778967136330467097,
//         3473748882448060443, 794625965904696341, 150601370378243850, 4129336036406339328, 6152322103641660222,
//         6302355975661771604, 5576700317533364290, 4563097935526446648, 4706642459836630839, 4126790774883761967,
//         2247925333337909269, 17213489408, 6352120424995714304, 982348882
//     };
//
//     private byte[] pesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();
//
//     #endregion
//
//     bool stopThinking() // TODO: Can we save tokens by using properties instead of methods?
//     {
//         // The / 32 makes the most sense when the game last for another 32 moves. Currently, poor bot performance causes unnecesarily
//         // long games in winning positions but as we improve our engine we should see less time trouble overall.
//         return timer.MillisecondsElapsedThisTurn > Math.Min(timer.GameStartTimeMilliseconds / 64, timer.MillisecondsRemaining / 32);
//     }
//
//
//     public Move Think(Board board, Timer theTimer)
//     {
//         // Ideas to try out: Better positional eval (with better compressed psqts, passed pawns bit masks from Bits.cs, king safety by (semi) open file, ...),
//         // removing both b.IsDraw() and b.IsInCheckmate() from the leaf code path to avoid calling GetLegalMoves() (but first update to 1.18),
//         // updating a materialDif variable instead of recalculating that from scratch in every eval() call,
//         // null move pruning, reverse futility pruning, recapture extension (by 1 ply, no need to limit number of extensions: Extend when prev moved captured same square),
//         // depth replacement strategy for the TT (maybe by adding board.PlyCount (which is the actual position's ply count) so older entries get replaced),
//         // using a simple form of cuckoo hashing for the TT where we take the lowest or highest bits of the zobrist key as index, maybe combined with
//         // different replacement strategies (depth vs always overwrite) for both
//         // Also, the NPS metric is really low. Since almost all nodes are due to quiescent search (as it should), that's where the optimization potential lies.
//         // Also, apparently LINQ is really slow for no good reason, so if we can spare the tokens we may want to use a manual for loop :(
//         b = board;
//         timer = theTimer;
//         
//         var moves = board.GetLegalMoves();
//         // iterative deepening using the transposition table for move ordering; without the bound on depth, it could exceed
//         // 256 in case of forced checkmates, but that would overflow the TT entry and could potentially create problems
//         for (int depth = 1; depth++ < 50 && !stopThinking(); ) // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
// #if PRINT_DEBUG_INFO
//         { // comment out `&& !stopThinking()` to save some tokens at the cost of slightly less readable debug output
//             int score = negamax(depth, -30_000, 30_000, 0, false);
//
//             Console.WriteLine("Score: " + score + ", best move: " + bestRootMove.ToString());
//             var ttEntry = transpositionTable[board.ZobristKey & 8_388_607];
//             Console.WriteLine("Current TT entry: " + ttEntry.ToString());
//             // might fail if there is a zobrist hash collision (not just a table collision!) between the current position and a non-quiescent
//             // position reached during this position's eval, and the time is up. But that's very unlikely, so the assertion stays.
//             Debug.Assert(ttEntry.bestMove == bestRootMove || ttEntry.key != b.ZobristKey);
//             Debug.Assert(ttEntry.score != 12345); // the canary value from cancelled searches, would require +5 queens to be computed normally
//         }
// #else
//                     negamax(depth, -30_000, 30_000, 0, false);
// #endif
//
// #if PRINT_DEBUG_INFO
//         Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " + betaCutoffCtr
//             + ", percent cutting (higher is better): " + (100.0 * betaCutoffCtr / allNodeCtr).ToString("0.0")
//             + ", percent cutting for parents of inner nodes: " + (100.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("0.0")
//             + ", TT occupancy in percent: " + (100.0 * numTTEntries / transpositionTable.Length).ToString("0.0")
//             + ", TT collisions: " + numTTCollisions + ", num transpositions: " + numTranspositions + ", num TT writes: " + numTTWrites);
//         Console.WriteLine("NPS: {0}k", (allNodeCtr / (double)timer.MillisecondsElapsedThisTurn).ToString("0.0"));
//         Console.WriteLine("Time:{0} of {1} ms, remaining {2}", timer.MillisecondsElapsedThisTurn, timer.GameStartTimeMilliseconds, timer.MillisecondsRemaining);
//         Console.WriteLine("PV: ");
//         printPv(8);
//         Console.WriteLine();
//         allNodeCtr = 0;
//         nonQuiescentNodeCtr = 0;
//         betaCutoffCtr = 0;
//         parentOfInnerNodeCtr = 0;
//         parentOfInnerNodeBetaCutoffCtr = 0;
//         numTTCollisions = 0;
//         numTranspositions = 0;
//         numTTWrites = 0;
// #endif
//
//         return bestRootMove;
//     }
//
//     // return the score from the point of view of the player who can move now,
//     // ie a high value means that the current player likes their position.
//     // also sets bestRootMove if ply is zero (assuming remainingDepth > 0 and at least one legal move)
//     // searched depth may be larger than minRemainingDepth due to move extensions and quiescent search
//     // This function can deal with fail soft values from fail high scenarios but not from fail low ones.
//     int negamax(int remainingDepth, int alpha, int beta, int ply, bool allowNMP) // TODO: store remainingDepth and ply, maybe alpha and beta, as class members to save tokens
//     {
// #if PRINT_DEBUG_INFO
//         ++allNodeCtr;
//         if (remainingDepth > 0) ++nonQuiescentNodeCtr;
//         if (remainingDepth > 1) ++parentOfInnerNodeCtr;
// #endif
//
//         if (b.IsInCheckmate()) // TODO: Avoid (indirectly) calling GetLegalMoves in leafs, which is very slow apparently
//             return ply - 30_000; // being checkmated later is better (as is checkmating earlier); save
//         if (b.IsDraw())
//             // time-based contempt(tm): if we have more time than our opponent, try to cause timeouts.
//             // This really isn't the best way to calculate the contempt factor but it's relatively token efficient.
//             // Disabled for now because it doesn't seem to improve elo beyond the margin of doubt at this point, maybe a better implementation could?
//             // return (timer.MillisecondsRemaining - timer.OpponentMillisecondsRemaining) * (ply % 2 * 200 - 100) / timer.GameStartTimeMilliseconds;
//             return 0;
//
//         bool isRoot = ply == 0;
//         int killerIdx = ply * 2;
//         bool quiescent = remainingDepth <= 0;
//         var legalMoves = b.GetLegalMoves(quiescent).OrderByDescending(move =>
//             // order promotions and captures first (sorted by the value of the captured piece and then the captured, aka MVV/LVA),
//             // then killer moves, then normal ("quiet" non-killer) moves
//             move.IsPromotion ? (int)move.PromotionPieceType * 100 : move.IsCapture ? (int)move.CapturePieceType * 100 - (int)move.MovePieceType :
//                 move == killerMoves[killerIdx] || move == killerMoves[killerIdx + 1] ? -1000 : -1001);
//         if (!legalMoves.Any()) return eval(); // can only happen in quiescent search at the moment
//
//
//         int bestScore = -32_000;
//         int originalAlpha = alpha;
//         ulong ttIdx = b.ZobristKey & 8_388_607;
//         bool maybePvNode = alpha + 1 < beta;
//         if (quiescent)
//         {
//             // updating bestScore not only saves tokens compared to using a new standPat variable, it also increases playing strength by 100 elo compared to not
//             // updating bestScore. This is because the standPat score is often a more reliable estimate than forcibly taking a possible capture, so it should be
//             // returned if all captures fail low.
//             bestScore = eval(); // TODO: Instead of introducing new named variables, use an `int temp` member that can be used for these things to save tokens.
//             if (bestScore >= beta) return bestScore;
//             // delta pruning, a version of futility pruning: If the current position is hopeless, abort
//             // technically, we should also check capturing promotions, but they are rare so we don't care
//             // TODO: At some point in the future, test against this to see if the token saving potential is worth it
//             if (bestScore + 1150 < alpha) return alpha;
//             // The following is the "correct" way of doing it, but may not be stronger now (retest,also tune parameters)
//             // This doesn't work at all any more for psqt adjusted piece values
//             // if (bestScore + mgPieceValues[legalMoves.Select(move => (int)move.CapturePieceType).Max()] + 300 < alpha) return bestScore; // TODO: This only has a very small effect
//             alpha = Math.Max(alpha, bestScore); // TODO: If statement should use fewer tokens
//         }
//         else // TODO: Also do a table lookup during quiescent search? Test performance and tokens
//         {
//             var lookupVal = transpositionTable[ttIdx];
//             // reorder moves: First, we try the entry from the transposition table, then captures, then the rest
//             if (lookupVal.key == b.ZobristKey)
//                 // TODO: There's some token saving potential here
//                 // don't do TT cuts in pv nodes (which includes the root) because tt entries can sometimes be wrong
//                 // because chess isn't markovian (eg 3 fold repetition)
//                 // the Math.Abs(lookupVal.score) < 29_000 is a crude (but working) way to not do TT cutoffs for mate scores, TODO: Test if necessary (probably not)
//                 if (lookupVal.depth >= remainingDepth && Math.Abs(lookupVal.score) < 29_000 && !maybePvNode
//                     && (lookupVal.type == 0 || lookupVal.type < 0 && lookupVal.score <= alpha || lookupVal.type > 0 && lookupVal.score >= beta))
//                     return lookupVal.score;
//                 else // search the most promising move (as determined by previous searches) first, which creates great alpha beta bounds
//                     legalMoves = legalMoves.OrderByDescending(move => move == lookupVal.bestMove); // stable sorting, also works in case of a zobrist hash collision
//         }
//         // nmp, TODO: tune R, actually make sure phase is correct instead of possibly from a sibling or parent (which shouldn't hurt much but is still incorrect)
//         if (allowNMP && !maybePvNode && remainingDepth > 3 && phase > 0 && b.TrySkipTurn())
//         {
//             // if we have pvs, we can use pvs here as well (but for now it stays normal negamax)
//             // we can't reuse killerIdx because C# basically captures by reference, so we need to create a new variable
//             // increase ply by 20 to prevent clashes with normal search for killer moves
//             int nmpScore = -negamax(remainingDepth - 3, -beta, -alpha, ply + 20, false);
//             b.UndoSkipTurn();
//             if (nmpScore > beta) return nmpScore;
//             alpha = Math.Max(alpha, nmpScore);
//             bestScore = Math.Max(bestScore, nmpScore); // TODO: Optimize tokens by folding into nmpScore declaration
//         }
//
//         // fail soft: Instead of only updating alpha, maintain an additional bestScore variable. This might be useful later on
//         // but also means the TT entries can have lower upper bounds, potentially leading to a few more cuts
//         Move localBestMove = legalMoves.First();
//         foreach (var move in legalMoves)
//         {
//             // check extension: extend depth by 1 (ie don't reduce by 1) for checks -- this has no effect for the quiescent search
//             // However, this causes subsequent TT entries to have the same depth as their ancestors, which seems like it might lead to bugs
//             int newDepth = remainingDepth - (b.IsInCheck() ? 0 : 1);
//             // pvs: If we already increased alpha (which should happen in the first node), do a zero window search to confirm the best move so far is
//             // actually the best move in this position (which is more likely to be true the better move ordering works), zws should fail faster so use less time
//             // TODO: Maybe actually check for not being the first child instead of bestScore > alpha since that doesn't work for all-nodes (ie fail-low nodes)?
//             bool doPvs = bestScore > originalAlpha && !quiescent; // TODO: Also do in qsearch? Probably not (cause no tt for qsearch atm) but meassure
//             b.MakeMove(move);
//             // lmr: If not in qsearch and not the first node (aka doPvs), reduce by one unless in a non-quiet position
//             int score = -negamax(newDepth - (doPvs && !move.IsCapture && !b.IsInCheck() ? 1 : 0), doPvs ? -alpha - 1 : -beta, -alpha, ply + 1, true);
//             if (alpha < score && score < beta && doPvs)
//                 // zero window search failed, so research with full window
//                 score = -negamax(newDepth, -beta, -alpha, ply + 1, true);
//             b.UndoMove(move);
//
//             // testing this only in the Think function introduces too much variance into the time needed to calculate a move
//             if (stopThinking()) return 12345; // the value won't be used, so use a canary to detect bugs
//
//             if (score > bestScore)
//             {
//                 bestScore = score;
//                 localBestMove = move;
//                 alpha = Math.Max(alpha, score);
//                 if (score >= beta)
//                 {
// #if PRINT_DEBUG_INFO
//                     ++betaCutoffCtr;
//                     if (remainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
// #endif
//                     // TODO: This quiet move detection considers promotions to be quiet, but the mvvlva code doesn't.
//                     // Use this definition also for the move ordering code?
//                     if (!move.IsCapture && move != killerMoves[killerIdx])  // TODO: enabling the move != killerMoves[killerIdx] check looses like 30 elo ?!?
//                     {
//                         killerMoves[killerIdx + 1] = killerMoves[killerIdx];
//                         killerMoves[killerIdx] = move;
//                     }
//                     break;
//                 }
//             }
//         }
// #if PRINT_DEBUG_INFO
//         if (!quiescent) // don't fold into actual !quiescent test because then we'd need {}, adding an extra token
//         {
//             ++numTTWrites;
//             if (transpositionTable[ttIdx].key == 0) ++numTTEntries; // this counter doesn't get reset every move
//             else if (transpositionTable[ttIdx].key == b.ZobristKey) ++numTranspositions;
//             else ++numTTCollisions;
//         }
// #endif
//         if (!quiescent)
//             // always overwrite on hash table collisions (pure hash collisions should be pretty rare, but hash table collision frequent once the table is full)
//             // this removes old entries that we don't care about any more at the cost of potentially throwing out useful high-depth results in favor of much
//             // more frequent low-depth results, but doesn't use too many tokens
//             // A problem of this approach is that the more nodes it searches (for example, on longer time controls), the less likely it is that useful high-depth
//             // positions remain in the table, althoug it seems to work fine for usual 1 minute games
//             transpositionTable[ttIdx]
//                 = new(b.ZobristKey, localBestMove, (short)bestScore, (byte)remainingDepth, (sbyte)(bestScore <= originalAlpha ? -1 : bestScore >= beta ? 1 : 0));
//
//         if (isRoot) bestRootMove = localBestMove;
//         return bestScore;
//     }
//
//
//     // for the time being, this is very closely based on JW's example bot (ie tier 2 bot)
//     int eval()
//     {
//         phase = mg = eg = 0;
//         foreach (bool stm in new[] { true, false })
//         {
//             for (var p = PieceType.None; ++p <= PieceType.King; )
//             {
//                 int piece = (int)p - 1, ind;
//                 ulong mask = b.GetPieceBitboard(p, stm);
//                 while (mask != 0)
//                 {
//                     phase += pesto[768 + piece];
//                     ind = 64 * piece + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
//                     // The (47 << piece) trick doesn't really save all that much at the moment, but...
//                     // ...TODO: By storing mg tables first, then eg tables, this code can be reused for mg and eg calculation, potentially saving a few tokens
//                     mg += pesto[ind] + (47 << piece) + pesto[piece + 776];
//                     eg += pesto[ind + 384] + (47 << piece) + pesto[piece + 782];
//                 }
//             }
//
//             mg = -mg;
//             eg = -eg;
//         }
//
//         return (mg * phase + eg * (24 - phase)) / (b.IsWhiteToMove ? 24 : -24);
//         // int res = (mg * phase + eg * (24 - phase)) / 24 * (b.IsWhiteToMove ? 1 : -1);
//         // int expected = Pesto.originalPestoEval(b);
//         // if (res != expected)
//         // {
//         //     Console.WriteLine("eval was {0}, should be {1}", res, expected);
//         //     Debug.Assert(false);
//         // }
//         // return res;
//     }
        
//         //JW's example bot, aka tier 2 bot
//         Move bestmoveRoot = Move.NullMove;
//
//         // https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
//         int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };
//         int[] piecePhase = { 0, 0, 1, 1, 2, 4, 0 };
//         ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421, 366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514, 329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181, 402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047, 347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492, 347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863, 419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691, 383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };
//
//         #if PRINT_DEBUG_INFO
//         private int nodeCtr;
//         #endif
//         
//         // https://www.chessprogramming.org/Transposition_Table
//         struct TTEntry
//         {
//             public ulong key;
//             public Move move;
//             public int depth, score, bound;
//             public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound)
//             {
//                 key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
//             }
//         }
//
//         const int entries = (1 << 20);
//         TTEntry[] tt = new TTEntry[entries];
//
//         public int getPstVal(int psq)
//         {
//             return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
//         }
//
//         public int Evaluate(Board board)
//         {
//             int mg = 0, eg = 0, phase = 0;
//
//             foreach (bool stm in new[] { true, false })
//             {
//                 for (var p = PieceType.Pawn; p <= PieceType.King; p++)
//                 {
//                     int piece = (int)p, ind;
//                     ulong mask = board.GetPieceBitboard(p, stm);
//                     while (mask != 0)
//                     {
//                         phase += piecePhase[piece];
//                         ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);
//                         mg += getPstVal(ind) + pieceVal[piece];
//                         eg += getPstVal(ind + 64) + pieceVal[piece];
//                     }
//                 }
//
//                 mg = -mg;
//                 eg = -eg;
//             }
//
//             return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
//         }
//
//         // https://www.chessprogramming.org/Negamax
//         // https://www.chessprogramming.org/Quiescence_Search
//         public int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply)
//         {
//             ulong key = board.ZobristKey;
//             bool qsearch = depth <= 0;
//             bool notRoot = ply > 0;
//             int best = -30000;
//
// #if PRINT_DEBUG_INFO
//             ++nodeCtr;
// #endif
//             
//             // Check for repetition (this is much more important than material and 50 move rule draws)
//             if (notRoot && board.IsRepeatedPosition())
//                 return 0;
//
//             TTEntry entry = tt[key % entries];
//
//             // TT cutoffs
//             if (notRoot && entry.key == key && entry.depth >= depth && (
//                 entry.bound == 3 // exact score
//                     || entry.bound == 2 && entry.score >= beta // lower bound, fail high
//                     || entry.bound == 1 && entry.score <= alpha // upper bound, fail low
//             )) return entry.score;
//
//             int eval = Evaluate(board);
//
//             // Quiescence search is in the same function as negamax to save tokens
//             if (qsearch)
//             {
//                 best = eval;
//                 if (best >= beta) return best;
//                 alpha = Math.Max(alpha, best);
//             }
//
//             // Generate moves, only captures in qsearch
//             Move[] moves = board.GetLegalMoves(qsearch);
//             int[] scores = new int[moves.Length];
//
//             // Score moves
//             for (int i = 0; i < moves.Length; i++)
//             {
//                 Move move = moves[i];
//                 // TT move
//                 if (move == entry.move) scores[i] = 1000000;
//                 // https://www.chessprogramming.org/MVV-LVA
//                 else if (move.IsCapture) scores[i] = 100 * (int)move.CapturePieceType - (int)move.MovePieceType;
//             }
//
//             Move bestMove = Move.NullMove;
//             int origAlpha = alpha;
//
//             // Search moves
//             for (int i = 0; i < moves.Length; i++)
//             {
//                 if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 30000;
//
//                 // Incrementally sort moves
//                 for (int j = i + 1; j < moves.Length; j++)
//                 {
//                     if (scores[j] > scores[i])
//                         (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
//                 }
//
//                 Move move = moves[i];
//                 board.MakeMove(move);
//                 int score = -Search(board, timer, -beta, -alpha, depth - 1, ply + 1);
//                 board.UndoMove(move);
//
//                 // New best move
//                 if (score > best)
//                 {
//                     best = score;
//                     bestMove = move;
//                     if (ply == 0) bestmoveRoot = move;
//
//                     // Improve alpha
//                     alpha = Math.Max(alpha, score);
//
//                     // Fail-high
//                     if (alpha >= beta) break;
//
//                 }
//             }
//
//             // (Check/Stale)mate
//             if (!qsearch && moves.Length == 0) return board.IsInCheck() ? -30000 + ply : 0;
//
//             // Did we fail high/low or get an exact score?
//             int bound = best >= beta ? 2 : best > origAlpha ? 3 : 1;
//
//             // Push to TT
//             tt[key % entries] = new TTEntry(key, bestMove, depth, best, bound);
//
//             return best;
//         }
//
//         public Move Think(Board board, Timer timer)
//         {
// #if PRINT_DEBUG_INFO
//             nodeCtr = 0;
// #endif
//             // https://www.chessprogramming.org/Iterative_Deepening
//             for (int depth = 1; depth <= 50; depth++)
//             {
//                 int score = Search(board, timer, -30000, 30000, depth, 0);
//
//                 // Out of time
//                 if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
//                     break;
//             }
// #if PRINT_DEBUG_INFO
//             Console.WriteLine("Evil Bot: Nodes: {0}, NPS: {1}k", nodeCtr, (nodeCtr / (double)timer.MillisecondsElapsedThisTurn).ToString("0.0"));
// #endif
//             return bestmoveRoot;
//         }
    }
}