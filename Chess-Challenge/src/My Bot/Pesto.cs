using System;
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
        
    public static int[] pestoPieceValues => new int[]{82, 337, 365, 477, 1025, 100,  94, 281, 297, 512, 936, 100};
    
    public static int[][] pestoPsqts =>
        new int[][]{
            new int[]
            {
                // pawn mg
                0, 0, 0, 0, 0, 0, 0, 0,
                98, 134, 61, 95, 68, 126, 34, -11,
                -6, 7, 26, 31, 65, 56, 25, -20,
                -14, 13, 6, 21, 23, 12, 17, -23,
                -27, -2, -5, 12, 17, 6, 10, -25,
                -26, -4, -4, -10, 3, 3, 33, -12,
                -35, -1, -20, -23, -15, 24, 38, -22,
                0, 0, 0, 0, 0, 0, 0, 0
            },
            new int[]
            {
                // knight mg
                -167, -89, -34, -49, 61, -97, -15, -107,
                -73, -41, 72, 36, 23, 62, 7, -17,
                -47, 60, 37, 65, 84, 129, 73, 44,
                -9, 17, 19, 53, 37, 69, 18, 22,
                -13, 4, 16, 13, 28, 19, 21, -8,
                -23, -9, 12, 10, 19, 17, 25, -16,
                -29, -53, -12, -3, -1, 18, -14, -19,
                -105, -21, -58, -33, -17, -28, -19, -23
            },
            new int[]
            {
                // bishop mg
                -29, 4, -82, -37, -25, -42, 7, -8,
                -26, 16, -18, -13, 30, 59, 18, -47,
                -16, 37, 43, 40, 35, 50, 37, -2,
                -4, 5, 19, 50, 37, 37, 7, -2,
                -6, 13, 13, 26, 34, 12, 10, 4,
                0, 15, 15, 15, 14, 27, 18, 10,
                4, 15, 16, 0, 7, 21, 33, 1,
                -33, -3, -14, -21, -13, -12, -39, -21
            },
            new int[]
            {
                // rook mg
                32, 42, 32, 51, 63, 9, 31, 43,
                27, 32, 58, 62, 80, 67, 26, 44,
                -5, 19, 26, 36, 17, 45, 61, 16,
                -24, -11, 7, 26, 24, 35, -8, -20,
                -36, -26, -12, -1, 9, -7, 6, -23,
                -45, -25, -16, -17, 3, 0, -5, -33,
                -44, -16, -20, -9, -1, 11, -6, -71,
                -19, -13, 1, 17, 16, 7, -37, -26
            },
            new int[]
            {
                // queen mg
                -28, 0, 29, 12, 59, 44, 43, 45,
                -24, -39, -5, 1, -16, 57, 28, 54,
                -13, -17, 7, 8, 29, 56, 47, 57,
                -27, -27, -16, -16, -1, 17, -2, 1,
                -9, -26, -9, -10, -2, -4, 3, -3,
                -14, 2, -11, -2, -5, 2, 14, 5,
                -35, -8, 11, 2, 8, 15, -3, 1,
                -1, -18, -9, 10, -15, -25, -31, -50
            },
            //new int[]
            //{
            //    // king mg
            //    -65, 23, 16, -15, -56, -34, 2, 13,
            //    29, -1, -20, -7, -8, -4, -38, -29,
            //    -9, 24, 2, -16, -20, 6, 22, -22,
            //    -17, -20, -12, -27, -30, -25, -14, -36,
            //    -49, -1, -27, -39, -46, -44, -33, -51,
            //    -14, -14, -22, -46, -44, -30, -15, -27,
            //    1, 7, -8, -64, -43, -16, 9, 8,
            //    -15, 36, 12, -54, 8, -28, 24, 14
            //},
            new int[]
            {
                // king mg, modified
                255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255,
                255, 255, 255, 255, 255, 255, 255, 255,
                250, 250, 250, 250, 250, 250, 250, 250,
                200, 200, 200, 200, 200, 200, 200, 200,
                100, 100, 100, 100, 100, 100, 100, 100,
                10, 10, 50, 50, 50, 50, 30, 10,
                0, 0, 0, 3, 5, 2, 0, 0
            },
            new int[]
            {
                // pawn eg
                0, 0, 0, 0, 0, 0, 0, 0,
                178, 173, 158, 134, 147, 132, 165, 187,
                94, 100, 85, 67, 56, 53, 82, 84,
                32, 24, 13, 5, -2, 4, 17, 17,
                13, 9, -3, -7, -7, -8, 3, -1,
                4, 7, -6, 1, 0, -5, -1, -8,
                13, 8, 8, 10, 13, 0, 2, -7,
                0, 0, 0, 0, 0, 0, 0, 0,
            },
            new int[]
            {
                // knight eg
                -58, -38, -13, -28, -31, -27, -63, -99,
                -25, -8, -25, -2, -9, -25, -24, -52,
                -24, -20, 10, 9, -1, -9, -19, -41,
                -17, 3, 22, 22, 22, 11, 8, -18,
                -18, -6, 16, 25, 16, 17, 4, -18,
                -23, -3, -1, 15, 10, -3, -20, -22,
                -42, -20, -10, -5, -2, -20, -23, -44,
                -29, -51, -23, -15, -22, -18, -50, -64
            },
            new int[]
            {
                // bishop eg
                -14, -21, -11, -8, -7, -9, -17, -24,
                -8, -4, 7, -12, -3, -13, -4, -14,
                2, -8, 0, -1, -2, 6, 0, 4,
                -3, 9, 12, 9, 14, 10, 3, 2,
                -6, 3, 13, 19, 7, 10, -3, -9,
                -12, -3, 8, 10, 13, 3, -7, -15,
                -14, -18, -7, -1, 4, -9, -15, -27,
                -23, -9, -23, -5, -9, -16, -5, -17
            },
            new int[]
            {
                // rook eg
                13, 10, 18, 15, 12, 12, 8, 5,
                11, 13, 13, 11, -3, 3, 8, 3,
                7, 7, 7, 5, 4, -3, -5, -3,
                4, 3, 13, 1, 2, 1, -1, 2,
                3, 5, 8, 4, -5, -6, -8, -11,
                -4, 0, -5, -1, -7, -12, -8, -16,
                -6, -6, 0, 2, -9, -9, -11, -3,
                -9, 2, 3, -1, -5, -13, 4, -20
            },
            new int[]
            {
                // queen eg
                -9, 22, 22, 27, 27, 19, 10, 20,
                -17, 20, 32, 41, 58, 25, 30, 0,
                -20, 6, 9, 49, 47, 35, 19, 9,
                3, 22, 24, 45, 57, 40, 57, 36,
                -18, 28, 19, 47, 31, 34, 39, 23,
                -16, -27, 15, 6, 9, 17, 10, 5,
                -22, -23, -30, -16, -16, -23, -36, -32,
                -33, -28, -22, -43, -5, -32, -20, -41
            },
            new int[]
            {
                // king eg
                -74, -35, -18, -18, -11, 15, 4, -17,
                -12, 17, 14, 17, 17, 38, 23, 11,
                10, 17, 23, 15, 20, 45, 44, 13,
                -8, 22, 24, 27, 26, 33, 26, 3,
                -18, -4, 21, 24, 27, 23, 9, -11,
                -19, -3, 11, 21, 23, 16, 7, -9,
                -27, -11, 4, 13, 14, 4, -5, -17,
                -53, -34, -21, -11, -28, -14, -24, -43
            },
        };

    public static ulong[] createCompressedPesto()
    {
        ulong[] compresto = new ulong[99];
        // create a local copy that can be modified and prevents inadvertently trying to modify the global array
        int[][] pestoPsqts = Pesto.pestoPsqts;
        int[] pestoPieceValues = Pesto.pestoPieceValues;
        
        // first, compress the piece square tables
        int numOverflows = 0;
        for (int table = 0; table < pestoPsqts.Length; ++table)
        {
            int offset = Math.Min(-pestoPsqts[table].Min(), 255 - pestoPsqts[table].Max());
            Debug.Assert(offset >= 0);
            pestoPieceValues[table] -= offset;
            pestoPsqts[table] = pestoPsqts[table].Select(val => val + offset).ToArray();
            for (int row = 0; row < 8; ++row)
            {
                ref ulong entry = ref compresto[row + table * 8];
                entry = 0;
                for (int column = 0; column < 8; ++column)
                {
                    int pestoVal = pestoPsqts[table][row * 8 + column];
                    Debug.Assert(pestoVal <= 0xff);
                    if (pestoVal < 0)
                    {
                        ++numOverflows;
                        pestoVal = 0; // clamp to zero 
                    }
                    entry |= (ulong)(pestoVal & 0xff) << (8 * column);
                }
            }
        }
        Debug.Assert(numOverflows == 1); // only the piece square value for knight on a8 in the middle game overflows
        
        // compress the phase values
        Debug.Assert(piecePhase.Length == 6);
        Debug.Assert(pestoPieceValues.Length == 12);
        compresto[96] = compresto[97] = compresto[98] = 0;
        for (int i = 0; i < piecePhase.Length; ++i)
        {
            Debug.Assert(piecePhase[i] is >= 0 and <= 0xff);
            compresto[96] |= ((ulong)piecePhase[i] & 0xff) << (8 * i);
        }
        
        // compress the piece values
        for (int i = 0; i < pestoPieceValues.Length; ++i) // the king's value doesn't matter
        {
            int compressedPieceVal = pestoPieceValues[i] - (47 << (i % 6));
            Debug.Assert(compressedPieceVal is >= 0 and <= 0xff || i % 6 == 5);
            compresto[i / 8 + 97] |= ((ulong)compressedPieceVal & 0xff) << (8 * (i % 8));
        }
        testCompressedPesto(compresto);
        return compresto;
    }

    // This array contains not only the piece square tables, but also the phase weights and the piece values
    // public static ulong[] compresto = createCompressedPesto();
    
    // uncompress it like this, but replace the `compresto` variable with the hard-coded values
    // (which you can get using the printCompressedPesto() function):
    // public static byte[] pesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();
    
    public static void printCompressedPesto()
    {
        Console.WriteLine("{ " + string.Join(", ", createCompressedPesto()) + " }");
    }
    
    public static void testCompressedPesto(ulong[] compresto)
    {
        byte[] uncompressedPesto = compresto.SelectMany(BitConverter.GetBytes).ToArray();
        for (int i = 0; i < piecePhase.Length; ++i)
        {
            Debug.Assert(piecePhase[i] == uncompressedPesto[768 + i]);
        }

        int[] uncompressedPieceValues = new int[pestoPieceValues.Length];
        Debug.Assert(pestoPieceValues.Length == 12);
        for (int i = 0; i < pestoPieceValues.Length; ++i)
        {
            uncompressedPieceValues[i] = uncompressedPesto[776 + i] + (47 << (i % 6));
        }
        for (int i = 0; i < pestoPieceValues.Length; ++i)
        {
            // pesto piece values are changed by subtracting an offset, so only use >= for comparison
            Debug.Assert(i % 6 == 5 || // the king's piece value doesn't matter
                         pestoPieceValues[i] >= uncompressedPieceValues[i]);
        }
        
        for (int i = 0; i < 64 * 12; ++i)
        {
            int piece = i / 64;
            if (i == 64) // the middle game knight on a8 value can't be represented properly in compressed pesto 
            {
                Debug.Assert(uncompressedPesto[i] == 0);
                Debug.Assert(uncompressedPesto[i] + uncompressedPieceValues[piece] - 41 == pestoPsqts[piece][i % 64]+ pestoPieceValues[piece]);
            } else if (piece % 6 != 5) { // king values differ, but that's fine because the white and black values cancel each other out 
                Debug.Assert(uncompressedPesto[i] + uncompressedPieceValues[piece] == pestoPsqts[piece][i % 64] + pestoPieceValues[piece]);
            }
        }
    }

    // This function is useful for testing that the pesto compression worked. Not meant to be called in the actual bot.
    // The code is adapted from cpw (https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function)
    public static int originalPestoEval(Board board)
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
                mg[piece.IsWhite ? WHITE : BLACK] += pestoPsqts[pc][square] + pestoPieceValues[pc];
                eg[piece.IsWhite ? WHITE : BLACK] += pestoPsqts[pc + 6][square] + pestoPieceValues[pc + 6];
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
