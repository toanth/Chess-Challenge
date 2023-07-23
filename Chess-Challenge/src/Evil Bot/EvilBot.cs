using ChessChallenge.API;
using System;
using System.Linq;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {

        Board b;

        public Move Think(Board board, Timer timer)
        {
            b = board;

            var moves = board.GetLegalMoves();
            var bestMove = moves[0];
            var bestScore = float.NegativeInfinity;
            foreach (var move in moves)
            {
                var result = -deepen(move, 3, float.NegativeInfinity, float.PositiveInfinity);
                if (result > bestScore)
                {
                    bestMove = move;
                    bestScore = result;
                    Console.WriteLine("move: " + bestMove.ToString());
                }
            }

            Console.WriteLine("Score: " + bestScore);
            return bestMove;
        }

        // return the score from the point of view of the current player, i.e. a high value
        // means that the player who can choose between `moves` thinks they are winning
        float search_score(Move[] moves, int depth, float alpha, float beta)
        {
            foreach (var move in moves)
            {
                var result = -deepen(move, depth, alpha, beta);
                if (result >= beta) return beta;
                alpha = Math.Max(alpha, result);
            }

            return alpha;
        }

        // return the score from the point of view of the player who can move after `move` has been made,
        // ie a high value means that `move` was probably a bad move because the opponent likes their position
        float deepen(Move move, int depth, float alpha, float beta)
        {
            var score = float.NegativeInfinity;
            b.MakeMove(move);

            if (b.GetLegalMoves().Length == 0)
            {
                if (!b.IsInCheck()) score = 0;
            }
            else if (depth <= 0)
            {
                /*if (depth > -3)
                {
                    var captures = b.GetLegalMoves(true);
                    if (captures.Length > 0) score = Math.Max(alpha, -search_score(captures, depth - 1, -beta, -alpha));
                    else score = eval();

                }
                else*/
                score = eval();
            }
            else
            {
                score = search_score(b.GetLegalMoves(), depth - 1, -beta, -alpha);
            }
            b.UndoMove(move);
            return score;
        }

        int eval()
        {
            return b.GetAllPieceLists().Select(pieceList =>
                pieceList.Count * (int)pieceList.TypeOfPieceInList * (pieceList.IsWhitePieceList == b.IsWhiteToMove ? 1 : -1)).Sum();
        }
        //    // Piece values: null, pawn, knight, bishop, rook, queen, king
        //    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        //    public Move Think(Board board, Timer timer)
        //    {
        //        Move[] allMoves = board.GetLegalMoves();

        //        // Pick a random move to play if nothing better is found
        //        Random rng = new();
        //        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        //        int highestValueCapture = 0;

        //        foreach (Move move in allMoves)
        //        {
        //            // Always play checkmate in one
        //            if (MoveIsCheckmate(board, move))
        //            {
        //                moveToPlay = move;
        //                break;
        //            }

        //            // Find highest value capture
        //            Piece capturedPiece = board.GetPiece(move.TargetSquare);
        //            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

        //            if (capturedPieceValue > highestValueCapture)
        //            {
        //                moveToPlay = move;
        //                highestValueCapture = capturedPieceValue;
        //            }
        //        }

        //        return moveToPlay;
        //    }

        //    // Test if this move gives checkmate
        //    bool MoveIsCheckmate(Board board, Move move)
        //    {
        //        board.MakeMove(move);
        //        bool isMate = board.IsInCheckmate();
        //        board.UndoMove(move);
        //        return isMate;
        //    }
        //}
    }
}