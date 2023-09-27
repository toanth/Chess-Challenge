#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
#define GUI_INFO
#endif

using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // TODO: Not necessary except for decompression
using static System.Math;
using static System.Convert;

namespace ChessChallenge.Example
{

    public class EvilBot : IChessBot
    {
        
    // TODO: Likely bug with the scores? Output:
    // Depth 1, score 29990, best Move: 'c8c7', nodes 0k, time 0, nps ∞k
    // Depth 2, score 1816, best Move: 'c8c7', nodes 0k, time 0, nps ∞k
    // Depth 3, score 29994, best Move: 'c8c7', nodes 0k, time 0, nps ∞k

    // TODO: Use tuple for this, add more 200 token optimizations
    
    // record struct TTEntry (
    //     ulong key,
    //     Move bestMove,
    //     short score,
    //     byte flag, 
    //     sbyte depth
    // );

    // the tt move ordering alone gained almost 400 elo (and >100 elo for cutoffs)
    // flag: 1 = upper, > 1 = exact, 0 = lower
    // key, bestMove, score, flag, depth 
    private (ulong, Move, short, byte, sbyte)[] tt = new (ulong, Move, short, byte, sbyte)[0x80_0000];
    // private dynamic tt = new (ulong, Move, short, byte, sbyte)[0x80_0000];
        
    private Move bestRootMove, chosenMove;

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


    // The compression is basically the same as my public compression to ulongs,
    // except that I'm now compressing to decimals and I'm not using the PeSTO values anymore.
    // Instead, these are my own tuned values, which are different from my public tuned values
    // because they are specifically tuned for King Gambot's playstyle. // TODO: Actually do that
    // The values were tuned using Gedas' tuner: https://github.com/GediminasMasaitis/texel-tuner 
#if NO_JOKE
    // The engine plays normally
    private static decimal[] compresto = { 41259558725118300045770787m, 12148637347380221118036452643m, 17409920479543823259362417699m,
        17415950995801781949306850595m, 19582331915682275235129309987m, 27619270727875096341232753955m, 24204036247829851047561096995m,
        17716926000957032489238008611m, 19219649429467200750846752133m, 28239454489355429731532690857m, 27325506771411732918706423392m,
        28264832339681780230600893058m, 28285318131004420259134346599m, 34744636933779701502470634657m, 30108430193492018388713899333m,
        26358318502736173427450932504m, 26024674163127829595043286813m, 28222501119009740190685575722m, 30083038105128088690663203645m,
        27655505596085059564487753538m, 29200508052015652233113096804m, 36923093278145199429799706459m, 36594255899084194500476127036m, 
        26988140853361540657189005839m, 20481735021348528704700970261m, 29789250406994901518261915440m, 30410685558647888737259065641m, 
        31364471306583536207063724856m, 31069498221195796367646630714m, 33215336754067364878761837359m, 31069483850178722243284668468m, 
        23925955328855061363613013004m, 17361492703139014988150894856m, 21749903039957399227271709217m, 29476162306263244483910929950m, 
        30438448480538401518838909743m, 31347517973580801938501704244m, 30113200044799793211834798377m, 25786444850924756325208199981m, 
        19577387562017695046517028362m, 17054392377348271864303216393m, 21992873407402656559689463071m, 26376415020547519571355077151m, 
        29460403713898723701833173017m, 30082972231781025471371710758m, 27926224772350531264175771430m, 25132415906995777328306755396m, 
        20174573194104268789821238807m, 14571249260866825043314106624m, 19521800419985621626352912674m, 24155641624402136810920243727m, 
        26957941230043066728457599756m, 27267374386283860922325368084m, 24164061566224768722331471931m, 21362970886548660321026011209m, 
        17654024028026168252663294733m, 6511326489309019165282080035m, 12397638507713166148304333091m, 16428201656437991938157986851m, 
        19497645755591385527336394019m, 14282320806016632398299884835m, 18582432038614987650233688611m, 15502169531075520246737234723m, 
        9596453343092721579231897379m, 9596453343092634734267793664m, 18121138423700824047694607616m };

#else

    // Modified king middle game table to make the king lead the army (as he should)

    private static decimal[] compresto =
        //{ 41259558725125996627165219m, 12148637347380132057594602787m, 17409920479543741895501962275m, // King on the Hill
        //17415950995801734670306856227m, 19582331915682273036106054435m, 27619270727875069952953687331m, 24204036247829785076863430435m,
        //17716926000956954423912436515m, 19219649429467103993823507845m, 28239454489355386850579207593m, 27325506771411738416264562272m,
        //28264832339681771434507870850m, 28285318131004412562552952167m, 34744636933779689407842729121m, 30108430193492016189690643781m,
        //26358318502736140442102099224m, 26024674163127773519950270237m, 28222501119009669821941398058m, 30083038105128108481872503613m,
        //27655505596085170615162158914m, 29200508052015767681834013284m, 36923093278145214822962495323m, 36594255899084126330755204924m,
        //26988140853361498875747150351m, 20481735021348480326189348117m, 29789250406994879528029359920m, 30410685558647940414305571113m,
        //31364471306583714327947424568m, 31069498221195977787065213754m, 33215336754067430849459503919m, 31069483850178693655982346292m,
        //23925955328855033875822318604m, 17361492703139000694499733768m, 21749903039957356346318225953m, 29476162306263312653631852062m,
        //30438448480538592833862142767m, 31347517973581000950106331700m, 30113200044799880073253392681m, 25786444850924748628626805549m,
        //19577387562017682951889122826m, 17054392377348217988233455369m, 21992873407402627972387140895m, 26376415020547554755727165983m,
        //29460403713898812762275022873m, 30082972231781112332790305062m, 27926224772350575244640882470m, 25132415906995749840516060996m,
        //20174573194104229207402638871m, 14571249260866753575058301184m, 19521800419985569949306407202m, 24155641624402129114338849295m,
        //26957941230043120604527360780m, 27267374386283891708650945812m, 24164061566224769821843099707m, 21362970886548606444956250185m,
        //17654024028026089087826094861m, 6511326489308964189700691235m, 12397638507713055097629927715m, 16428201656437916071855670307m,
        //19497645755591373432708488483m, 14282320806016552133951057187m, 18582432038614946968303460899m, 15502169531075431186295384867m,
        //9596453343092634717813303075m, 9596453343092634734267793664m, 18121138423700895515950413056m };
    { 41259558725398675510853667m, 12148637347380404736478291235m, 17409920479544014574385650723m, // Gᴀᴍʙᴏᴛ
        17415950995802007349190544675m, 19582331915682545714989742883m, 27619270727875342631837375779m, 24204036247830057755747118883m,
        17716926000957227102796124963m, 19219649429467377772218824069m, 28239454489355639738253596073m, 27325506771411963816148256352m,
        28264832339681996834391564930m, 28285318131004637962436646247m, 34744636933779914807726423201m, 30108430193492269077365032261m,
        26358318502736414220497415448m, 26024674163128048397857214237m, 28222501119009922709615786538m, 30083038105128295398849225533m,
        27655505596085286063883075394m, 29200508052015883130554929764m, 36923093278145401739939217243m, 36594255899084379218429593404m,
        26988140853361773753654094351m, 20481735021348750806049781013m, 29789250406995126918145609520m, 30410685558648105341049737513m,
        31364471306583769303528813368m, 31069498221196032762646602554m, 33215336754067595776203670319m, 31069483850178941046098595892m,
        23925955328855304355682751500m, 17361492703139217298290405640m, 21749903039957548760853086753m, 29476162306263422604794629662m,
        30438448480538592833862142767m, 31347517973581000950106331700m, 30113200044799990024416170281m, 25786444850924941043161666349m,
        19577387562017899555679794698m, 17054392377348325740372977417m, 21992873407402710435759224095m, 26376415020547582243517860383m,
        29460403713898812762275022873m, 30082972231781112332790305062m, 27926224772350602732431576870m, 25132415906995832303888144196m,
        20174573194104336959542160919m, 14571249260866763470662951168m, 19521800419985553456631990562m, 24155641624402129114338849295m,
        26957941230043120604527360780m, 27267374386283891708650945812m, 24164061566224769821843099707m, 21362970886548611942514389065m,
        17654024028026098983430744845m, 6511326489308964189700691235m, 12397638507713055097629927715m, 16428201656437907275762648099m,
        19497645755591376731243371811m, 14282320806016557631509196067m, 18582432038614949167326716451m, 15502169531075422390202362659m,
        9596453343092634717813303075m, 9596453343092634734267793664m, 18121138423700895515950413056m };

#endif

    // TODO: Inline compresto
    private byte[] pesto = compresto.SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes).ToArray();

    #endregion // compresto



    public Move Think(Board board, Timer timer)
    {
        // use longs for history scores to avoid overflow issues with high depths
        var history = new long[2, 7, 64];
        var killers = new Move[256];

        // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
        // 30_000 is used as infinity because it comfortably fits into 16 bits
        // History scores can only handle depths less than 63 (msb for positive longs), so use that as upper bound on the dpeth
        // (because of extensions, the actual depth can be larger, but that's not a problem since remainingDepth < 63 remains true)
        for (int depth = 1, alpha = -30_000, beta = 30_000; depth < 63 && timer.MillisecondsElapsedThisTurn <= timer.MillisecondsRemaining / 64;)
        {
            int score = negamax(depth, alpha, beta, 0, false);
            // TODO: Test returning here in case of a soft timeout?
            // excluding checkmate scores was inconclusive after 6000 games, so likely not worth the tokens
            if (score <= alpha) alpha = score; // don't update `chosenMove` on a fail-low. Gains surprisingly little elo, but it's still a gain.
            else if (score >= beta) {
                beta = score;
                chosenMove = bestRootMove; 
            }
            else
            {
#if PRINT_DEBUG_INFO
                Console.WriteLine("Depth {0}, score {1}, best {2}, nodes {3}k, time {4}ms, nps {5}k",
                    depth, score, bestRootMove, Round(allNodeCtr / 1000.0), timer.MillisecondsElapsedThisTurn,
                    Round(allNodeCtr / (double)timer.MillisecondsElapsedThisTurn, 1));
#endif
#if GUI_INFO
                lastDepth = depth;
                if (score != 12345) lastScore = score; // don't display the canary value of an unfinished search (the chosen move may still have been updated)
#endif
                alpha = beta = score; // reset window to be centered on the current score, will be widened shortly
                chosenMove = bestRootMove;
                ++depth;
            }

            // start the next iteration of ID or re-search with relaxed AW bounds.
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
        var (key, bestMove, score, flag, depth) = tt[board.ZobristKey & 0x7f_ffff];
        var move = bestMove;
        if (board.ZobristKey == key && board.GetLegalMoves().Contains(move) && remainingDepth > 0)
        {
            Console.Write(move + " ");
            board.MakeMove(move);
            printPv(remainingDepth - 1);
            board.UndoMove(move);
        }
    }
#endif

        return chosenMove; // The end. Now follow some methods that have been turned into local functions to save tokens

        // The recursive search function. Returns a score and also sets `bestRootMove`, which will eventually be returned by `Think`.
        // halfPly (ie quarter move) instead of ply to save tokens when accessing killers
        int negamax(int remainingDepth, int alpha, int beta, int halfPly, bool allowNmp)
        {
#if PRINT_DEBUG_INFO
            ++allNodeCtr;
            if (remainingDepth > 0) ++nonQuiescentNodeCtr;
            if (remainingDepth > 1) ++parentOfInnerNodeCtr;
            System.Diagnostics.Debug.Assert(alpha < beta); // spell out name to avoid having to guard the additional using directive with PRINT_DEBUG_INFO
            System.Diagnostics.Debug.Assert(alpha >= -32_000);
            System.Diagnostics.Debug.Assert(beta <= 32_000);
            System.Diagnostics.Debug.Assert(remainingDepth < 100);
            System.Diagnostics.Debug.Assert(halfPly >= 0 && halfPly % 2 == 0 && halfPly < 256);
#endif
            
            // In general, try to abort as early as possible to avoid unnecessary work
            // (TT probing will cause cache misses and IsInCheck() is also slow)
            if (board.IsRepeatedPosition()) // no need to check for halfPly > 0 here as the method already does essentially the same
                return 0;
            ulong ttIndex = board.ZobristKey & 0x7f_ffff;
            var (ttKey, ttMove, ttScore, ttFlag, ttDepth) = tt[ttIndex];

            // Using stackalloc doesn't gain elo
            bool isNotPvNode = alpha + 1 >= beta,
                inCheck = board.IsInCheck(),
                allowPruning = isNotPvNode && !inCheck,
                trustTTEntry = ttKey == board.ZobristKey,
                stmColor = board.IsWhiteToMove;

            
            // Eval. Currently psqt only with modified PeSTO values. Uses a lossless "compression" of 12 bytes into one decimal literal
            // Using a small "random" component (the last 4 bits of the zobrist hash) as tempo bonus to approximate mobility didn't gain :/
            int phase = 0, mg = 7, eg = 7;
            // int phase = 0, mg = (int)board.ZobristKey & 15, eg = mg; // TODO: Test, use ttEntry key?
            foreach (bool isWhite in new[] { stmColor, !stmColor })
            {
                for (int piece = 6; piece >= 1;)
                    for (ulong mask = board.GetPieceBitboard((PieceType)piece--, isWhite); mask != 0;)
                    {
                        phase += pesto[1024 + piece];
                        int psqtIndex = 16 * (BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^
                                              56 * ToInt32(isWhite)) + piece;
                        // The + (47 << piece) part is just a trick to encode piece values in one byte
                        mg += pesto[psqtIndex] + (47 << piece) + pesto[piece + 1040];
                        eg += pesto[psqtIndex + 6] + (47 << piece) + pesto[piece + 1046];
                    }
                mg = -mg;
                eg = -eg;
            }
            // return (mg * phase + eg * (24 - phase)) / 24; // if scores weren't stored as shorts in the TT, the / 24 would be unnecessary // TODO: Change TT size?
            
            int bestScore = -32_000,
                // using the TT score passed the SPRT with a 20 elo gain. This is a bit weird since this value is obviously
                // incorrect if the flag isn't exact, but since it gained elo over only trusting exact scores it stays like this
                // The reason behind this is probably that most of the time, original ab bounds weren't completely different from
                // the current ab bounds, so inexact scores are still closer to the truth than the static eval on average
                // (and using the static eval in non-quiet positions is a bad idea anyway), so this leads to better (nmp/rfp/fp) pruning
                // TODO: Use qsearch result as standPat score if not in qsearch?
                standPat = trustTTEntry ? ttScore : (mg * phase + eg * (24 - phase)) / 24, // Mate score when in check lost elo
                moveIdx = 0,
                childScore; // TODO: Merge standPat and childScore? Would make FP more difficult / token hungry

            byte flag = 1;
            
            int search(int minusNewAlpha, int reduction = 1, bool allowNullMovePruning = true) =>
                childScore = -negamax(remainingDepth - reduction, -minusNewAlpha, -alpha, halfPly + 2, allowNullMovePruning);

            // Check Extensions
            if (inCheck) ++remainingDepth;
            
            // Set the inQsearch flag after a possible checkExtension to avoid dropping into qsearch while in check (passing SPRT with +34(!) elo) 
            bool inQsearch = remainingDepth <= 0;
                
            if (inQsearch && (alpha = Max(alpha, bestScore = standPat)) >= beta) // the qsearch stand-pat check, token optimized
                return standPat;

            if (isNotPvNode && ttDepth >= remainingDepth && trustTTEntry
                    && ttFlag != 0 | ttScore >= beta // Token-efficient flag cut-off condition by Broxholme
                    && ttFlag != 1 | ttScore <= alpha)
                return ttScore;

            // Internal Iterative Reduction (IIR). Tests TT move instead of hash to reduce in previous fail low nodes.
            // It's especially important to reduce in pv nodes(!), and better to do this after(!) checking for TT cutoffs.
            if (remainingDepth > 3 /*TODO: Test with 4?*/ && ttMove == default) // TODO: also test for matching tt hash to only reduce fail low?
                --remainingDepth;
            
            if (allowPruning)
            {
                // Reverse Futility Pruning (RFP) // TODO: Increase depth limit? Should probably increase the scaling factor
                // The scaling factor of 64 is really aggressive but somehow gains elo even in LTC over something like 100
                if (!inQsearch && remainingDepth < 5 && standPat >= beta + 64 * remainingDepth)
                    return standPat;

                // Null Move Pruning (NMP). TODO: Avoid zugzwang by testing phase? Probably not worth the tokens
                if (remainingDepth >= 4 && allowNmp && standPat >= beta)
                {
                    board.ForceSkipTurn();
                    // changing the ply by a large number doesn't seem to gain elo, even though this should prevent overwriting killer moves
                    search(beta, 3 + remainingDepth / 4, false);
                    board.UndoSkipTurn();
                    if (childScore >= beta)
                        return childScore;
                }
            }

            // the following is 13 tokens for a slight (not passing SPRT after 10k games) elo improvement
            // killers[halfPly + 2] = killers[halfPly + 3] = default;

            // Generate moves. Do this as late as possible to avoid paying the performance cost when we can forward prune.
            var legalMoves = board.GetLegalMoves(inQsearch);
            
            // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending
            var moveScores = new long[legalMoves.Length];
            foreach (Move move in legalMoves)
                moveScores[moveIdx++] = -(move == ttMove ? 1_000_000_001 // order the TT move first
                    // Considering queen promotions as non-quiet moves didn't gain
                    : move.IsCapture ? (int)move.CapturePieceType * 1_048_576 - (int)move.MovePieceType // then captures, ordered by MVV-LVA
                    // Giving the first killer a higher score doesn't seem to gain after 10k games
                    : move == killers[halfPly] || move == killers[halfPly + 1] ? 1_000_000 // killers
                    : history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]); // quiet history

            
            // Don't update the TT for draws and checkmates. This needs fewer tokens and gains elo (there wouldn't be a tt move anyway).
            if (moveIdx == 0) // slightly better than using `IsInCheckmate` and `IsDraw`, also not too token-hungry
                return inQsearch ? bestScore : inCheck ? halfPly - 30_000 : 0; // being checkmated later is better (as is checkmating earlier)
            
            Array.Sort(moveScores, legalMoves);

            Move localBestMove = ttMove; // init to TT move to prevent overriding the TT move in fail-low nodes
            moveIdx = 0;
            // Also a possible idea: using a parameter parentMove, which can be used for 7th rank pawn move extensions
            foreach (Move move in legalMoves) // Still fewer tokens than writing while (--moveIdx >= 0)
            {
                // -1_000_000 is the killer move score, so only history moves are uninteresting
                bool uninterestingMove = moveScores[moveIdx] > -1_000_000;
                // Futility Pruning (FP) and Late Move Pruning (LMP). Would probably benefit from more tuning
                if (remainingDepth <= 5 && bestScore > -29_000 && allowPruning  // && !inQsearch doesn't gain
                    && (uninterestingMove && standPat + 300 + 64 * remainingDepth < alpha
                        || moveIdx > 7 + remainingDepth * remainingDepth)) // TODO: Try adding && !inQsearch to LMP only?
                    break;
                board.MakeMove(move);
                // Principle Variation Search (PVS). Mostly there to have the `isNotPvNode` variable to signify which nodes are uninteresting
                // adding || inQsearch loses elo, quickly fails the SPRT
                if (moveIdx++ == 0 || // Assume that the TT move is the best choice, so search it with a full window and everything else with a zero window
                    alpha < search(alpha + 1,
                        // Late Move Reductions (LMR), needs further parameter tuning. `reduction` is R + 1 to save tokens 
                        moveIdx >= (isNotPvNode ? 3 : 4)
                        && remainingDepth > 3 // don't do LMR at shallow depths or in qsearch
                        && uninterestingMove // Only gains a very small amount of elo over !move.IsCapture
                        // the inCheck condition doesn't seem to gain, failed a [0,10] SPRT with +1.6 after 5.7k games
                            // reduction values based on values originally from the Viridithas engine, which seem pretty widely used by now
                            // Log is expensive to compute, but precomputing would need too many tokens
                            // check extensions take care of not dropping into qsearch while in check (not adding -1 to remainingDepth passes the SPRT with +34 elo)
                            ? Min((int)(1.0 + Log(remainingDepth) * Log(moveIdx) / 2.36) + ToInt32(!isNotPvNode), remainingDepth)
                            : 1
                        ) && childScore < beta) // here, `childScore` refers to the result from the zw search we just did in the same statement
                    search(beta); // pvs re-search or first move

                board.UndoMove(move);

                if (timer.MillisecondsElapsedThisTurn * 16 > timer.MillisecondsRemaining) // time management hard bound
                    return 12345; // the value won't be used, so use a canary to detect bugs

                bestScore = Max(childScore, bestScore);
                if (childScore <= alpha)         
                    continue; // `continue` doesn't save tokens but saves indentation, making the code easier to read
                
                localBestMove = move;
                if (halfPly == 0) bestRootMove = localBestMove; // update in the move loop to use the result of unfinished searches (unless they failed low in the aw)
                alpha = childScore;
                ++flag; // saves one token over flag = 2, won't ever reach 256 so it's fine
                
                if (childScore < beta) continue;
                // found a beta cutoff, now update some move ordering statistics (killer moves and history scores) before breaking
#if PRINT_DEBUG_INFO
            ++betaCutoffCtr;
            if (remainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
#endif
                flag = 0;
                
                if (move.IsCapture) break; // saves 1 token over if (!move.IsCapture) {}
    
                // Having 2 killers isn't worth the tokens, but at least it's different from most other "strong" challenge engines
                if (move != killers[halfPly])
                    killers[halfPly + 1] = killers[halfPly];
                killers[halfPly] = move;


                // History gravity wasn't worth the tokens.
                // ref long histValue = ref history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index];
                // long increment = 1L << remainingDepth; // could make parameters, like alpha and beta, `long` to save one token here
                // histValue += increment - histValue * increment / 524_288;
                
                // From-To instead of color-piece-to lost around 5 elo, which would arguably be worth it for the saved tokens,
                // but I feel like this is a bit more unique, at least
                // 1 << remainingDepth gained over remainingDepth * remainingDepth
                history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]
                    += 1L << remainingDepth; // remainingDepth can't be negative because then the move wouldn't be quiet

                break;
            }
            
            // After the move loop: Update the TT and return the best child score

            tt[ttIndex] = (board.ZobristKey, localBestMove, (short)bestScore, flag, (sbyte)remainingDepth);
            
            return bestScore;
        }
    }
        
        
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