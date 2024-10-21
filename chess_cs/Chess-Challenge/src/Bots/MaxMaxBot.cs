namespace ChessChallenge.Bots;
using ChessChallenge.API;
using System;
using System.Linq;


public class MaxMaxBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        float board_eval_us = AllPieceCounter(board, false, board.IsWhiteToMove);
        if (board_eval_us < 6 && timer.MillisecondsRemaining > 10000)
        {
            return BestMove(board, 4, timer).Item2;
        }
        else if (board_eval_us < 8 && timer.MillisecondsRemaining > 5000)
        {
            return BestMove(board, 3, timer).Item2;
        }
        return BestMove(board, 2, timer).Item2;
    }

    public (float, Move) BestMove(Board board, int depth, Timer timer)
    {

        Random rnd = new Random();
        Move[] moves = board.GetLegalMoves().OrderBy(a => rnd.Next()).ToArray();
        float besteval = -1000;


        Move bestmove = moves[0];
        foreach (var move in moves)
        {
            if (timer.MillisecondsElapsedThisTurn > 4000 * (((float)timer.MillisecondsRemaining) / 60000))
            {
                if (depth > 1)
                {
                    depth = 1;
                }
            }
            if (timer.MillisecondsRemaining < 100)
            {
                break;
            }
            board.MakeMove(move);
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                return (10000, move);
            }

            if (!board.IsInCheckmate() && board.GetLegalMoves().Length == 0)
            {
                depth = 0;
            }

            if (depth > 0)
            {
                (float eval, Move opponent_move) = BestMove(board, depth - 1, timer);


                if ((-eval) > besteval)
                {
                    besteval = -eval;
                    bestmove = move;
                }
            }
            else
            {
                float final_eval = EvaluationFunction(board, !board.IsWhiteToMove);
                if (final_eval > besteval)
                {
                    besteval = final_eval;
                    bestmove = move;
                }
            }
            board.UndoMove(move);
        }
        return (besteval, bestmove);
    }

    public float EvaluationFunction(Board board, bool iswhite)
    {
        float evalboard_white = BoardEvaluator(board, true);
        float evalboard_black = BoardEvaluator(board, false);


        float final_eval;
        if (iswhite)
        {
            final_eval = evalboard_white - evalboard_black;
        }
        else
        {
            final_eval = evalboard_black - evalboard_white;

        }

        if (board.IsInCheck())
        {
            final_eval += 1.0f;
        }

        if (board.IsInCheckmate())
        {
            final_eval = -10000;
        }

        // Tis be stalemate
        if (!board.IsInCheckmate() && board.GetLegalMoves().Length == 0)
        {
            final_eval = 0;
        }

        return final_eval;
    }
    public float[] Pawn_Ranks_Early = { 1, 1, 1.2f, 1.4f, 1.4f, 1.2f, 1, 1 };
    public float[] Pawn_Columns_Early = { 1, 1.1f, 1.2f, 1.3f, 1.3f, 1.2f, 1.1f, 1 };
    public float[] Pawn_Ranks_Late = { 1, 1, 1.1f, 1.2f, 1.4f, 1.7f, 1.9f, 3f };
    public float[] Pawn_Columns_Late = { 1.3f, 1.2f, 1.1f, 1f, 1f, 1.1f, 1.2f, 1.3f };


    public float BoardEvaluator(Board board, bool white)
    {
        float evalualtion = 0.0f;
        PieceList pawns = board.GetPieceList(PieceType.Pawn, white);
        for (int i = 0; i < pawns.Count; i++)
        {
            Piece pawn = pawns.GetPiece(i);
            if (AllPieceCounter(board, false, true) > 12)
            {
                evalualtion += Pawn_Ranks_Early[pawn.Square.Rank - 1] * Pawn_Columns_Early[pawn.Square.Index % 8] * 0.76f;
            }
            else
            {
                if (white)
                {
                    evalualtion += Pawn_Ranks_Late[pawn.Square.Rank - 1] * Pawn_Columns_Late[pawn.Square.Index % 8] * 0.8f;
                }
                else
                {
                    evalualtion += Pawn_Ranks_Late[8 - pawn.Square.Rank] * Pawn_Columns_Late[pawn.Square.Index % 8] * 0.8f;
                }
            }

        }
        evalualtion += 2.9f * board.GetPieceList(PieceType.Knight, white).Count;
        evalualtion += 3f * board.GetPieceList(PieceType.Bishop, white).Count;
        evalualtion += 5f * board.GetPieceList(PieceType.Rook, white).Count;
        evalualtion += 9f * board.GetPieceList(PieceType.Queen, white).Count;
        return evalualtion;

    }

    public int AllPieceCounter(Board board, bool bothcol, bool white = true)
    {
        int sum = 0;
        if (bothcol)
        {
            PieceList[] allpieces = board.GetAllPieceLists();
            foreach (PieceList piecelist in allpieces)
            {
                sum += piecelist.Count;
            }
        }
        else
        {
            PieceList[] allpieces = board.GetAllPieceLists();
            foreach (PieceList piecelist in allpieces)
            {
                if (piecelist.IsWhitePieceList && white)
                {
                    sum += piecelist.Count;
                }
                else if (!piecelist.IsWhitePieceList && !white)
                {
                    sum += piecelist.Count;
                }
            }
        }
        return sum;
    }
}