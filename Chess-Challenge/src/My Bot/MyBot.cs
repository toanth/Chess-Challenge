#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
// #define GUI_INFO // requires a modified version of the framework
#endif

// #define NO_JOKE // Uncomment this to replace King Gᴀᴍʙᴏᴛ Ⅳ with a more cautious king.

using System;
using static System.Math;
using static System.Convert;
using System.Linq;
using ChessChallenge.API;

// King Gᴀᴍʙᴏᴛ, A Joke Bot by toanth (aka ToTheAnd, which is easier to pronounce I guess)
// Thanks to everyone on the discord server who helped me write this engine (I can't even list all names, you know who you are!)

/***********************************************************************************************************************
 The noble King Gᴀᴍʙᴏᴛ Ⅳ, ruling King of the Hill, has mustered a large army to fight off the vile invaders threatening
 his lands. A veteran of many battles, His Highness prefers to personally lead his troops, charging at his foes wit no
 heed for his own safety.
 This radical approach to strategy has already cost the life of all of his ancestors, but the Gᴀᴍʙᴏᴛ dynasty values courage
 above all else. Eager to prove once more that he is not a craven, the King will be found where the fighting
 is thickest! Despite this apparent foolishness, King Gᴀᴍʙᴏᴛ Ⅳ is a shrewd tactician and will mercilessly crush any
 pretender (Estimated Strength: > 2000 Elo).
***********************************************************************************************************************/

// King Gᴀᴍʙᴏᴛ Ⅳ is a standard, albeit strong, challenge engine where the King Middlegame Piece Square Table has been
// replaced with this:
// 255, 255, 255, 255, 255, 255, 255, 255, 
// 255, 255, 255, 255, 255, 255, 255, 255, 
// 255, 255, 255, 255, 255, 255, 255, 255, 
// 250, 250, 250, 250, 250, 250, 250, 250, 
// 200, 200, 200, 200, 200, 200, 200, 200, 
// 100, 100, 100, 100, 100, 100, 100, 100, 
//  50,  50,  50,  50,  50,  50,  50,  50, 
//   0,   0,   0,   0,   5,   0,   0,   0, 

// Features:
// - Alpha-Beta Pruning Negamax
// - Quiescent Search
// - Move Ordering:
// -- TT Move
// -- MVV-LVA
// -- Two Killer Moves
// -- History Heuristic
// - Transposition Table (move ordering and cutoffs, also used in place of static eval)
// - Iterative Deepening
// - Aspiration Windows
// - Principle Variation Search
// - Check Extensions
// - Pawn Move to 2nd/7th Rank Extensions (aka Passed Pawn Extensions)
// - Null Move Pruning
// - Late Move Reductions
// - Reverse Futility Pruning
// - Futility Pruning
// - Late Move Pruning
// - Internal Iterative Reductions
// - Time Management with a soft and hard bound
// - Eval Function:
// -- Piece Square Tables
// -- King on (Semi) Open File Malus
// -- Doubled Pawns Malus
// -- All eval weights tuned specifically for Gᴀᴍʙᴏᴛ using Gedas' tuner (https://github.com/GediminasMasaitis/texel-tuner),
//      tuned with datasets generated from self play (of a prior version based on my public general tuned PSTs) and
//      two publicly available datasets, lichess-big3-resolved and the zurichess quiet-labeled v7 dataset.

public class MyBot : IChessBot
{

    // TT move ordering alone gained almost 400 elo (and > 200 elo for TT cutoffs)
    // flag: 1 = upper, > 1 = exact, 0 = lower
    // entries are (key, bestMove, score, flag, depth) 
    private (ulong, Move, short, byte, sbyte)[] tt = new (ulong, Move, short, byte, sbyte)[0x80_0000];
    // private dynamic tt = new (ulong, Move, short, byte, sbyte)[0x80_0000];
        
    private Move bestRootMove;

#if PRINT_DEBUG_INFO
    long allNodeCtr;
    long nonQuiescentNodeCtr;
    long betaCutoffCtr;
    long parentOfInnerNodeCtr; // node where remainingDepth is at least 2, so move ordering matters more
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


    // The "compression" is basically the same as my public (i.e., posted on Discord) compression to ulongs,
    // except that I'm now compressing to decimals and I'm not using the PeSTO values anymore.
    // Instead, these are my own tuned values, which are different from my public tuned values
    // because they are specifically tuned for King Gᴀᴍʙᴏᴛ's playstyle.
    // The values were tuned using Gedas' tuner: https://github.com/GediminasMasaitis/texel-tuner 
    // Interestingly, my general tuned values provided ~40 elo over PeSTO values for some (most/all? Idk)
    // challenge engines but performed only slightly better than PeSTO when substituting the joke king mg table.
    // So, these values are tuned under the assumption that the king mg table is modified.
#if NO_JOKE
    // The engine plays normally (untuned PeSTO values); specially tuned values would probably have given >= 40 elo 
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

    // Modified king middle game table to make the king lead his army (as he should)
    private static readonly byte [] weights = new[]
        // tuned values with additional score, Gᴀᴍʙᴏᴛ
{ 208063153150458622325293100m, 15682342277095336718331215916m, 17862073254533007938894580780m, 28993848255878051352383863852m, 25266692137604337784659873324m, 27430631391026990066160962348m, 26172111076271539771879985964m, 2682663467954403624427526444m, 26172176967737096112208692334m, 34270865850678084886824511630m, 37098534027005422770695479919m, 33410058887660581824132911522m, 38065636866709363910008874376m, 41426422051708640261174580371m, 41145918500806541029066570290m, 32696617406943010943609569808m, 30515871621706581698507796759m, 35798891418545368124946486831m, 41410725296276936301272013900m, 43581941734889116348673274963m, 44228660433808512153548805979m, 42649765607883535731683557227m, 42897547900264106899276737630m, 35172516818391737146598859311m, 27428293858110792459561755149m, 37672717475748669943891062567m, 42643858022592253047452960811m, 46690104665254284038682551598m, 46711803828559790887905954117m, 45446025309083856093597116987m, 42961668674194836996316034120m, 36128800718065307670610151467m, 23712032203204831173005824768m, 33650630551661478982315566624m, 41085538750994661787626009372m, 46063871480082052131389403697m, 46059026332213898642908085811m, 42640179464900655546098547240m, 38605932446840062652789718847m, 33638455847133473433555131418m, 21846636013514157421274677248m, 29271849175800440073925907996m, 36125249777110266856902518557m, 39828208453283882833982744602m, 40148564461283172764058089006m, 37672641769220111003978721570m, 31763322379079945569862315340m, 27724315178777278485335002408m, 15960347770659249787776022786m, 24627141640160101683000392736m, 28945438909283628048857977369m, 32670153803534771908499299599m, 33611864607805264413621640486m, 30483145438992624403376529723m, 24567805326371884075653355875m, 18635657795503422635219310633m, 6357816596486659951681209388m, 11307220660319016242922275628m, 17499371361932403565171125548m, 23675759742632019172112219948m, 15341396734006724797127803180m, 23078503145270531520487051564m, 14377844760509158994642944300m, 5388182382925415220359798828m, 5388182382925414868201832704m, 60331228170297136319483124743m }
        .SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes).ToArray();
    
#endif

#endregion // weights



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
            // Aspiration Windows (AW): Assume that our search will return a similar score to the last time we searched,
            // so set alpha and beta around that score and gradually widen if that assumption gets disproven
            // excluding checkmate scores was inconclusive after 6000 games, so likely not worth the tokens
            if (score <= alpha) alpha = score;
            else if (score >= beta) beta = score;
            else
            {
#if PRINT_DEBUG_INFO
                Console.WriteLine("Depth {0}, score {1}, best {2}, nodes {3}k, time {4}ms, nps {5}k",
                    depth, score, bestRootMove, Round(allNodeCtr / 1000.0), timer.MillisecondsElapsedThisTurn,
                    Round(allNodeCtr / (double)timer.MillisecondsElapsedThisTurn, 1));
#endif
#if GUI_INFO
                lastDepth = depth;
                if (score <= 30_000) lastScore = score; // don't display the canary value of an unfinished search (the chosen move may still have been updated)
#endif
                alpha = beta = score; // reset window to be centered on the current score, will be widened shortly
                ++depth;
            }

            // start the next iteration of ID or re-search with relaxed AW bounds.
#if PRINT_DEBUG_INFO
            awRetryCtr += 1 - ToInt32(alpha == beta && alpha == score);
#endif
            // tested values: 8, 15, 20, 30 (15 being the second best)
            alpha -= 20;
            beta += 20;
        }
        // Use bestRootMove even from aborted iterations, which works because the TT move at the root is always tried first.
        // Unlike for the other nodes, the TT entry for the root can't ever be overwritten in an incomplete iteration.

#if PRINT_DEBUG_INFO
        Console.WriteLine("All nodes: " + allNodeCtr + ", non quiescent: " + nonQuiescentNodeCtr + ", beta cutoff: " +
            betaCutoffCtr + ", percent cutting (higher is better): " + (1.0 * betaCutoffCtr / allNodeCtr).ToString("P1")
            + ", percent cutting for parents of inner nodes: "
            + (1.0 * parentOfInnerNodeBetaCutoffCtr / parentOfInnerNodeCtr).ToString("P1")
            + ", aspiration window retries: " + awRetryCtr);
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
        var (key, bestMove, _, _, _) = tt[board.ZobristKey & 0x7f_ffff];
        var move = bestMove;
        // for nodes that failed low, no move is stored in the TT
        if (board.ZobristKey == key && board.GetLegalMoves().Contains(move) && remainingDepth > 0)
        {
            Console.Write(move + " ");
            board.MakeMove(move);
            printPv(remainingDepth - 1);
            board.UndoMove(move);
        }
    }
#endif

        return bestRootMove; // The end. Now follow some methods that have been turned into local functions to save tokens

        // The recursive search function. Returns a score and also sets `bestRootMove`, which will eventually be returned by `Think`.
        // halfPly (ie quarter move) instead of ply to save tokens when accessing killers
        int negamax(int remainingDepth, int alpha, int beta, int halfPly, bool allowNmp)
        {
#if PRINT_DEBUG_INFO
            ++allNodeCtr; // apparently, it's more common to count nodes right after the MakeMove call, but this should work as well for nps.
            if (remainingDepth > 0) ++nonQuiescentNodeCtr;
            if (remainingDepth > 1) ++parentOfInnerNodeCtr;
            System.Diagnostics.Debug.Assert(alpha < beta); // spell out name to avoid having to guard the additional using directive with PRINT_DEBUG_INFO
            System.Diagnostics.Debug.Assert(alpha >= -32_000);
            System.Diagnostics.Debug.Assert(beta <= 32_000);
            System.Diagnostics.Debug.Assert(remainingDepth < 100);
            System.Diagnostics.Debug.Assert(halfPly >= 0 && halfPly % 2 == 0 && halfPly < 256);
#endif
            
            // Try to abort as early as possible to avoid unnecessary work
            if (board.IsRepeatedPosition()) // no need to check for halfPly > 0 here as the method already does essentially the same
                return 0;
            ulong ttIndex = board.ZobristKey & 0x7f_ffff;
            var (ttKey, bestMove, ttScore, ttFlag, ttDepth) = tt[ttIndex];

            bool isNotPvNode = alpha + 1 >= beta,
                inCheck = board.IsInCheck(),
                allowPruning = isNotPvNode && !inCheck,
                trustTTEntry = ttKey == board.ZobristKey,
                stmColor = board.IsWhiteToMove;

            
            // Eval. Uses a lossless "compression" of 12 bytes into one decimal literal.
            // Using a small "random" component (the last 4 bits of the zobrist hash) as tempo bonus to approximate mobility didn't gain :/
            int phase = 0, mg = 7, eg = 7;
            foreach (bool isWhite in new[] { stmColor, !stmColor })
            {
                ulong pawns = board.GetPieceBitboard(PieceType.Pawn, isWhite);
                int numDoubledPawns = BitboardHelper.GetNumberOfSetBits(pawns & pawns << 8),
                    piece = 6;
                while (piece >= 1)
                    for (ulong mask = board.GetPieceBitboard((PieceType)piece--, isWhite); mask != 0;)
                    {
                        phase += weights[1024 + piece];
                        int psqtIndex = 16 * (BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^
                                              56 * ToInt32(isWhite)) + piece;
                        // The + (<constant> << piece) part is just a trick to encode piece values in one byte
                        mg += weights[psqtIndex] + (34 << piece) + weights[piece + 1040];
                        eg += weights[psqtIndex + 6] + (55 << piece) + weights[piece + 1046];
                    }

                // Penalize Kings on opponent's semi open files. Expressed as giving a penalty when that's not the case
                // for the opponent to save tokens; the boni will cancel each other out to calculate the same result
                if (ToBoolean(0x0101_0101_0101_0101UL << board.GetKingSquare(!isWhite).File & pawns))
                    mg -= 40; // Avoid dangerous areas while still going forward with the king
                // Flip the sign before adding the opponent's eval value and also add a doubled pawns penalty
                mg = numDoubledPawns * 9 - mg;
                eg = numDoubledPawns * 32 - eg;
            }

            int bestScore = -32_000,
                // using the TT score passed the SPRT with a 20 elo gain. This is a bit weird since this value is obviously
                // incorrect if the flag isn't exact, but since it gained elo over only trusting exact scores it stays like this.
                // The reason for this is probably that most of the time, original ab bounds weren't completely different from
                // the current ab bounds, so inexact scores are still closer to the truth than the static eval on average
                // (and using the static eval in non-quiet positions is a bad idea anyway), so this leads to better (nmp/rfp/fp) pruning
                // If scores weren't stored as shorts in the TT, the / 24 would be unnecessary // TODO: Change TT size?
                // 50 Move Rule Scaling didn't gain.
                standPat = trustTTEntry ? ttScore : (mg * phase + eg * (24 - phase)) / 24, // Mate score when in check lost elo
                moveIdx = 0,
                passedPawnExtension = 0,
                childScore;

            // Idea of using a local function for the recursion to save tokens originally from Tyrant (I think?),
            // whose engine was very influential (for many strong engines in the discord server) in general.
            // That being said, I've noticed a lot of similarities to his engines that have arisen through some kind
            // of "convergent evolution" when saving tokens instead of through copy-pasting / reading source code.
            int search(int minusNewAlpha, int reduction = 1, bool allowNullMovePruning = true) =>
                childScore = -negamax(remainingDepth - reduction + passedPawnExtension, -minusNewAlpha, -alpha, halfPly + 2, allowNullMovePruning);

            // Check Extensions
            if (inCheck) ++remainingDepth;
            
            // Set the inQsearch flag after a possible checkExtension to avoid dropping into qsearch while in check (passing SPRT with +34(!) elo) 
            bool inQsearch = remainingDepth <= 0;
                
            if (inQsearch && (alpha = Max(alpha, bestScore = standPat)) >= beta) // the qsearch stand-pat check, token optimized
                return standPat; // TODO: Move this and eval after tt cutoffs (iir specifically)? Probably really good idea, test

            if (isNotPvNode && ttDepth >= remainingDepth && trustTTEntry
                // Very token-efficient cutoff condition based on an implementation by cj5716 in the nn example bot by jw
                && (ttScore >= beta ? ttFlag != 1 : ttFlag != 0))
                return ttScore;

            ttFlag = 1;
            // Internal Iterative Reduction (IIR). Tests TT move instead of hash to reduce in previous fail low nodes.
            // It's especially important to reduce in pv nodes(!), and better to do this after(!) checking for TT cutoffs.
            if (remainingDepth > 3 && bestMove == default)
                --remainingDepth;
            
            if (allowPruning)
            {
                // Reverse Futility Pruning (RFP) // TODO: Increase depth limit? Should probably increase the scaling factor
                // The scaling factor of 64 is really aggressive but somehow gains elo even in LTC over something like 100
                if (!inQsearch && remainingDepth < 5 && standPat >= beta + 64 * remainingDepth)
                    return standPat;

                // Null Move Pruning (NMP).
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
            
            // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending,
            // but using Span and stackalloc didn't gain elo at all.
            var moveScores = new long[legalMoves.Length];
            foreach (Move move in legalMoves)
                moveScores[moveIdx++] = -(move == bestMove ? 2_000_000_000 // order the TT move first
                    // Considering queen promotions as non-quiet moves didn't gain
                    : move.IsCapture ? (int)move.CapturePieceType * 268_435_456 - (int)move.MovePieceType // then captures, ordered by MVV-LVA
                    // Giving the first killer a higher score doesn't seem to gain after 10k games
                    : move == killers[halfPly] || move == killers[halfPly + 1] ? 250_000_000 // killers (use a score > most history scores)
                    : history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]); // quiet history
            
            // Don't update the TT for draws and checkmates. This needs fewer tokens and gains elo (there wouldn't be a tt move anyway).
            // This condition is slightly better than using `IsInCheckmate` and `IsDraw`, also not too token-hungry
            // Thanks to @cj5716 for this suggestion.
            if (moveIdx == 0)
                return inQsearch ? bestScore : inCheck ? halfPly - 30_000 : 0; // being checkmated later is better (as is checkmating earlier)
            
            Array.Sort(moveScores, legalMoves);

            moveIdx = 0;
            // Also a possible idea: using a parameter parentMove, which can be used for 7th rank pawn move extensions
            foreach (Move move in legalMoves) // Still fewer tokens than writing while (--moveIdx >= 0)
            {
                // -250_000_000 is the killer move score, so only history moves are uninteresting
                bool uninterestingMove = moveScores[moveIdx] > -250_000_000;
                // Futility Pruning (FP) and Late Move Pruning (LMP). Would probably benefit from more tuning
                if (remainingDepth <= 5 && bestScore > -29_000 && allowPruning  // && !inQsearch doesn't gain
                    && (uninterestingMove && standPat + 300 + 64 * remainingDepth < alpha
                        || moveIdx > 7 + remainingDepth * remainingDepth))
                    break;
                // Passed pawn extensions are only worth ~10 elo for 26 tokens, but seem to be rather uncommon,
                // which makes King Gᴀᴍʙᴏᴛ be NoT lIkE tHe OtHeR eNgInEs
                // Passed Pawn Extensions (PPE): Pawn moves to the 7th/2nd rank are extended to mitigate the horizon effect before the promotion
                // same number of tokens as the `is` statement, but without a bugged token count (I hope)
                passedPawnExtension = ToInt32(move.MovePieceType == PieceType.Pawn && move.TargetSquare.Rank % 5 == 1);
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
                            // reduction values based on values from Stormphrax, originally from the Viridithas engine
                            // Log is expensive to compute, but precomputing would need too many tokens
                            // check extensions ensure we don't drop into qsearch while in check (not adding -1 to remainingDepth passed the SPRT with +34 elo)
                            ? Min((int)(1.0 + Log(remainingDepth) * Log(moveIdx) / 2.36) + ToInt32(!isNotPvNode), remainingDepth)
                            : 1
                        ) && childScore < beta) // here, `childScore` refers to the result from the zw search we just did in the same statement
                    search(beta); // pvs re-search or first move

                board.UndoMove(move);

                if (timer.MillisecondsElapsedThisTurn * 16 > timer.MillisecondsRemaining) // time management hard bound
                    // the value won't be used, so use a canary to detect bugs (also needs to be big enough to fail low in the parent node)
                    return 30_999;

                bestScore = Max(childScore, bestScore);
                if (childScore <= alpha)         
                    continue; // `continue` doesn't save tokens but saves indentation, making the code easier to read
                
                bestMove = move;
                // Update in the move loop to use the result of unfinished searches
                // Since this is executed only for moves that raise alpha, it won't change the move if the aw failed low
                if (halfPly == 0) bestRootMove = bestMove;
                alpha = childScore;
                ++ttFlag; // saves one token over ttFlag = 2, won't ever reach 256 so it's fine
                
                if (childScore < beta) continue;
                // found a beta cutoff, now update some move ordering statistics (killer moves and history scores) before breaking
#if PRINT_DEBUG_INFO
            ++betaCutoffCtr;
            if (remainingDepth > 1) ++parentOfInnerNodeBetaCutoffCtr;
#endif
                ttFlag = 0;
                
                if (move.IsCapture) break; // saves 1 token over if (!move.IsCapture) {}
    
                // Having 2 killers isn't worth the tokens, but at least it's different from most other "strong" challenge engines
                if (move != killers[halfPly])
                    killers[halfPly + 1] = killers[halfPly];
                killers[halfPly] = move;


                // History gravity wasn't worth the tokens.
                // From-To instead of color-piece-to lost around 5 elo, which would arguably be worth it for the saved tokens,
                // but I feel like this is a bit more unusual, at least
                // 1 << remainingDepth seemed to gain over remainingDepth * remainingDepth, and it's a bit more unique
                history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index]
                    += 1L << remainingDepth; // remainingDepth can't be negative because then the move wouldn't be quiet

                break;
            }
            
            // After the move loop: Update the TT and return the best child score

            tt[ttIndex] = (board.ZobristKey, bestMove, (short)bestScore, ttFlag, (sbyte)remainingDepth);
            
            return bestScore;
        }
    }
}