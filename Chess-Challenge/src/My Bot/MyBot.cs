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


public class MyBot : IChessBot
{

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


    private Board board;

    private Timer timer;

    private Move bestRootMove;

#if PRINT_DEBUG_INFO
    long allNodeCtr;
    long nonQuiescentNodeCtr;
    long betaCutoffCtr;
    // node where remainingDepth is at least 2, so move ordering actually matters
    // (it also matters for quiescent nodes but it's difficult to count non-leaf quiescent nodes and they don't use the TT, would skew results)
    long parentOfInnerNodeCtr;
    long parentOfInnerNodeBetaCutoffCtr;
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

        // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
        for (int depth = 1; depth++ < 50 && !shouldStopThinking();)
        {
            int score = negamax(depth, -30_000, 30_000, 0);
#if PRINT_DEBUG_INFO
            Console.WriteLine("Depth {0}, score {1}, best move {2}", depth, score, bestRootMove);
#endif
        }

#if PRINT_DEBUG_INFO
        Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " + betaCutoffCtr
            + ", percent cutting (higher is better): " + (100.0 * betaCutoffCtr / allNodeCtr).ToString("0.0")
            + ", percent cutting for parents of inner nodes: " + (100.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("0.0"));
        Console.WriteLine("NPS: {0}k", (allNodeCtr / (double)timer.MillisecondsElapsedThisTurn).ToString("0.0"));
        Console.WriteLine("Time:{0} of {1} ms, remaining {2}", timer.MillisecondsElapsedThisTurn, timer.GameStartTimeMilliseconds, timer.MillisecondsRemaining);
        Console.WriteLine("PV: ");
        Console.WriteLine();
        allNodeCtr = 0;
        nonQuiescentNodeCtr = 0;
        betaCutoffCtr = 0;
        parentOfInnerNodeCtr = 0;
        parentOfInnerNodeBetaCutoffCtr = 0;
#endif

        return bestRootMove;
    }


    int negamax(int remainingDepth, int alpha, int beta, int ply)
    {
#if PRINT_DEBUG_INFO
        ++allNodeCtr;
        if (remainingDepth > 0) ++nonQuiescentNodeCtr;
        if (remainingDepth > 1) ++parentOfInnerNodeCtr;
#endif

        if (board.IsInCheckmate()) // TODO: Avoid (indirectly) calling GetLegalMoves in leafs, which is very slow apparently
            return ply - 30_000; // being checkmated later is better (as is checkmating earlier); save
        if (board.IsDraw())
            return 0;

        bool isRoot = ply == 0,
            inQsearch = remainingDepth <= 0;
        int bestScore = -32_000,
            originalAlpha = alpha,
            standPat = eval();

        if (inQsearch)
        {
            bestScore = standPat;
            if (standPat >= beta) return standPat;
            if (alpha < standPat) alpha = standPat;
        }

        // Using stackalloc doesn't gain elo
        var legalMoves = board.GetLegalMoves(inQsearch);
        int numMoves = legalMoves.Length;

        if (numMoves == 0)
            return standPat;
        // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending
        var scores = new int[numMoves]; // TODO: Make static/ member to avoid potential SO?
        for (int i = 0; i < numMoves; i++)
        {
            Move move = legalMoves[i];
            scores[i] = move.IsCapture ? (int)move.MovePieceType - (int)move.CapturePieceType * 100 : 0;
        }
        Array.Sort(scores, legalMoves);


        Move localBestMove = Move.NullMove;
        foreach (var move in legalMoves)
        {
            int newDepth = remainingDepth - 1;
            board.MakeMove(move);
            int score = -negamax(newDepth, -beta, -alpha, ply + 1);
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
                    break;
                }
            }
        }

        if (isRoot) bestRootMove = localBestMove;

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
                int piece = (int)p - 1, square, index;
                ulong mask = board.GetPieceBitboard(p, stm);
                while (mask != 0)
                {
                    phase += pesto[768 + piece];
                    square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask);
                    index = square ^ (stm ? 56 : 0) + 64 * piece;
                    // The (47 << piece) trick doesn't really save all that much at the moment, but...
                    // ...TODO: By storing mg tables first, then eg tables, this code can be reused for mg and eg calculation, potentially saving a few tokens
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
}