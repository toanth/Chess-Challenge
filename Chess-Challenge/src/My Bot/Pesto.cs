using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

// This compression isn't really novel. It combines ideas from Skolin, folke, JW and probably others in a way
// that results in a lossless(*) compression using relatively few tokens
// (*) The compression is lossless except for the square a8 in the middle game knight table, which is too
// large by 41 (but still rated very negatively). This shouldn't make a big difference in practice, though.
public class Pesto
{
    // Apparently, there's no way to make arrays immutable in C#, so these are functions that return temporaries.
    public static int[] piecePhase => new int[]{ 0, 1, 1, 2, 4, 0 };
        
    public static double[][] rawTunedPsqts =
        {
 new double []{ // pawn mg
0, 0, 0, 0, 0, 0, 0, 0, 
67.2705, 77.41, 65.1971, 95.992, 80.1443, 83.2687, -6.65961, -29.8519, 
-20.9019, 1.55439, 34.1535, 38.507, 43.5519, 66.7422, 50.5613, 3.84763, 
-30.5371, -4.83439, -0.437566, 2.66975, 27.0673, 15.866, 30.347, 0.175414, 
-43.4882, -11.4647, -13.567, 5.20102, 7.29256, -3.475, 17.264, -16.9946, 
-43.4357, -15.5584, -14.711, -16.6706, 2.08782, -11.1295, 31.831, -2.96272, 
-41.4836, -10.5868, -18.229, -27.9852, -6.24429, 13.7896, 51.6861, -5.07505, 
0, 0, 0, 0, 0, 0, 0, 0, 
 },

 new double []{ // knight mg
-137.865, -91.1226, -24.882, -18.3909, 36.4118, -64.239, -69.4161, -62.6506, 
-19.9033, 7.91996, 62.7807, 49.1355, 62.6092, 109.457, 34.4418, 33.8667, 
10.5069, 50.0158, 59.7328, 85.0585, 114.482, 144.332, 74.5496, 52.7688, 
6.88926, 18.7397, 42.6552, 67.7101, 45.3614, 74.8371, 24.4225, 42.5718, 
-8.45882, 7.92672, 22.3578, 23.5696, 34.2284, 28.5796, 30.8004, -1.22314, 
-27.6461, -4.72284, 12.3105, 12.7506, 25.0573, 13.177, 17.4021, -14.1369, 
-38.7261, -29.4333, -11.906, 1.42422, 1.36368, 7.29756, -3.14399, -8.22835, 
-84.5286, -26.6188, -45.8084, -29.8295, -25.646, -11.1798, -28.83, -64.2635, 
 },

 new double []{ // bishop mg
3.94171, -23.2604, -7.86076, -59.5026, -60.4845, -35.2663, 39.726, -23.3409, 
4.10453, 43.4252, 21.2816, 15.3354, 32.6571, 62.6754, 27.1023, 36.3346, 
17.3984, 44.3735, 60.0858, 66.9576, 64.0672, 87.8119, 82.8034, 44.5072, 
6.31285, 25.1209, 47.4553, 64.9681, 56.3277, 52.1337, 24.9185, 19.9765, 
5.36689, 18.9532, 25.9558, 46.8933, 48.5988, 25.6989, 21.5744, 8.60548, 
17.9275, 24.4393, 23.6879, 26.782, 27.6491, 24.2875, 21.8615, 35.1835, 
18.37, 23.225, 30.4787, 10.0668, 17.0488, 32.6152, 47.1976, 23.5451, 
-6.23591, 12.9795, 2.35751, -5.4579, -4.45783, -6.10697, 17.1816, 8.58079, 
 },

 new double []{ // rook mg
65.8605, 67.7853, 75.2904, 75.2162, 98.4202, 133.339, 152.64, 159.181, 
18.3227, 11.2446, 36.9658, 61.3579, 42.2299, 71.8762, 70.7219, 124.817, 
-5.01001, 16.6397, 16.718, 30.6875, 56.646, 57.0964, 103.518, 60.5273, 
-28.2511, -10.8409, -6.40991, 11.2318, 15.3758, 16.392, 26.095, 11.4958, 
-47.9256, -44.8944, -34.1647, -13.3653, -10.6712, -27.599, -6.9574, -30.3142, 
-56.1079, -41.9432, -33.5652, -27.1923, -21.8057, -23.1342, 9.49385, -19.8133, 
-58.9681, -41.6693, -27.1284, -24.7856, -20.5899, -13.4831, 4.19496, -53.8708, 
-39.4103, -35.7033, -23.9, -2.89277, -5.83467, -13.4944, -23.0428, -58.6516, 
 },

 new double []{ // queen mg
-17.9183, 7.95616, 33.9558, 71.3461, 75.2399, 120.295, 136.585, 71.5748, 
-3.78678, -24.6244, -19.4579, -28.0875, -28.8669, 39.4808, 14.89, 115.39, 
-1.99807, -4.16623, 3.13688, 15.7892, 24.0547, 65.2924, 80.546, 56.6187, 
-19.4746, -13.6502, -9.37207, -8.70735, -2.43, 7.16068, 8.64512, 18.5851, 
-20.0627, -17.0118, -18.4112, -11.6323, -7.73428, -9.25308, 1.77997, 3.29134, 
-19.4457, -9.55657, -15.524, -13.6596, -9.81643, -3.77959, 7.15277, 0.596095, 
-21.7303, -12.0368, -3.70616, 1.6529, -3.04489, 10.9913, 19.4736, 26.4264, 
-22.9876, -34.2787, -23.7473, -6.40373, -17.4095, -29.9375, -36.7143, -40.5401, 
 },

 new double []{ // king mg
255, 255, 255, 255, 255, 255, 255, 255, 
255, 255, 255, 255, 255, 255, 255, 255, 
255, 255, 255, 255, 255, 255, 255, 255, 
250, 250, 250, 250, 250, 250, 250, 250, 
200, 200, 200, 200, 200, 200, 200, 200, 
100, 100, 100, 100, 100, 100, 100, 100, 
10, 10, 50, 50, 50, 50, 30, 10, 
0, 0, 0, 3, 5, 2, 0, 0, 
 },

 new double []{ // pawn eg
0, 0, 0, 0, 0, 0, 0, 0, 
197.784, 191.988, 181.519, 132.169, 124.221, 138.86, 193.762, 204.019, 
128.445, 131.533, 95.6649, 72.2345, 58.9186, 46.0018, 95.8212, 96.1757, 
50.4106, 36.7118, 14.9686, 2.48036, -9.00047, -4.06035, 15.693, 19.3628, 
23.7035, 18.4126, -1.30899, -7.19926, -9.96916, -5.72418, 6.91235, 2.78279, 
15.952, 17.7088, -2.31649, 7.77473, 1.66084, 1.46714, 10.0394, -0.206534, 
22.3445, 23.2142, 13.3392, 16.1225, 16.5236, 13.5897, 12.0192, -1.53487, 
0, 0, 0, 0, 0, 0, 0, 0, 
 },

 new double []{ // knight eg
-75.2233, -21.7753, -4.91457, -5.89962, -7.2077, -21.4962, -10.4024, -106.49, 
-20.9692, 0.675256, 1.88909, 9.31808, -4.27398, -16.0809, -10.9662, -33.9404, 
-7.20791, 7.06836, 29.8888, 24.4236, 6.19298, 2.54067, -5.3191, -21.2066, 
4.60918, 31.1483, 40.5081, 44.6622, 42.2177, 37.1027, 27.7432, -2.63246, 
6.83236, 19.6135, 43.9666, 43.8204, 46.4403, 36.108, 20.3237, 2.84209, 
-10.7351, 11.562, 20.1848, 37.0092, 37.2801, 16.2401, 7.98933, -7.22919, 
-16.5018, -0.437336, 10.6132, 12.6905, 12.1239, 10.4543, -3.7534, -6.33679, 
-28.0577, -41.0683, -5.96809, -0.692596, -2.39213, -4.44638, -34.4351, -33.8198, 
 },

 new double []{ // bishop eg
5.94331, 17.5155, 15.5381, 30.2884, 29.6726, 19.4879, 6.43893, 6.50669, 
-5.3704, 15.2523, 21.2583, 22.7615, 16.2482, 7.49733, 24.3235, -5.62186, 
26.3212, 17.6949, 29.2603, 18.0245, 22.8417, 25.2733, 12.3635, 15.835, 
19.1442, 40.3004, 29.6638, 44.8564, 36.6132, 31.7823, 32.9553, 15.8364, 
16.3875, 31.0036, 44.6008, 35.4272, 37.5919, 37.3139, 31.0639, 5.8987, 
12.7046, 27.775, 33.317, 32.922, 37.7888, 36.0396, 19.2704, 6.28538, 
9.06657, 6.69638, 10.913, 23.8172, 29.2605, 19.6869, 19.0542, -10.363, 
-10.0229, 11.2999, -12.7839, 17.9136, 12.5176, 14.8707, -4.36334, -18.2948, 
 },

 new double []{ // rook eg
30.9511, 35.8394, 44.3693, 43.5263, 36.4268, 22.7853, 15.8522, 10.9579, 
34.1376, 51.6954, 52.6487, 42.8876, 44.3454, 36.9212, 30.1699, 4.86022, 
37.7928, 40.0376, 41.4971, 37.2116, 25.3715, 19.0898, 11.0878, 9.63853, 
40.8143, 37.9028, 47.4272, 41.2012, 27.2585, 19.3896, 16.6058, 13.4805, 
35.1104, 41.1707, 42.9672, 40.4596, 36.8259, 35.044, 21.9818, 19.9321, 
29.561, 28.6879, 29.2855, 32.9166, 31.5729, 21.4898, 1.80824, 2.57084, 
22.7091, 26.0839, 28.7623, 31.8011, 23.7411, 20.1104, 4.39864, 14.6405, 
15.7269, 28.567, 36.911, 33.5708, 28.7115, 24.0069, 17.2809, 2.32217, 
 },

 new double []{ // queen eg
58.4154, 63.8843, 79.9773, 63.1875, 55.5421, 47.1759, 15.6787, 53.0366, 
32.2866, 73.5017, 110.042, 130.033, 146.003, 102.075, 115.699, 42.1439, 
37.5126, 54.6524, 94.0432, 92.8469, 115.053, 86.5575, 43.4291, 45.8662, 
44.3202, 71.6318, 82.2046, 106.256, 120.15, 103.78, 88.2626, 66.1477, 
47.9697, 72.9367, 79.621, 104.204, 95.3914, 86.8856, 75.0603, 62.2777, 
31.2342, 40.6621, 71.7467, 62.9162, 70.1472, 62.3877, 49.8094, 34.4079, 
30.7413, 29.1889, 24.1511, 31.6238, 40.6846, 9.82406, -18.9189, -56.2882, 
21.5361, 25.5701, 28.1635, 11.3603, 30.9652, 32.464, 9.80377, -9.73784, 
 },

 new double []{ // king eg
-94, -44, -37, -1, -13, -6, -10, -86, 
-10, 16, 25, 13, 28, 39, 38, 11, 
4, 21, 39, 46, 48, 43, 44, 19, 
-6, 27, 43, 56, 56, 52, 44, 22, 
-18, 14, 38, 54, 54, 43, 30, 14, 
-24, 0, 22, 34, 35, 27, 8, -5, 
-43, -15, -1, 11, 14, 4, -15, -34, 
-74, -58, -38, -18, -45, -20, -48, -77, 
 },
};



    public static double[] rawTunedPieceValues =
        
{ 82.9626, 313.965, 337.646, 456.181, 926.007, 0.0, 
121.646, 357.042, 367.056, 653.142, 1241.98, 0.0 };
//
// { 77.4184, 301.884, 324.512, 435.604, 879.878, 0.0, 
// 109.021, 330.511, 338.584, 593.574, 1129.52, 0.0 };



    public static int[][] tunedPsqts =>
        rawTunedPsqts.Select(psqt => psqt.Select(x => (int)Math.Round(x)).ToArray()).ToArray();
    
    public static int[] tunedPieceValues =>
        rawTunedPieceValues.Select(x => (int)Math.Round(x)).ToArray();
    
    
    // // Eg Values are tuned under the assumption that they are used t
    // public static int[] adjustKingEgTable(int[] tunedKingMgTable, int[] modifiedKingMgTable)
    
    public static (Decimal[], int, int) createCompressedPsqts()
    {
        // A decimal can store 12 easily accessible bytes, so the 12 * 64 + 6 + 2 * 6 bytes can be stored in 66 decimals
        Decimal[] compresto = new Decimal[66];
        // create a local copy that can be modified and prevents inadvertently trying to modify the global array
        int[][] tunedPsqts = Pesto.tunedPsqts;
        int[] tunedPieceValues = Pesto.tunedPieceValues;
        
        // first, compress the piece square tables
        int numOverflows = 0;
        byte[] decimalBytes = new byte[16];

        Decimal bytesToDecimal()
        {
            int[] bytesAsInts = new int[4];
            Buffer.BlockCopy(decimalBytes, 0, bytesAsInts, 0, 16);
            return new decimal(bytesAsInts);
        }

        Debug.Assert(tunedPsqts.Length == 12);
        for (int table = 0; table < tunedPsqts.Length; ++table)
        {
            int offset = Math.Min(-tunedPsqts[table].Min(), 255 - tunedPsqts[table].Max());
            // queen piece values tend to be too large to easily find a scaling constant,
            // but this greedy strategy solves that problem in practice
            if (table % 6 == 4) 
            {
                offset = Math.Max(-tunedPsqts[table].Min(), 255 - tunedPsqts[table].Max());
            }
            // Debug.Assert(offset >= 0);
            // Debug.Assert(offset <= 255);
            tunedPieceValues[table] -= offset;
            Debug.Assert(table % 6 == 5 || tunedPieceValues[table] >= 0);
            Debug.Assert(table % 6 == 5 || tunedPieceValues[table] < 2_000);
            tunedPsqts[table] = tunedPsqts[table].Select(val => val + offset).ToArray();
        }
        
        for (int row = 0; row < 8; ++row)
        {
            for (int column = 0; column < 8; ++column)
            {
                int square = row * 8 + column;
                for (int table = 0; table < tunedPsqts.Length; ++table)
                {
                    int psqtEntry = tunedPsqts[table][square];
                    Debug.Assert(psqtEntry <= 0xff);
                    if (psqtEntry < 0)
                    {
                        Console.WriteLine("Overflow on square {0}, piece {1}", square, table);
                        ++numOverflows;
                        psqtEntry = 0; // clamp to zero
                    }
                    decimalBytes[table] = (byte)(psqtEntry & 0xff);
                }
                compresto[square] = bytesToDecimal();
            }
        }
        Debug.Assert(compresto[64] == 0);
        Debug.Assert(numOverflows == 1); // only the piece square value for knight on a8 in the middle game overflows
        
        // compress the phase values
        Debug.Assert(piecePhase.Length == 6);
        Debug.Assert(tunedPieceValues.Length == 12);
        for (int i = 0; i < piecePhase.Length; ++i)
        {
            Debug.Assert(piecePhase[i] is >= 0 and <= 0xff);
            decimalBytes[i] = (byte)(piecePhase[i] & 0xff);
        }
        compresto[64] = bytesToDecimal();
        
        // compress the piece values

        int findScalingConstant(int[] values)
        {
            for (int factor = 0; factor < 100; ++factor)
            {
                int i = 0;
                for (; i < 5; ++i)
                {
                    if (values[i] - (factor << i) is < 0 or >= 256)
                        break;
                }
                if (i == 5) return factor;
            }
            // probably not the right exception type, but who cares?
            Console.WriteLine("values are " + string.Join(", ", values));
            throw new DataException("no scaling constant found");
        }

        int mgScaling = findScalingConstant(tunedPieceValues[..6]);
        int egScaling = findScalingConstant(tunedPieceValues[6..]);
        
        for (int i = 0; i < tunedPieceValues.Length; ++i) // the king's value doesn't matter
        {
            int compressedPieceVal = tunedPieceValues[i] - ((i < 6 ? mgScaling : egScaling) << (i % 6));
            Debug.Assert(compressedPieceVal is >= 0 and <= 0xff || i % 6 == 5);
            compressedPieceVal &= 0xff;
            // Console.WriteLine("compressed piece val {0}, reconstructed {1}", compressedPieceVal, compressedPieceVal + ((i < 6 ? mgScaling : egScaling) << (i % 6)));
            decimalBytes[i] = (byte)compressedPieceVal;
        }
        compresto[65] = bytesToDecimal();
        testCompressedEval(compresto, mgScaling, egScaling);
        return (compresto, mgScaling, egScaling);
    }

    // This array contains not only the piece square tables, but also the phase weights and the piece values
    // public static decimal[] compresto = createCompressedEval();
    
    // uncompress it like this, but replace the `compresto` variable with the hard-coded values
    // (which you can get using the printCompressedPesto() function):
    // public static byte[] evalData = compresto.SelectMany(BitConverter.GetBytes).ToArray();
    
    public static void printCompressedPsqt()
    {
        var (compressed, mgScaling, egScaling) = createCompressedPsqts();
        Console.WriteLine("{ " + string.Join("m, ", compressed) + "m }");
        Console.WriteLine("Scaling: {0}, {1}", mgScaling, egScaling);
    }
    
    public static void testCompressedEval(Decimal[] compresto, int mgScaling, int egScaling)
    {
        byte[] uncompressedPsqts = compresto.SelectMany(decimal.GetBits).SelectMany(BitConverter.GetBytes).ToArray();
        Debug.Assert(uncompressedPsqts.Length == 66 * 16);
        for (int i = 0; i < piecePhase.Length; ++i)
        {
            Debug.Assert(piecePhase[i] == uncompressedPsqts[64 * 16 + i]);
        }

        int[] uncompressedPieceValues = new int[tunedPieceValues.Length];
        Debug.Assert(tunedPieceValues.Length == 12);
        // Console.WriteLine("Scaling factors: {0}, {1}", mgScaling, egScaling);

        for (int i = 0; i < tunedPieceValues.Length; ++i)
        {
            uncompressedPieceValues[i] = uncompressedPsqts[65 * 16 + i] + ((i < 6 ? mgScaling : egScaling) << (i % 6));
            // Console.WriteLine("piece {0}, value {1}", i, uncompressedPieceValues[i]);
            // pesto piece values are changed by subtracting a table-specific offset, so only use >= for comparison
            Debug.Assert(i % 6 == 5 || // the king's piece value doesn't matter
                         tunedPieceValues[i] + 50 >= uncompressedPieceValues[i]);
        }
        
        for (int square = 0; square < 64; ++square)
        {
            for (int piece = 0; piece < 12; ++piece)
            {
                int i = 16 * square + piece;
                if (piece == 1 && square == 0) // the middle game knight on a8 value can't be represented properly in compressed pesto 
                {
                    Debug.Assert(uncompressedPsqts[i] == 0);
                    Debug.Assert(uncompressedPsqts[i] + uncompressedPieceValues[piece] >= tunedPsqts[piece][square] + tunedPieceValues[piece]);
                    Debug.Assert(uncompressedPsqts[i] + uncompressedPieceValues[piece] - 50 <= tunedPsqts[piece][square] + tunedPieceValues[piece]);
                }
                else if (piece % 6 != 5)
                { // king values differ, but that's fine because the white and black values cancel each other out
                    // Console.WriteLine("psqt {0}, piece val {1}, should be {2}, {3}; square {4}, piece {5}", uncompressedPsqts[i], uncompressedPieceValues[piece],
                    //     tunedPsqts[piece][square] , tunedPieceValues[piece], square, piece);
                    Debug.Assert(uncompressedPsqts[i] + uncompressedPieceValues[piece] == tunedPsqts[piece][square] + tunedPieceValues[piece]);
                }
            }
        }
    }

    // This function is useful for testing that the pesto compression worked. Not meant to be called in the actual bot.
    // The code is adapted from cpw (https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function)
    public static int uncompressedEval(Board board)
    {            
        int[] mg = new int[2];
        int[] eg = new int[2];
        int gamePhase = 0;
    
        const int WHITE = 0;
        const int BLACK = 1;
        
        mg[WHITE] = 0;
        mg[BLACK] = 0;
        eg[WHITE] = 0;
        eg[BLACK] = 0;
    
        /* evaluate each piece */
        for (int sq = 0; sq < 64; ++sq)
        {
            // chess challenge square numbers start at a1, but pesto square numbers start at a8
            Piece piece = board.GetPiece(new Square(sq ^ 56));
            int square = piece.IsWhite ? sq : sq ^ 56;
            int pc = (int)piece.PieceType - 1;
            if (piece.PieceType != PieceType.None) {
                mg[piece.IsWhite ? WHITE : BLACK] += tunedPsqts[pc][square] + tunedPieceValues[pc];
                eg[piece.IsWhite ? WHITE : BLACK] += tunedPsqts[pc + 6][square] + tunedPieceValues[pc + 6];
                gamePhase += piecePhase[pc];
            }
            // Console.Write("(" + (pc >= 0 ? pestoPsqts[pc][square] : -1) + ") " + pc + ";  ");
            // if (sq % 8 == 7) Console.WriteLine();
        }

        var onA8 = board.GetPiece(new Square("a8"));
        if (onA8.PieceType == PieceType.Knight && onA8.IsWhite)
        {
            mg[WHITE] += 41;
        }

        var onA1 = board.GetPiece(new Square("a1"));
        if (onA1.PieceType == PieceType.Knight && !onA1.IsWhite)
        {
            mg[BLACK] += 41;
        }
        
        /* tapered eval */
        int mgScore = mg[WHITE] - mg[BLACK];
        int egScore = eg[WHITE] - eg[BLACK];
        int mgPhase = gamePhase;
        // this isn't done for the challenge pesto eval
        // if (mgPhase > 24) mgPhase = 24; /* in case of early promotion */
        int egPhase = 24 - mgPhase;
        return (mgScore * mgPhase + egScore * egPhase) / 24 * (board.IsWhiteToMove ? 1 : -1);
    }
    
}
