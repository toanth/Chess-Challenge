using System;
using System.Collections.Generic;
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
        new double[]
        {
            // pawn mg
            0, 0, 0, 0, 0, 0, 0, 0,
            66.2494, 97.5421, 66.7439, 118.427, 92.4425, 103.006, 6.49168, -27.6079,
            -20.6674, 3.0237, 31.8183, 39.1681, 47.3107, 63.0699, 49.9595, 3.1704,
            -31.2294, -4.57117, -1.29955, 2.08899, 25.4081, 14.6176, 27.6878, -1.18419,
            -44.026, -12.1549, -15.6457, 5.01411, 6.9373, -3.77734, 19.0803, -17.8315,
            -43.8648, -15.9458, -15.2048, -17.6787, 2.45316, -9.60889, 31.9592, -3.52711,
            -41.8802, -11.8711, -19.1384, -29.1666, -5.67849, 15.4017, 54.6546, -3.26821,
            0, 0, 0, 0, 0, 0, 0, 0,
        },

        new double[]
        {
            // knight mg
            -142.917, -104.479, -16.7223, -32.6485, 48.5789, -82.0003, -74.3094, -68.1609,
            -24.8146, 10.9799, 69.2096, 51.8204, 72.0599, 118.985, 28.5435, 36.9929,
            20.1342, 56.8297, 62.9259, 86.6191, 122.252, 162.472, 78.96, 69.3998,
            5.41542, 17.8112, 42.7606, 71.7852, 43.9036, 76.9762, 23.1872, 43.481,
            -10.0205, 9.24849, 21.9908, 20.5997, 32.7435, 29.2894, 34.1054, -0.947456,
            -28.6727, -6.79321, 10.1011, 10.8412, 24.9368, 12.4551, 15.6371, -16.166,
            -35.9896, -33.3057, -10.7709, -0.0776318, 0.292794, 4.1333, -3.84753, -5.09719,
            -89.1349, -26.2452, -48.3739, -30.2, -27.5533, -12.0298, -27.6502, -60.5225,
        },

        new double[]
        {
            // bishop mg
            1.91945, -23.3394, -27.7059, -58.3112, -55.4674, -32.7213, 33.9723, -21.0603,
            6.91117, 42.3305, 17.5484, 6.92909, 28.6832, 75.4419, 31.8908, 36.5964,
            19.3668, 47.3173, 63.8443, 71.2712, 70.3831, 93.9458, 84.2114, 38.907,
            5.55636, 21.0587, 48.6394, 67.4269, 55.5596, 53.2325, 18.6478, 17.2365,
            2.24822, 15.7907, 23.5277, 44.9138, 46.8573, 22.1164, 23.6507, 3.15402,
            15.1336, 24.7484, 21.617, 24.5665, 24.6751, 24.1028, 20.3743, 31.7315,
            17.8278, 19.9999, 29.8005, 7.08309, 16.1025, 33.8503, 47.4958, 21.2539,
            -12.1543, 12.0481, -0.796932, -6.20906, -5.38963, -8.78146, 8.76897, 7.34614,
        },

        new double[]
        {
            // rook mg
            73.0837, 76.7994, 86.8507, 93.0932, 117.911, 138.386, 150.377, 151.877,
            22.214, 12.6322, 40.0731, 62.1122, 42.8295, 75.3573, 75.6415, 125.416,
            -1.23566, 17.3843, 18.9049, 33.689, 56.8321, 64.3245, 121.742, 60.0209,
            -26.406, -11.5606, -4.37565, 14.5154, 15.8248, 17.9136, 22.2404, 14.2871,
            -50.2111, -44.8531, -33.3195, -14.5541, -10.8076, -27.3424, -8.22393, -33.6736,
            -58.7634, -40.8041, -35.2768, -28.7447, -21.4048, -22.7094, 9.51707, -20.4757,
            -60.4855, -42.2079, -27.0122, -24.2403, -18.8876, -12.5177, 4.3521, -53.3799,
            -42.1033, -37.2903, -25.826, -4.39719, -6.83281, -14.7228, -25.2115, -60.3547,
        },

        new double[]
        {
            // queen mg
            -16.4215, 20.3271, 60.3854, 84.2727, 92.2453, 122.345, 111.26, 59.0595,
            -4.53845, -27.3996, -19.527, -31.0955, -27.6223, 48.7256, 13.7906, 106.701,
            -1.93788, -4.69022, 1.76194, 12.5473, 27.4496, 58.9425, 79.8284, 51.8179,
            -17.0625, -15.7839, -12.9939, -10.9586, -6.4496, 6.42839, 9.3187, 14.4032,
            -20.8284, -16.1695, -18.669, -11.5489, -10.8557, -8.04399, -0.546742, 3.08306,
            -19.8811, -8.59102, -17.0563, -14.2409, -11.5748, -9.7063, 7.12756, 0.0929689,
            -21.3032, -13.5078, -3.22009, 2.76501, -2.89477, 11.0418, 21.1379, 34.6956,
            -25.1681, -34.8579, -21.6454, -7.62406, -18.7711, -30.8232, -35.1356, -46.6502,
        },

        new double[]
        {
            // king mg
            255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255,
            250, 250, 250, 250, 250, 250, 250, 250,
            200, 200, 200, 200, 200, 200, 200, 200,
            100, 100, 100, 100, 100, 100, 100, 100,
            50, 50, 50, 50, 50, 50, 50, 50,
            0, 0, 0, 0, 5, 0, 0, 0,
        },

        new double[]
        {
            // pawn eg
            0, 0, 0, 0, 0, 0, 0, 0,
            199.013, 188.809, 184.217, 126.342, 118.953, 136.086, 198.237, 204.661,
            129.064, 131.605, 96.9479, 71.3397, 57.7746, 47.3337, 96.1949, 97.0297,
            50.1812, 35.3546, 14.367, 0.378127, -10.3775, -4.46341, 15.2165, 19.6301,
            23.9824, 18.0814, -1.46817, -8.31238, -11.4158, -7.31071, 5.44485, 2.12308,
            16.161, 16.4483, -3.33614, 7.77369, 0.741655, -0.32274, 7.56891, -1.24529,
            23.1157, 23.3556, 13.0435, 19.4205, 16.8357, 10.669, 10.2318, -1.39599,
            0, 0, 0, 0, 0, 0, 0, 0,
        },

        new double[]
        {
            // knight eg
            -71.583, -22.7455, -8.03683, -7.1852, -15.0075, -18.6835, -11.3149, -106.417,
            -16.4222, -1.14319, -2.87988, 7.10191, -5.04904, -19.7784, -10.9051, -36.1621,
            -10.2826, 3.36177, 27.3799, 23.4269, 2.42243, -3.49545, -7.40531, -26.282,
            2.5267, 30.3949, 38.4625, 43.3628, 43.4841, 34.1897, 24.4581, -5.85921,
            6.262, 16.9825, 42.2316, 45.1893, 47.0559, 34.8752, 17.7458, 2.53466,
            -11.8159, 9.83555, 20.6271, 37.6054, 36.1239, 14.5248, 6.02319, -10.252,
            -21.5757, 0.557044, 8.6405, 12.217, 10.4136, 10.7722, -3.06051, -9.46542,
            -27.4266, -45.7788, -7.27847, -0.681708, -4.1684, -5.67711, -42.3937, -38.2809,
        },

        new double[]
        {
            // bishop eg
            2.90811, 18.6727, 15.5149, 28.3894, 25.7681, 16.2544, 5.46303, 3.09751,
            -7.1587, 13.178, 19.7963, 21.87, 18.0747, 3.79452, 19.7465, -5.66824,
            24.0115, 13.1282, 25.4716, 15.7941, 18.9382, 22.9591, 7.62538, 17.5513,
            18.1014, 38.8857, 27.5917, 41.5678, 35.5441, 29.1708, 34.2356, 15.8644,
            13.8656, 29.8152, 43.1069, 33.2101, 33.0957, 37.2773, 27.8752, 5.65148,
            11.754, 24.0382, 30.5738, 31.3739, 37.2181, 31.2987, 16.9619, 5.92803,
            7.09597, 4.09362, 8.33179, 23.4048, 27.932, 14.3295, 15.672, -9.81368,
            -11.1238, 8.57961, -13.0201, 16.4742, 11.6374, 14.6707, -4.66826, -18.7591,
        },

        new double[]
        {
            // rook eg
            29.2931, 34.595, 42.8951, 40.0455, 31.7751, 24.2923, 17.9773, 13.8784,
            32.4698, 51.5157, 50.1022, 41.2313, 42.2002, 35.6946, 28.7975, 3.57071,
            36.7261, 40.1412, 39.5018, 36.8645, 25.2572, 16.5859, 6.93802, 8.48305,
            41.0386, 38.0328, 46.2408, 39.8316, 26.5094, 19.5752, 17.1223, 13.4838,
            36.2086, 40.3285, 42.9382, 38.4065, 35.6854, 35.1276, 21.5749, 21.8747,
            31.3343, 29.1663, 29.2472, 33.2294, 31.1435, 21.9113, 2.94083, 5.50692,
            24.5349, 25.9283, 29.0185, 31.752, 23.0725, 19.3937, 5.42108, 16.4568,
            18.4011, 30.6663, 38.3308, 34.6276, 29.2353, 25.0259, 20.5051, 2.32112,
        },

        new double[]
        {
            // queen eg
            54.6215, 55.0698, 65.8678, 58.1967, 46.7969, 44.7891, 28.3926, 54.3115,
            28.2714, 71.0757, 105.591, 127.009, 138.379, 101.602, 125.702, 48.9196,
            36.8506, 54.5797, 88.9874, 92.8292, 116.23, 89.7448, 39.2841, 49.2614,
            43.3392, 69.0989, 84.8582, 104.289, 122.091, 99.3775, 92.2104, 71.6962,
            41.0285, 69.7698, 76.4439, 98.4154, 93.7278, 81.624, 73.1708, 59.6382,
            34.3306, 32.2364, 69.2774, 60.1078, 68.9585, 69.2579, 44.5297, 32.0347,
            28.7257, 30.3038, 18.4832, 26.5689, 38.1282, 9.96359, -18.8429, -62.1923,
            22.3089, 20.2961, 22.2369, 11.3706, 29.1155, 28.8306, -0.44002, -11.9122,
        },

        new double[]
        {
            // king eg
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
// material:
{ 85.4236, 316.749, 337.808, 458.34, 921.556, 0.0, 
121.757, 356.286, 366.766, 648.048, 1236.63, 0.0 };
// -5.34245, 10.3249 // king on semi-open file
//
// { 82.9626, 313.965, 337.646, 456.181, 926.007, 0.0, 
// 121.646, 357.042, 367.056, 653.142, 1241.98, 0.0 };
//
// { 77.4184, 301.884, 324.512, 435.604, 879.878, 0.0, 
// 109.021, 330.511, 338.584, 593.574, 1129.52, 0.0 };



    public static int[][] tunedPsqts =>
        rawTunedPsqts.Select(psqt => psqt.Select(x => (int)Math.Round(x)).ToArray()).ToArray();
    
    public static int[] tunedPieceValues =>
        rawTunedPieceValues.Select(x => (int)Math.Round(x)).ToArray();
    
    
    // // Eg Values are tuned under the assumption that they are used t
    // public static int[] adjustKingEgTable(int[] tunedKingMgTable, int[] modifiedKingMgTable)

    public record Overflow(
        int amount,
        int piece,
        int square
    );
    
    public static (Decimal[], int, int) createCompressedPsqts()
    {
        // A decimal can store 12 easily accessible bytes, so the 12 * 64 + 6 + 2 * 6 bytes can be stored in 66 decimals
        Decimal[] compresto = new Decimal[66];
        // create a local copy that can be modified and prevents inadvertently trying to modify the global array
        int[][] tunedPsqts = Pesto.tunedPsqts;
        int[] tunedPieceValues = Pesto.tunedPieceValues;
        
        // first, compress the piece square tables
        List<Overflow> overflows = new();
        int numOverflows = 0;
        int maxOverflow = 0;
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
                        Console.WriteLine("Overflow on square {0}, piece {1}, amount {2}", square, table, -psqtEntry);
                        overflows.Add(new (-psqtEntry, table, square));
                        ++numOverflows;
                        maxOverflow = Math.Max(maxOverflow, -psqtEntry);
                        psqtEntry = 0; // clamp to zero
                    }
                    decimalBytes[table] = (byte)(psqtEntry & 0xff);
                }
                compresto[square] = bytesToDecimal();
            }
        }
        Debug.Assert(compresto[64] == 0);
        Debug.Assert(overflows.Count <= 3); // only very few entries should overflow
        Debug.Assert(overflows.Max(x => x.amount) <= 50); // and overflows shouldn't be too large
        
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
        testCompressedEval(compresto, mgScaling, egScaling, overflows);
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
        Console.WriteLine("// Scaling: {0}, {1}", mgScaling, egScaling);
    }
    
    public static void testCompressedEval(Decimal[] compresto, int mgScaling, int egScaling, List<Overflow> overflows)
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
                if (overflows.Any(x => x.piece == piece && x.square == square)) 
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
