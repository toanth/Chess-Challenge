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
    // TODO: Likely bug with the scores? Output:
    // Depth 1, score 29990, best Move: 'c8c7', nodes 0k, time 0, nps ∞k
    // Depth 2, score 1816, best Move: 'c8c7', nodes 0k, time 0, nps ∞k
    // Depth 3, score 29994, best Move: 'c8c7', nodes 0k, time 0, nps ∞k

    // TODO: Use tuple for this, add more 200 token optimizations
    
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


#if NO_JOKE

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

    // modified pesto values to make the king lead the army (as he should)

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

    // values are from pesto for now, with a modified king middle game table unless NO_JOKE is defined
    //private byte[] pesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();
    private byte[] pesto = compresto.SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes).ToArray();

    #endregion // compresto



    public Move Think(Board board, Timer timer)
    {
        var history = new int[2, 7, 64];
        var killers = new Move[256];

        // starting with depth 0 wouldn't only be useless but also incorrect due to assumptions in negamax
        // 30_000 is used as infinity because it comfortably fits into 16 bits
        for (int depth = 1, alpha = -30_000, beta = 30_000; depth < 64 && timer.MillisecondsElapsedThisTurn <= timer.MillisecondsRemaining / 64;)
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
                Console.WriteLine("Depth {0}, score {1}, best {2}, nodes {3}k, time {4}, nps {5}k",
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
            // (TT probing will result in cache misses and IsInCheck() is also slow)
            if (halfPly > 0 && board.IsRepeatedPosition())
                return 0;
            
            ref TTEntry ttEntry = ref tt[board.ZobristKey & 0x7f_ffff];

            // Using stackalloc doesn't gain elo
            bool inQsearch = remainingDepth <= 0,
                isNotPvNode = alpha + 1 >= beta,
                inCheck = board.IsInCheck(),
                allowPruning = isNotPvNode && !inCheck,
                trustTTScore = ttEntry.key == board.ZobristKey
                    && ttEntry.flag != 0 | ttEntry.score >= beta // Token-efficient flag cut-off condition by Broxholme
                    && ttEntry.flag != 1 | ttEntry.score <= alpha,
                stmColor = board.IsWhiteToMove/*,
                tmp = stmColor*/;

            
        // Uses a lossless "compression" of 12 bytes into one decimal literal
        // TODO: Maybe add a small "random" component like the last 4 bits of the zobrist hash to approximate mobility?
        int phase = 0, mg = 7, eg = 7;
        foreach (bool isWhite in new[] { stmColor, !stmColor })
        // do
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

            // stmColor ^= true;
            mg = -mg;
            eg = -eg;
        } //while (stmColor != tmp);
        // return (mg * phase + eg * (24 - phase)) / 24; // if scores weren't stored as shorts in the TT, the / 24 would be unnecessary // TODO: Change TT size?
            
            int bestScore = -32_000,
                // using the TT score passed the SPRT with a 20 elo gain. This is a bit weird since this value is obviously
                // incorrect if the flag isn't exact, but since it gained elo it stays like this // TODO: Retest with only trusting exact scores?
                standPat = trustTTScore ? ttEntry.score : (mg * phase + eg * (24 - phase)) / 24, // TODO: Use -30_000 for standPat when in check? 
                moveIdx = 0,
                childScore; // TODO: Merge standPat and childScore? Would make FP more difficult / token hungry

            byte flag = 1;
            
            int search(int minusNewAlpha, int reduction = 1, bool allowNullMovePruning = true) =>
                childScore = -negamax(remainingDepth - reduction, -minusNewAlpha, -alpha, halfPly + 2, allowNullMovePruning);

            if (inQsearch && (alpha = Max(alpha, bestScore = standPat)) >= beta) // the qsearch stand-pat check
                return standPat;
            
            // Check Extensions
            if (inCheck) ++remainingDepth; // TODO: Do this before setting qsearch to disallow dropping to qsearch in check? Probably unimportant

            // TODO: Use tuple as TT entries

            if (isNotPvNode && ttEntry.depth >= remainingDepth && trustTTScore)
                return ttEntry.score;

            // Internal Iterative Reduction (IIR)
            if (remainingDepth > 3 /*TODO: Test with 4? Add || !isNotPvNode?*/ && ttEntry.bestMove == default) // TODO: also test for matching tt hash to only reduce fail low?
                --remainingDepth;
            
            if (allowPruning)
            {
                // Reverse Futility Pruning (RFP) // TODO: Increase depth limit?
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
                    if (childScore >= beta)
                        return childScore;
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
                // TODO: consider move.PromotionPieceType == PieceType.Queen as non-quiet everywhere?
                // TODO: Maybe order captures (or even quiet moves) later if the the target square is attacked (at all/by a less valuable piece), discount SEE
                scores[moveIdx++] = /*-ToInt32(board.SquareIsAttackedByOpponent(move.TargetSquare)) // maybe do this only for captures but increase offset dramatically?*/
                    - (move == ttEntry.bestMove ? 1_000_000_000 : // order the TT move first
                    move.IsCapture ? (int)move.CapturePieceType * 1_048_576 - (int)move.MovePieceType : // then captures, ordered by MVV-LVA
                    // Giving the first killer a higher score doesn't seem to gain after 10k games
                    move == killers[halfPly] || move == killers[halfPly + 1] ? 1_000_000 : // killers
                    history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]); // quiet history
            }

            Array.Sort(scores, legalMoves);

            Move localBestMove = ttEntry.bestMove; // init to TT move to prevent overriding the TT move in fail-low nodes
            moveIdx = 0;
            foreach (Move move in legalMoves)
            {
                // Futility Pruning (FP) and Late Move Pruning (LMP). Would probably benefit from more tuning
                if (remainingDepth <= 5 && bestScore > -29_000 && allowPruning
                    && (scores[moveIdx] > -1_000_000 && standPat + 300 + 64 * remainingDepth < alpha
                        || moveIdx > 7 + remainingDepth * remainingDepth))
                    break;
                board.MakeMove(move);
                // adding || inQsearch loses elo, quickly fails the SPRT
                if (moveIdx++ == 0 ||
                    // Late Move Reductions (LMR), needs further parameter tuning. `reduction` is R + 1 to save tokens
                    alpha < search(alpha + 1, 
                        moveIdx < (isNotPvNode ? 3 : 4)
                        || remainingDepth <= 3
                        || move.IsCapture
                        // the inCheck condition doesn't seem to gain, failed a [0,10] SPRT with +1.6 after 5.7k games
                            ? 1 
                            // reduction values originally from the Viridithas engine, which seem pretty widely used by now
                            // Log is expensive to compute, but precomputing would need too many tokens
                        // ~~TODO~~ + 1 - ToInt32(isNotPvNode) -> + ToInt32(!isNotPvNode)
                            : Clamp((int)(0.77 + Log(remainingDepth) * Log(moveIdx) / 2.36) + 1 - ToInt32(isNotPvNode), 1, remainingDepth - 1)
                        ) && childScore < beta) // here, `childScore` refers to the result from the search we just did in the same statement
                        search(beta); // pvs re-search or first move

                board.UndoMove(move);

                if (timer.MillisecondsElapsedThisTurn * 16 > timer.MillisecondsRemaining) // time management hard bound
                    return 12345; // the value won't be used, so use a canary to detect bugs

                bestScore = Max(childScore, bestScore);
                if (childScore <= alpha) continue; // `continue` doesn't save tokens but saves indentation, making the code easier to read
                
                localBestMove = move;
                if (halfPly == 0) bestRootMove = localBestMove; // update in the move loop to use the result of unfinished searches (unless they failed low in the aw)
                alpha = childScore;
                ++flag; // saves one token over flag = 2, won't ever reach 256 so it's fine
                if (childScore < beta) continue;
                // found a beta cutoff, now update some statistics like killer moves and history scores
#if PRINT_DEBUG_INFO
            ++betaCutoffCtr;
            if (remainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
#endif
                flag = 0;
                
                if (move.IsCapture) break; // saves 1 token over if (!move.IsCapture) {}

                if (move != killers[halfPly]) // TODO: Test using only 1 killer move
                    killers[halfPly + 1] = killers[halfPly];
                killers[halfPly] = move;


                // gravity didn't gain (TODO: Retest later when the engine is better), but history still gained quite a bit
                // TODO: Test from-to instead of stm-piece-to
                // TODO: Test reducing the history score for moves that don't raise alpha
                history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]
                    += remainingDepth * remainingDepth;

                break;
            }
            
            // After the move loop: If there were any legal moves, update the TT and return the best child score
            
            if (moveIdx == 0) // slightly better than using `IsInCheckmate` and `IsDraw`, also not too token-hungry
                return inQsearch ? bestScore : inCheck ? halfPly - 30_000 : 0; // being checkmated later is better (as is checkmating earlier)

            ttEntry = new(board.ZobristKey, localBestMove, (short)bestScore,
                flag, (sbyte)remainingDepth);

            return bestScore;
        }
    }
}