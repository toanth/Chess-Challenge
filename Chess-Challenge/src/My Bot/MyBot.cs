#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
#define GUI_INFO
#endif

// #define PRINT_DEBUG_INFO
// #define GUI_INFO
// #define NO_JOKE

using System;
using static System.Math;
using static System.Convert;
using System.Linq;
using ChessChallenge.API;


// King Gᴀᴍʙᴏᴛ, A Joke Bot

public class MyBot : IChessBot
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

    
    record struct TTEntry (
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
        for (int depth = 1, alpha = -30_000, beta = 30_000; depth < 64 && timer.MillisecondsElapsedThisTurn <= timer.MillisecondsRemaining / 64;)
        {
            int score = negamax(depth, alpha, beta, 0, false);
            // TODO: Test returning here in case of a soft timeout?
            // excluding checkmate scores was inconclusive after 6000 games, so likely not worth the tokens
            if (score <= alpha) alpha = score;
            else if (score >= beta) {
                beta = score;
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
                allowPruning = isNotPvNode && !inCheck;
            stmColor = board.IsWhiteToMove;

            int bestScore = -32_000,
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

            if (allowPruning)
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
                // if (remainingDepth <= 5 && bestScore > -29_000 && allowPruning
                //     && moveIdx > remainingDepth * remainingDepth + 4 && scores[moveIdx] < 1_000_000 /*|| standPat + 500 + 128 * remainingDepth < alpha*/) break;
                // Futility Pruning (FP) and Late Move Pruning (LMP). Would probably benefit from more tuning
                if (remainingDepth <= 5 && bestScore > -29_000 && allowPruning
                    && (scores[moveIdx] > -1_000_000 && standPat + 300 + 64 * remainingDepth < alpha || moveIdx > 7 + remainingDepth * remainingDepth)) break;
                board.MakeMove(move);
                // pvs like this is -7 +- 20 elo after 1000 games; adding inQsearch || ... doesn't change that, nor does move == ttMove
                if (moveIdx++ == 0)
                    search(beta);
                else
                {
                    // Late Move Reductions (LMR), needs further parameter tuning. `reduction` is R + 1 to save tokens
                    search(alpha + 1, 
                        moveIdx < (isNotPvNode ? 3 : 4)
                        || remainingDepth <= 3
                        || move.IsCapture
                        // || inCheck // the inCheck condition doesn't seem to gain, failed a [0,10] SPRT with +1.6 after 5.7k games
                            ? 1 
                            // reduction values originally from the Viridithas engine, which seem pretty widely used by now
                            // TODO: Precompute and compress the individual Log values (should take up at most 4 bits each)? Probably far too token-hungry
                            : Clamp((int)(0.77 + Log(remainingDepth) * Log(moveIdx) / 2.36) + 1 - ToInt32(isNotPvNode), 1, remainingDepth - 1)
                    );
                    if (alpha < score && score < beta)
                        search(beta);
                }

                board.UndoMove(move);

                if (timer.MillisecondsElapsedThisTurn * 16 > timer.MillisecondsRemaining) // TODO: Test with 16
                    return 12345; // the value won't be used, so use a canary to detect bugs

                bestScore = Max(score, bestScore);
                if (score <= alpha) continue;
                
                localBestMove = move;
                if (halfPly == 0) bestRootMove = localBestMove; // updating here (instead of at the end) together with the aw fix is better, now testing without the aw fix
                alpha = score;
                ++flag;
                if (score < beta) continue;
                
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
                    // TODO: Test reducing the history score for moves that don't raise alpha
                    history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]
                        += remainingDepth * remainingDepth;
                }

                flag = 0;

                break;
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
            int phase = 0, mg = 7, eg = 7;
            foreach (bool isWhite in new[] { stmColor, !stmColor })
            {
                for (int piece = 6; --piece >= 0;)
                    for (ulong mask = board.GetPieceBitboard((PieceType)piece + 1, isWhite); mask != 0; )
                    {
                        phase += pesto[768 + piece];
                        int psqtIndex = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^
                                        56 * ToInt32(isWhite) + 64 * piece;
                        // TODO: Use pesto[^piece] for endgame piece values (requires reversing the order of eg piece values)
                        mg += pesto[psqtIndex] + (47 << piece) + pesto[piece + 776];
                        eg += pesto[psqtIndex + 384] + (47 << piece) + pesto[piece + 782];
                    }
                mg = -mg;
                eg = -eg;
            }
            return (mg * phase + eg * (24 - phase)) / 24;
        }
    }
}