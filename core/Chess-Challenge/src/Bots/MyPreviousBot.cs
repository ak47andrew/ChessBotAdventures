using System;
using System.Linq;
using System.Globalization;
using ChessChallenge.API;
using System.Collections.Generic;

namespace ChessChallenge.Bots;

enum PrevGamePhase
{
    Opening,
    Midgame,
    Endgame
}

public class MyPreviousBot : IChessBot
{
    private const int MATE_SCORE = 100000;
    private const int INFINITY = 1000000;    

    private static readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>
    {
        {PieceType.Pawn, 100},
        {PieceType.Knight, 320},
        {PieceType.Bishop, 330},
        {PieceType.Rook, 500},
        {PieceType.Queen, 900},
        {PieceType.King, 20000},
        {PieceType.None, 0}
    };
    private static readonly Dictionary<PieceType, int> PieceValuesPhase = new Dictionary<PieceType, int>
    {
        {PieceType.Pawn, 0},
        {PieceType.Knight, 1},
        {PieceType.Bishop, 1},
        {PieceType.Rook, 2},
        {PieceType.Queen, 4},
        {PieceType.King, 0},
        {PieceType.None, 0}
    };
    private bool isTimeOut = false;
    private const float safetyMargin = 100;
    private static Random random = new Random();
    private Move bestMove;
    private int nodeCount;

    public Move Think(Board board, Timer timer)
    {
        float timeleft = CalculateTimeLimit(timer, GetGamePhase(board), board.PlyCount);
        if (timeleft <= 150)
        {
            Move[] moves = board.GetLegalMoves();
            return moves[random.Next(moves.Length)];
        }
        Console.WriteLine($"Starting search with {timeleft / 1000}s search time");
        bestMove = Move.NullMove;
        isTimeOut = false;
        nodeCount = 0;
        int depth = 1;
        while (true)
        {
            (Move move, int eval) = FindBestMove(board, depth, timer, timeleft);
            if (move == Move.NullMove)
            {
                move = bestMove;
            }
            if (isTimeOut)
            {
                Console.WriteLine($"Time out. Playing {move}");
                return !move.IsNull ? move : board.GetLegalMoves()[0];
            }
            Console.WriteLine($"info depth {depth} time {timer.MillisecondsElapsedThisTurn} score cp {eval} nodes {nodeCount} nps {(nodeCount / Math.Max(0.0001f, timer.MillisecondsElapsedThisTurn / 1000)).ToString("F0", CultureInfo.InvariantCulture)} pv {move}");
            bestMove = move;
            // Console.WriteLine($"Depth: {depth}, Move: {move}, Eval: {eval}, time left: {(timeleft - timer.MillisecondsElapsedThisTurn) / 1000}s");
            depth++;
        }
    }

    float CalculateTimeLimit(Timer timer, PrevGamePhase gamePhase, int ply)
    {
        float timeleft = timer.MillisecondsRemaining;
        if (timeleft <= safetyMargin)
        {
            return 0;
        }
        else if (timeleft <= 2 * safetyMargin)
        {
            return timeleft - safetyMargin;
        }

        int movesLeft;
        if (gamePhase == PrevGamePhase.Endgame)
        {
            movesLeft = Math.Max(2, Math.Min(20, (int)Math.Round(timeleft / 5.0)));
        }
        else
        {
            movesLeft = 40 - (ply / 2);
        }

        float baseTime = timeleft / movesLeft;
        float timeWithIncrement = baseTime + (timer.IncrementMilliseconds * 0.8f);

        return Math.Min(
            Math.Min(
                timeWithIncrement,
                timeleft * 0.75f
            ),
            Math.Min(
                timeleft - safetyMargin,
                Math.Max(50, timeWithIncrement)
            )
        );
    }

    public (Move, int) FindBestMove(Board board, int depth, Timer timer, float timelimit)
    {
        Move bestMove = Move.NullMove;
        int bestValue = -INFINITY;
        int alpha = -INFINITY;
        int beta = INFINITY;

        // Generate and order moves
        Move[] legalMoves = board.GetLegalMoves();
        List<Move> orderedMoves = OrderMoves(legalMoves, board);

        foreach (Move move in orderedMoves)
        {
            if (timer.MillisecondsElapsedThisTurn >= timelimit)
            {
                isTimeOut = true;
                return (bestMove, bestValue);
            }
            board.MakeMove(move);
            int value = -Negamax(board, depth - 1, -beta, -alpha, timer, timelimit);
            if (isTimeOut)
            {
                return (bestMove, bestValue);
            }
            board.UndoMove(move);

            if (value > bestValue)
            {
                bestValue = value;
                bestMove = move;
            }

            if (value > alpha)
                alpha = value;

            if (alpha >= beta)
                break;
        }

        return (bestMove, bestValue);
    }

    private int Negamax(Board board, int depth, int alpha, int beta, Timer timer, float timelimit)
    {
        if (board.IsInCheckmate())
            return -MATE_SCORE - depth;

        if (board.IsDraw())
            return 0;

        if (depth == 0)
        {
            ++nodeCount;
            return Evaluate(board);
        }

        int bestValue = -INFINITY;
        Move[] legalMoves = board.GetLegalMoves();
        List<Move> orderedMoves = OrderMoves(legalMoves, board);

        foreach (Move move in orderedMoves)
        {
            if (timer.MillisecondsElapsedThisTurn >= timelimit)
            {
                isTimeOut = true;
                return bestValue;
            }

            board.MakeMove(move);
            int value = -Negamax(board, depth - 1, -beta, -alpha, timer, timelimit);
            if (isTimeOut)
            {
                return value;
            }
            board.UndoMove(move);

            bestValue = Math.Max(bestValue, value);
            alpha = Math.Max(alpha, value);

            if (alpha >= beta)
                break;
        }

        return bestValue;
    }

    private int Evaluate(Board board)
    {
        int score = 0;

        // Material evaluation
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            int sign = pieceList.IsWhitePieceList == board.IsWhiteToMove ? 1 : -1;
            score += PieceValues[pieceList.TypeOfPieceInList] * pieceList.Count * sign;
        }

        // Pawn positional evaluation
        List<Piece> pawns = board.GetPieceList(PieceType.Pawn, true).ToList();
        pawns.AddRange(board.GetPieceList(PieceType.Pawn, false).ToList());
        foreach (Piece pawn in pawns)
        {
            int sign = pawn.IsWhite == board.IsWhiteToMove ? 1 : -1;
            int rank = pawn.Square.Rank;

            // Adjust for player perspective
            if (pawn.IsWhite)
                score += sign * rank;       // White pawns advance upward
            else
                score += sign * (7 - rank); // Black pawns advance downward
        }

        // Penalty for being in check
        if (board.IsInCheck())
            score -= 50;

        return score;
    }

    private List<Move> OrderMoves(Move[] moves, Board board)
    {
        return moves.OrderByDescending(m => MoveScore(m, board)).ToList();
    }

    private int MoveScore(Move move, Board board)
    {
        int score = 0;

        if (move == bestMove) // Search move from the previous search first to safely play if search is interrupted by timeout
        {
            return INFINITY;
        }

        // Prioritize captures
        if (move.IsCapture)
        {
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            score += 10 * PieceValues[capturedPiece.PieceType];
        }

        // Prioritize promotions
        if (move.IsPromotion)
            score += PieceValues[PieceType.Queen];

        return score;
    }

    private int GetMaterialPhase(Board board)
    {
        int totalMaterial = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
            totalMaterial += PieceValuesPhase[pieceList.TypeOfPieceInList] * pieceList.Count;

        return totalMaterial;
    }

    private PrevGamePhase GetGamePhase(Board board)
    {
        int materialPhase = GetMaterialPhase(board);
        int ply = board.PlyCount;

        if (materialPhase < 13)
        {
            return PrevGamePhase.Endgame;
        }
        else if (materialPhase > 15 && ply < 15)
        {
            return PrevGamePhase.Opening;
        }
        else
        {
            return PrevGamePhase.Midgame;
        }
    }
}