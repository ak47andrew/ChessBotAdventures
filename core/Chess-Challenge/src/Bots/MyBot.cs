using System;
using System.Linq;
using System.Globalization;
using ChessChallenge.API;
using System.Collections.Generic;

namespace ChessChallenge.Bots;

enum GamePhase
{
    Opening,
    Midgame,
    Endgame
}

public class MyBot : IChessBot
{
    private const int MATE_SCORE = 100000;
    private const int INFINITY = int.MaxValue;

    private static readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>
    {
        {PieceType.Pawn, 100},
        {PieceType.Knight, 320},
        {PieceType.Bishop, 330},
        {PieceType.Rook, 500},
        {PieceType.Queen, 900},
        {PieceType.King, 0},
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
    private Dictionary<(PieceType, bool), int[]> mobilityBonus = new();

    public MyBot()
    {
        Console.WriteLine("Initializing MyBot...");
        int[,] mobilityPawns = new int[8, 8]
        {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            { 60, 60, 60, 60, 60, 60, 60, 60 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 40, 40, 40, 40, 40, 40, 40, 40 },
            { 30, 30, 30, 30, 30, 30, 30, 30 },
            { 20, 20, 20, 20, 20, 20, 20, 20 },
            { 35, 35, 35, 10, 10, 35, 35, 35 },
            {  0,  0,  0,  0,  0,  0,  0,  0 }
        };
        int[,] mobilityKnights = new int[8, 8]
        {
            {  -15,  -10,  -10,  -10,  -10,  -10,  -10,  -15 },
            {  -10,    5,   10,   10,   10,   10,    5,  -10 },
            {  -10,   10,   20,   20,   20,   20,   10,  -10 },
            {  -10,   10,   20,   35,   35,   20,   10,  -10 },
            {  -10,   10,   20,   35,   35,   20,   10,  -10 },
            {  -10,   10,   20,   20,   20,   20,   10,  -10 },
            {  -10,    5,   10,   10,   10,   10,    5,  -10 },
            {  -15,    0,  -10,  -10,  -10,  -10,    0,  -15 }
        };
        int[,] mobilityBishops = new int[8, 8]
        {
            {  -15,  -10,  -10,  -10,  -10,  -10,  -10,  -15 },
            {  -10,    5,   10,   10,   10,   10,    5,  -10 },
            {  -10,   10,   20,   20,   20,   20,   10,  -10 },
            {  -10,   10,   20,   30,   30,   20,   10,  -10 },
            {  -10,   10,   20,   30,   30,   20,   10,  -10 },
            {  -10,   10,   20,   20,   20,   20,   10,  -10 },
            {  -10,   15,   10,   20,   20,   10,   15,  -10 },
            {  -15,  -10,    0,  -10,  -10,    0,  -10,  -15 }
        };
        int[,] mobilityRooks = new int[8, 8]
        {
            {  -15,  -10,   -5,   -5,   -5,   -5,  -10,  -15 },
            {  -10,    0,    0,    0,    0,    0,    0,  -10 },
            {   -5,    0,   10,   10,   10,   10,    0,   -5 },
            {   -5,    0,   10,   20,   20,   10,    0,   -5 },
            {   -5,    0,   10,   20,   20,   10,    0,   -5 },
            {   -5,    0,   10,   10,   10,   10,    0,   -5 },
            {  -10,    0,    0,    5,    5,    0,    0,  -10 },
            {  -15,  -10,   -5,   -5,   -5,   -5,  -10,  -15 }
        };

        int[,] mobilityQueen = new int[8, 8]
        {
            {  -15,  -10,  -10,    0,    0,  -10,  -10,  -15 },
            {  -10,    0,    0,    0,    0,    0,    0,  -10 },
            {  -10,    0,   10,   10,   10,   10,    0,  -10 },
            {   -5,    0,   10,   20,   20,   10,    0,   -5 },
            {   -5,    0,   10,   20,   20,   10,    0,   -5 },
            {  -10,    0,   10,   10,   10,   10,    0,  -10 },
            {  -10,    0,    0,    5,    5,    0,    0,  -10 },
            {  -15,  -10,  -10,    0,    0,  -10,  -10,  -15 }
        };
        int[,] mobilityKing = new int[8, 8]
        {
            {  -50,  -50,  -50,  -50,  -50,  -50,  -50,  -50 },
            {  -50,  -50,  -50,  -50,  -50,  -50,  -50,  -50 },
            {  -30,  -30,  -30,  -30,  -30,  -30,  -30,  -30 },
            {  -20,  -20,  -20,  -20,  -20,  -20,  -20,  -20 },
            {  -10,  -10,  -10,  -10,  -10,  -10,  -10,  -10 },
            {   -5,   -5,   -5,   -5,   -5,   -5,   -5,   -5 },
            {    5,    5,    5,    5,    5,    5,    5,    5 },
            {   40,   40,   40,   10,   10,   40,   40,   40 }
        };

        mobilityBonus[(PieceType.Pawn, true)] = Flatten2DArray(mobilityPawns);
        mobilityBonus[(PieceType.Pawn, false)] = Flatten2DArray(Rotate180(mobilityPawns));
        mobilityBonus[(PieceType.Knight, true)] = Flatten2DArray(mobilityKnights);
        mobilityBonus[(PieceType.Knight, false)] = Flatten2DArray(Rotate180(mobilityKnights));
        mobilityBonus[(PieceType.Bishop, true)] = Flatten2DArray(mobilityBishops);
        mobilityBonus[(PieceType.Bishop, false)] = Flatten2DArray(Rotate180(mobilityBishops));
        mobilityBonus[(PieceType.Rook, true)] = Flatten2DArray(mobilityRooks);
        mobilityBonus[(PieceType.Rook, false)] = Flatten2DArray(Rotate180(mobilityRooks));
        mobilityBonus[(PieceType.Queen, true)] = Flatten2DArray(mobilityQueen);
        mobilityBonus[(PieceType.Queen, false)] = Flatten2DArray(Rotate180(mobilityQueen));
        mobilityBonus[(PieceType.King, true)] = Flatten2DArray(mobilityKing);
        mobilityBonus[(PieceType.King, false)] = Flatten2DArray(Rotate180(mobilityKing));
    }

    public Move Think(Board board, Timer timer)
    {
        GamePhase gamePhase = GetGamePhase(board);
        float timeleft = CalculateTimeLimit(timer, gamePhase, board.PlyCount);
        if (timeleft <= 150)
        {
            Move[] moves = board.GetLegalMoves();
            return moves[random.Next(moves.Length)];
        }
        Console.WriteLine($"info string Starting search with {timeleft / 1000}s search time");
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
                return !move.IsNull ? move : board.GetLegalMoves()[0];
            }
            Console.WriteLine($"info depth {depth} time {timer.MillisecondsElapsedThisTurn} score cp {eval} nodes {nodeCount} nps {(nodeCount / Math.Max(0.0001f, timer.MillisecondsElapsedThisTurn / 1000)).ToString("F0", CultureInfo.InvariantCulture)} pv {move}");
            bestMove = move;
            depth++;
        }
    }

    float CalculateTimeLimit(Timer timer, GamePhase gamePhase, int ply)
    {
        float timeleft = timer.MillisecondsRemaining;
        if (timeleft <= safetyMargin)
        {
            return 0;
        }
        if (timeleft <= 2 * safetyMargin)
        {
            return timeleft - safetyMargin;
        }

        int movesLeft;
        if (gamePhase == GamePhase.Endgame)
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

        GamePhase gamePhase = GetGamePhase(board);

        foreach (PieceList pl in board.GetAllPieceLists())
        {
            foreach (Piece p in pl)
            {
                int sign = p.IsWhite == board.IsWhiteToMove ? 1 : -1;

                // Material evaluation
                score += PieceValues[p.PieceType] * sign;
                if (!(p.PieceType == PieceType.King && gamePhase == GamePhase.Endgame)) {                
                    score += mobilityBonus[(p.PieceType, p.IsWhite)][p.Square.Index] * sign;
                }
            }
        }

        // Penalty for being in check
        if (board.IsInCheck())
            score -= 50;

        score += (int)Math.Round(Math.Pow(board.GetLegalMoves().Count(), (double)2 / 3) * 5);

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

    private GamePhase GetGamePhase(Board board)
    {
        int materialPhase = GetMaterialPhase(board);
        int ply = board.PlyCount;

        if (materialPhase < 13)
        {
            return GamePhase.Endgame;
        }
        else if (materialPhase > 15 && ply < 15)
        {
            return GamePhase.Opening;
        }
        else
        {
            return GamePhase.Midgame;
        }
    }

    public static int[,] Rotate180(int[,] matrix)
    {
        int n = matrix.GetLength(0);
        int[,] result = new int[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i, j] = matrix[n - 1 - i, n - 1 - j];
            }
        }

        return result;
    }

    public static int[] Flatten2DArray(int[,] array2D)
    {
        int rows = array2D.GetLength(0);
        int cols = array2D.GetLength(1);
        int[] array1D = new int[rows * cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array1D[i * cols + j] = array2D[i, j];
            }
        }

        return array1D;
    }
}