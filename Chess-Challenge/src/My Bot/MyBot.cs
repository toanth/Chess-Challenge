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
                if (score != 12345) lastScore = score; // don't display the canary value of an unfinished search (the chosen move may stlil have been updated)
#endif
                alpha = beta = score; // will shortly get widened
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
#endif
            
            ref TTEntry ttEntry = ref tt[board.ZobristKey & 0x7f_ffff];

            // Using stackalloc doesn't gain elo
            bool inQsearch = remainingDepth <= 0,
                isNotPvNode = alpha + 1 >= beta,
                inCheck = board.IsInCheck(),
                allowPruning = isNotPvNode && !inCheck,
                trustTTScore = ttEntry.key == board.ZobristKey
                    && ttEntry.flag != 0 | ttEntry.score >= beta // Token-efficient flag cut-off condition by Broxholme
                    && ttEntry.flag != 1 | ttEntry.score <= alpha;
            stmColor = board.IsWhiteToMove; // a member variable because it's also used in eval (eval is called at most once per node, before visiting children)

            int bestScore = -32_000,
                standPat = trustTTScore ? ttEntry.score : eval(), // using the TT score passed the SPRT with a 20 elo gain
                moveIdx = 0,
                childScore;

            byte flag = 1;

            if (halfPly > 0 && board.IsRepeatedPosition())
                return 0;

            if (inQsearch && (alpha = Max(alpha, bestScore = standPat)) >= beta)
                return standPat;
            
            // Check Extensions
            if (inCheck) ++remainingDepth; // TODO: Do this before setting qsearch to disallow dropping to qsearch in check? Probably unimportant

            // TODO: Use tuple as TT entries

            if (isNotPvNode && ttEntry.depth >= remainingDepth && trustTTScore)
                return ttEntry.score;

            int search(int minusNewAlpha, int reduction = 1, bool allowNullMovePruning = true) =>
                childScore = -negamax(remainingDepth - reduction, -minusNewAlpha, -alpha, halfPly + 2, allowNullMovePruning);

            if (allowPruning)
                // Reverse Futility Pruning (RFP) // TODO: Don't do in check? Increase depth?
                if (!inQsearch && remainingDepth < 5 && standPat >= beta + 64 * remainingDepth)
                    return standPat;

                // Null Move Pruning (NMP). TODO: Avoid zugzwang by testing phase? Probably not worth the tokens
                else if (remainingDepth >= 4 && allowNmp && standPat >= beta)
                {
                    board.ForceSkipTurn();
                    //int reduction = 3 + remainingDepth / 5;
                    // changing the ply by a large number doesn't seem to gain elo, even though this should prevent overwriting killer moves
                    search(beta, 3 + remainingDepth / 5, false);
                    board.UndoSkipTurn();
                    if (childScore >= beta)
                        return childScore;
                }

            // the following is 13 tokens for a slight (not passing SPRT after 10k games) elo improvement
            // killers[halfPly + 2] = killers[halfPly + 3] = default;

            // generate moves
            var legalMoves = board.GetLegalMoves(inQsearch);

            // using this manual for loop and Array.Sort gained about 50 elo compared to OrderByDescending
            var scores = new int[legalMoves.Length];
            foreach (Move move in legalMoves)
            {
                // TODO: Maybe order captures (or even quiet moves) later if the the target square is attacked (at all/by a less valuable piece), discount SEE
                scores[moveIdx++] = -(move == ttEntry.bestMove ? 1_000_000_000 : // order the TT move first
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
                    && (scores[moveIdx] > -1_000_000 && standPat + 300 + 64 * remainingDepth < alpha || moveIdx > 7 + remainingDepth * remainingDepth)) break;
                board.MakeMove(move);
                // adding || inQsearch and seems to lose elo, quickly fails the SPRT
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
                            : Clamp((int)(0.77 + Log(remainingDepth) * Log(moveIdx) / 2.36) + 1 - ToInt32(isNotPvNode), 1, remainingDepth - 1)
                        ) && childScore < beta) // here, `childScore` refers to the result from the search we just did in the same statement
                        search(beta); // pvs re-search or first move

                board.UndoMove(move);

                if (timer.MillisecondsElapsedThisTurn * 16 > timer.MillisecondsRemaining) // time management hard bound
                    return 12345; // the value won't be used, so use a canary to detect bugs

                bestScore = Max(childScore, bestScore);
                if (childScore <= alpha) continue; // `continue` doesn't save tokens but saves identation, making the code easier to read
                
                localBestMove = move;
                if (halfPly == 0) bestRootMove = localBestMove; // update in the move loop to use the result of unfinished searches (unless they failed low in the aw)
                alpha = childScore;
                ++flag; // saves one token over flag = 2, won't ever reach 256 so it's fine
                if (childScore < beta) continue;
                
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

            if (moveIdx == 0) // slightly better than using `IsInCheckmate` and `IsDraw`, also not too token-hungry
                return inQsearch ? bestScore : inCheck ? halfPly - 30_000 : 0; // being checkmated later is better (as is checkmating earlier)

            ttEntry = new(board.ZobristKey, localBestMove, (short)bestScore,
                flag, (sbyte)remainingDepth);

            return bestScore;
        }


        // Eval very loosely based on JW's example bot (ie tier 2 bot)
        // Uses a lossless "compression" of bytes to decimals
        int eval()
        {
            // TODO: Maybe add a small "random" component like the last 2 bits of the zobrist hash to approximate mobility?
            int phase = 0, mg = 7, eg = 7;
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
            return (mg * phase + eg * (24 - phase)) / 24; // if scores weren't stored as shorts in the TT, the / 24 would be unnecessary // TODO: Change TT size?
        }
    }
}