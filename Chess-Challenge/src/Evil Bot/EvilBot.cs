#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
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

    Move[] killers = new Move[256];
        
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
    long awRetryCtr;
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

        int[,,] history = new int[2, 7, 64];
        
        // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
        for (int depth = 1, alpha = -30_000, beta = 30_000; depth < 64 && timer.MillisecondsElapsedThisTurn <= timer.MillisecondsRemaining / 32;)
        {
            // TODO: This should be bugged when out of time when the last score failed low on the asp window
            int score = negamax(depth, alpha, beta, 0, false);
            // excluding checkmate scores was inconclusive after 6000 games, so likely not worth the tokens
            if (score <= alpha) alpha += score - alpha;
            else if (score >= beta) beta += score - beta;
            else
            {
#if PRINT_DEBUG_INFO
                lastDepth = depth - 1;
                if (score != 12345) lastScore = score;
                Console.WriteLine("Depth {0}, score {1}, best {2}, nodes {3}k, time {4}, nps {5}k",
                    depth, lastScore, bestRootMove, Round(allNodeCtr / 1000.0), timer.MillisecondsElapsedThisTurn,
                    Round(allNodeCtr / (double)timer.MillisecondsElapsedThisTurn, 1));
#endif
                alpha = beta = score;
                ++depth;
            }

#if PRINT_DEBUG_INFO
            ++awRetryCtr;
#endif
            // tested values: 8, 15, 20, 30 (15 being the second best)
            alpha -= 20;
            beta += 20;
            // !! TODO: Bug in eval score! !!
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
        Console.WriteLine("PV: ");
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
            Console.WriteLine(move);
            board.MakeMove(move);
            printPv(remainingDepth - 1);
            board.UndoMove(move);
        }
    }
#endif

        return bestRootMove;


        int negamax(int remainingDepth, int alpha, int beta, int ply, bool allowNmp)
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

            int bestScore = -32_000,
                // originalAlpha = alpha,
                standPat = eval(),
                moveIdx = 0,
                killerIdx = 2 * ply,
                score;

            byte flag = 1;

            if (ply > 0 && board.IsRepeatedPosition())
                return 0;

            if (inQsearch)
            {
                bestScore = standPat;
                if (standPat >= beta) return standPat;
                if (alpha < standPat) alpha = standPat;
            }

            // Check Extensions
            if (inCheck) ++remainingDepth; // TODO: Do this before setting qsearch to disallow dropping to qsearch in check? Probably unimportant

            // TODO: Use tt for stand pat score?
            // TODO: Use tuple as TT entries
            ref TTEntry ttEntry = ref tt[board.ZobristKey & 0x7f_ffff];

            if (isNotPvNode && ttEntry.depth >= remainingDepth && ttEntry.key == board.ZobristKey)
            {
                if (ttEntry.flag == 0 && ttEntry.score >= beta
                ||  ttEntry.flag == 1 && ttEntry.score <= alpha
                ||  ttEntry.flag > 1) return ttEntry.score;
            }

            int search(int minusNewAlpha, int reduction = 1, bool allowNullMovePruning = true) =>
                score = -negamax(remainingDepth - reduction, -minusNewAlpha, -alpha, ply + 1, allowNullMovePruning);

            if (canPrune)
            {
                // Reverse Futility Pruning (RFP)
                if (!inQsearch && remainingDepth < 5 && standPat >= beta + 64 * remainingDepth)
                    return standPat;

                // Null Move Pruning (NMP). TODO: Avoid zugzwang by testing phase?
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
            // killers[killerIdx + 2] = killers[killerIdx + 3] = default;

            // generate moves
            var legalMoves = board.GetLegalMoves(inQsearch);

            // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending
            var scores = new int[legalMoves.Length];
            foreach (Move move in legalMoves)
            {
                scores[moveIdx++] = -(move == ttEntry.bestMove ? 1_000_000_000 :
                    move.IsCapture ? (int)move.CapturePieceType * 1_048_576  - (int)move.MovePieceType :
                    // Giving the first killer a higher score doesn't seem to gain after 10k games
                    move == killers[killerIdx] || move == killers[killerIdx + 1] ? 1_000_000 :
                    history[ToInt32(board.IsWhiteToMove), (int)move.MovePieceType, move.TargetSquare.Index]);
            }

            Array.Sort(scores, legalMoves);

            Move localBestMove = default;
            moveIdx = 0;
            foreach (Move move in legalMoves)
            {
                // TODO: Futility Pruning (FP) / Late Move Pruning (LMP)
                // if (remainingDepth <= 5 && bestScore > -29_000 && canPrune
                //     && moveIdx > remainingDepth * remainingDepth + 4 && scores[moveIdx] < 1_000_000 /*|| standPat + 500 + 128 * remainingDepth < alpha*/) break;
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
                        moveIdx >= (isNotPvNode ? 4 : 6)
                                    && remainingDepth > 3
                                    && !move.IsCapture
                                    && !inCheck
                        ?
                        Clamp(3 + ToInt32(isNotPvNode), 1, remainingDepth - 1)
                        : 1
                        );
                    if (alpha < score && score < beta)
                        search(beta);
                }

                board.UndoMove(move);

                if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 32)
                    return 12345; // the value won't be used, so use a canary to detect bugs

                if (score > bestScore)
                {
                    localBestMove = move;
                    bestScore = score;
                    if (score >= alpha)
                    {
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
                                if (move != killers[killerIdx]) // TODO: Test using only 1 killer move
                                {
                                    killers[killerIdx + 1] = killers[killerIdx];
                                    killers[killerIdx] = move;
                                }

                                // gravity didn't gain (TODO: Retest later when the engine is better), but history still gained quite a bit
                                history[ToInt32(board.IsWhiteToMove), (int)move.MovePieceType, move.TargetSquare.Index]
                                    += remainingDepth * remainingDepth;
                            }

                            flag = 0;

                            break;
                        }
                    }
                }
            }

            if (moveIdx == 0)
                return inQsearch ? bestScore : inCheck ? ply - 30_000 : 0; // being checkmated later is better (as is checkmating earlier)

            if (ply == 0) bestRootMove = localBestMove;
            // not updating the tt move in qsearch gives close to 20 elo (with close to 20 elo error bounds, but measured two times with 1000 games each)
            // TODO: Retest with proper SPRT
            ttEntry = new(board.ZobristKey, localBestMove, (short)bestScore,
                flag, (sbyte)remainingDepth);

            return bestScore;
        }


        // Eval loosely based on JW's example bot (ie tier 2 bot)
        int eval()
        {
            bool ourColor = board.IsWhiteToMove;
            int phase = 0, mg = 7, eg = 7;
            foreach (bool isWhite in new[] { ourColor, !ourColor })
            {
                for (int piece = 6; --piece >= 0;)
                {
                    ulong mask = board.GetPieceBitboard((PieceType)piece + 1, isWhite);
                    while (mask != 0)
                    {
                        phase += pesto[768 + piece];
                        int psqtIndex = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^
                                        56 * ToInt32(isWhite) + 64 * piece;
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
    }
}