﻿#if DEBUG
// comment this to stop printing debug/benchmarking information
#define PRINT_DEBUG_INFO
#define GUI_INFO
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
 his lands. A veteran of many battles, His Highness prefers to personally lead his troops, charging at his foes.
 This radical approach to strategy has already cost the life of all of his ancestors, but the Gᴀᴍʙᴏᴛ dynasty values courage
 above all else. Eager to prove once more that he is not a craven, the King will be found where the fighting
 is thickest. Despite this apparent foolishness, King Gᴀᴍʙᴏᴛ Ⅳ is a shrewd tactician and will mercilessly crush any
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
// - Eval Function:
// -- Piece Square Tables
// -- King on (Semi) Open File Malus
// -- Bishop Pair Bonus
// -- All eval weights tuned specifically for Gᴀᴍʙᴏᴛ using Gedas' tuner (https://github.com/GediminasMasaitis/texel-tuner),
//      tuned with datasets generated from self-play (of a prior version based on my public general tuned PSTs) and
//      two publicly available datasets, lichess-big3-resolved and zurichess v7.

public class MyBot : IChessBot
{
    // TODO: Likely bug with the scores? Output:
    // Depth 1, score 29990, best Move: 'c8c7', nodes 0k, time 0, nps ∞k
    // Depth 2, score 1816, best Move: 'c8c7', nodes 0k, time 0, nps ∞k
    // Depth 3, score 29994, best Move: 'c8c7', nodes 0k, time 0, nps ∞k

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


    // The "compression" is basically the same as my public (i.e., posted on Discord) compression to ulongs,
    // except that I'm now compressing to decimals and I'm not using the PeSTO values anymore.
    // Instead, these are my own tuned values, which are different from my public tuned values
    // because they are specifically tuned for King Gᴀᴍʙᴏᴛ's playstyle.
    // The values were tuned using Gedas' tuner: https://github.com/GediminasMasaitis/texel-tuner 
    // Interestingly, my general tuned values provided ~40 elo over PeSTO values for some (most/all? Idk)
    // challenge engines but performed only slightly better than PeSTO when substituting the joke king mg table.
    // So, these values are tuned under the assumption that the king mg table is modified.
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

    private static readonly byte [] weights = new[]
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
        // { 41259558725398675510853667m, 12148637347380404736478291235m, 17409920479544014574385650723m, // Gᴀᴍʙᴏᴛ
        //     17415950995802007349190544675m, 19582331915682545714989742883m, 27619270727875342631837375779m, 24204036247830057755747118883m,
        //     17716926000957227102796124963m, 19219649429467377772218824069m, 28239454489355639738253596073m, 27325506771411963816148256352m,
        //     28264832339681996834391564930m, 28285318131004637962436646247m, 34744636933779914807726423201m, 30108430193492269077365032261m,
        //     26358318502736414220497415448m, 26024674163128048397857214237m, 28222501119009922709615786538m, 30083038105128295398849225533m,
        //     27655505596085286063883075394m, 29200508052015883130554929764m, 36923093278145401739939217243m, 36594255899084379218429593404m,
        //     26988140853361773753654094351m, 20481735021348750806049781013m, 29789250406995126918145609520m, 30410685558648105341049737513m,
        //     31364471306583769303528813368m, 31069498221196032762646602554m, 33215336754067595776203670319m, 31069483850178941046098595892m,
        //     23925955328855304355682751500m, 17361492703139217298290405640m, 21749903039957548760853086753m, 29476162306263422604794629662m,
        //     30438448480538592833862142767m, 31347517973581000950106331700m, 30113200044799990024416170281m, 25786444850924941043161666349m,
        //     19577387562017899555679794698m, 17054392377348325740372977417m, 21992873407402710435759224095m, 26376415020547582243517860383m,
        //     29460403713898812762275022873m, 30082972231781112332790305062m, 27926224772350602732431576870m, 25132415906995832303888144196m,
        //     20174573194104336959542160919m, 14571249260866763470662951168m, 19521800419985553456631990562m, 24155641624402129114338849295m,
        //     26957941230043120604527360780m, 27267374386283891708650945812m, 24164061566224769821843099707m, 21362970886548611942514389065m,
        //     17654024028026098983430744845m, 6511326489308964189700691235m, 12397638507713055097629927715m, 16428201656437907275762648099m,
        //     19497645755591376731243371811m, 14282320806016557631509196067m, 18582432038614949167326716451m, 15502169531075422390202362659m,
        //     9596453343092634717813303075m, 9596453343092634734267793664m, 18121138423700895515950413056m };

        // // tuned values, king unmodified
        // {
        //     212950802458729125124636713m, 15705354257707905913364293161m, 17886284734751062850179720233m,
        //     28704947985276955475610794537m, 25294525672770583567211211049m, 27453610351319154360527508265m,
        //     26169749855270427020240825385m, 3314993117715924912807488809m, 26175836800601437810498368870m,
        //     34266030091188926439701188726m, 37093712416456874870380215655m, 33407655091501223209364339586m,
        //     38065646162857171870545853818m, 41423975883593176816779063145m, 41092711504754071855917733671m,
        //     32689406223253070718788870922m, 30517094658631603746610188824m, 35800105103043881907481982250m,
        //     41414366203001419842005550153m, 43583164827085432359728436557m, 44225043100648980232468101715m,
        //     42646162423448002925958135659m, 42912083454327113169916973139m, 35158019042461194354428783657m,
        //     27434338486413216338155704589m, 37678766733824686569636666662m, 42642653781307542171894133800m,
        //     46696168127397208164817619755m, 46711822661676704951207638592m, 45459351826433318659740520760m,
        //     42965314229717791541126407229m, 36126392273543039653379355427m, 23722907775454309497098306048m,
        //     33654257291288248952187293982m, 41093996397602844786679717916m, 46074765959885808089312960814m,
        //     46066279867531703380949117742m, 42649846055703700396238415653m, 38604714001490392598358828335m,
        //     33636009586425886299589413137m, 21844213401892417718283104256m, 29288778803434937778961417754m,
        //     36131299090455187090276718107m, 39839093469907527657195804699m, 40155813218821525310528660266m,
        //     37667806009735555121085978654m, 31763317563396347581405633855m, 27724305622641003203319463455m,
        //     15965192881198626412568470272m, 24633200435130217957470203421m, 28959955388981402580140064789m,
        //     32685869764462704660981189389m, 33616704921830787246633618720m, 30486776845212109764262596402m,
        //     24576296029183214112863126090m, 18668308181087167497074867994m, 6361462280992279057982109737m,
        //     11318100937137963379662024745m, 17510251693370977274371464745m, 23681813723147150210670620201m,
        //     15342605548279929553156272425m, 23076080403873733449398517289m, 14383894037969364484963004713m,
        //     5401513622928895175288508713m, 5401513622928687749453119744m, 70245666651085040924749293832m
        // };

        // tuned values with additional score, Gᴀᴍʙᴏᴛ

{ 208063153150458622325293100m, 15682342277095336718331215916m, 17862073254533007938894580780m, 28993848255878051352383863852m, 25266692137604337784659873324m, 27430631391026990066160962348m, 26172111076271539771879985964m, 2682663467954403624427526444m, 26172176967737096112208692334m, 34270865850678084886824511630m, 37098534027005422770695479919m, 33410058887660581824132911522m, 38065636866709363910008874376m, 41426422051708640261174580371m, 41145918500806541029066570290m, 32696617406943010943609569808m, 30515871621706581698507796759m, 35798891418545368124946486831m, 41410725296276936301272013900m, 43581941734889116348673274963m, 44228660433808512153548805979m, 42649765607883535731683557227m, 42897547900264106899276737630m, 35172516818391737146598859311m, 27428293858110792459561755149m, 37672717475748669943891062567m, 42643858022592253047452960811m, 46690104665254284038682551598m, 46711803828559790887905954117m, 45446025309083856093597116987m, 42961668674194836996316034120m, 36128800718065307670610151467m, 23712032203204831173005824768m, 33650630551661478982315566624m, 41085538750994661787626009372m, 46063871480082052131389403697m, 46059026332213898642908085811m, 42640179464900655546098547240m, 38605932446840062652789718847m, 33638455847133473433555131418m, 21846636013514157421274677248m, 29271849175800440073925907996m, 36125249777110266856902518557m, 39828208453283882833982744602m, 40148564461283172764058089006m, 37672641769220111003978721570m, 31763322379079945569862315340m, 27724315178777278485335002408m, 15960347770659249787776022786m, 24627141640160101683000392736m, 28945438909283628048857977369m, 32670153803534771908499299599m, 33611864607805264413621640486m, 30483145438992624403376529723m, 24567805326371884075653355875m, 18635657795503422635219310633m, 6357816596486659951681209388m, 11307220660319016242922275628m, 17499371361932403565171125548m, 23675759742632019172112219948m, 15341396734006724797127803180m, 23078503145270531520487051564m, 14377844760509158994642944300m, 5388182382925415220359798828m, 5388182382925414868201832704m, 60331228170297136319483124743m }

        .SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes).ToArray();
    
#endif

#endregion // weights



    public Move Think(Board board, Timer timer)
    {
        // Pesto.printCompressedPsqt(); // TODO: Remove this line
        
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
            ++awRetryCtr;
#endif
            // tested values: 8, 15, 20, 30 (15 being the second best)
            alpha -= 20;
            beta += 20;
        }
        // Use bestRootMove even from aborted iterations, which works because the TT move at the root is always tried first.
        // Unlike for the other nodes, the TT entry for the root can't ever be overwritten

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
        var (key, bestMove, _, _, _) = tt[board.ZobristKey & 0x7f_ffff];
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
            
            // In general, try to abort as early as possible to avoid unnecessary work
            // (TT probing will cause cache misses and IsInCheck() is also slow)
            if (board.IsRepeatedPosition()) // no need to check for halfPly > 0 here as the method already does essentially the same
                return 0;
            ulong ttIndex = board.ZobristKey & 0x7f_ffff;
            var (ttKey, bestMove, ttScore, ttFlag, ttDepth) = tt[ttIndex];

            // Using stackalloc doesn't gain elo
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
                for (int piece = 6; piece >= 1;)
                    for (ulong mask = board.GetPieceBitboard((PieceType)piece--, isWhite); mask != 0;)
                    {
                        phase += weights[1024 + piece];
                        int psqtIndex = 16 * (BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^
                                              56 * ToInt32(isWhite)) + piece;
                        // The + (<constant> << piece) part is just a trick to encode piece values in one byte
                        mg += weights[psqtIndex] + (34 << piece) + weights[piece + 1040];
                        eg += weights[psqtIndex + 6] + (55 << piece) + weights[piece + 1046];
                        // if (piece == 2 && mask != 0) // Bishop pair bonus
                        // {
                        //     mg += 12;
                        //     eg += 15;
                        // }
                    }
                
                // Penalize Kings on opponent's semi open files. Expressed as giving a bonus when that's not the case
                // to save tokens; the boni will cancel each other out to calculate the same result // TODO: Tune constant
                // Worth 10 elo untuned
                if (ToBoolean(0x0101_0101_0101_0101UL << board.GetKingSquare(isWhite).File
                              & board.GetPieceBitboard(PieceType.Pawn, !isWhite)))
                    mg += 20;

                mg = -mg;
                eg = -eg;
            }
                        
            int bestScore = -32_000,
                // using the TT score passed the SPRT with a 20 elo gain. This is a bit weird since this value is obviously
                // incorrect if the flag isn't exact, but since it gained elo over only trusting exact scores it stays like this
                // The reason behind this is probably that most of the time, original ab bounds weren't completely different from
                // the current ab bounds, so inexact scores are still closer to the truth than the static eval on average
                // (and using the static eval in non-quiet positions is a bad idea anyway), so this leads to better (nmp/rfp/fp) pruning
                // TODO: Use qsearch result as standPat score if not in qsearch?
                // if scores weren't stored as shorts in the TT, the / 24 would be unnecessary // TODO: Change TT size?
                standPat = trustTTEntry ? ttScore : (mg * phase + eg * (24 - phase)) / 24, // Mate score when in check lost elo
                moveIdx = 0,
                passedPawnExtension = 0, //only used for passed pawn extensions, TODO: Rename
                childScore; // TODO: Merge standPat and childScore? Would make FP more difficult / token hungry

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
                return standPat;

            if (isNotPvNode && ttDepth >= remainingDepth && trustTTEntry
                    && ttFlag != 0 | ttScore >= beta // Token-efficient flag cut-off condition by Broxholme
                    && ttFlag != 1 | ttScore <= alpha) // TODO: Use cj's even more token-efficient condition
                return ttScore;

            ttFlag = 1;
            // Internal Iterative Reduction (IIR). Tests TT move instead of hash to reduce in previous fail low nodes.
            // It's especially important to reduce in pv nodes(!), and better to do this after(!) checking for TT cutoffs.
            // TODO: Do after nmp?
            if (remainingDepth > 3 /*TODO: Test with 4?*/ && bestMove == default) // TODO: also test for matching tt hash to only reduce fail low?
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
                        || moveIdx > 7 + remainingDepth * remainingDepth)) // TODO: Try adding && !inQsearch to LMP only?
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
                            // reduction values based on values originally from the Viridithas engine, which seem pretty widely used by now
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
                // ref long histValue = ref history[ToInt32(stmColor), (int)move.MovePieceType, move.TargetSquare.Index];
                // long increment = 1L << remainingDepth; // could make parameters, like alpha and beta, `long` to save one token here
                // histValue += increment - histValue * increment / 524_288;
                
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