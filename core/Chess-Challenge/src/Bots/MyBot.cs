using System;
using System.Linq;
using ChessChallenge.API;

namespace ChessChallenge.Bots;

public class MyBot : IChessBot
{
    public class MoveEval
    {
        public Move move;
        public int eval;

        public MoveEval(Move move, int eval)
        {
            this.move = move;
            this.eval = eval;
        }
    }

    static Random rnd = new Random();

    public Move Think(Board board, Timer timer)
    {
        var m = Search(board, 4);
        Console.WriteLine("------");
        return m;
    }

    public Move Search(Board board, int depth)
    {
        MoveEval? moveEval = null;
        var moves = board.GetLegalMoves().OrderBy(_ => rnd.Next()).ToArray();

        foreach (var move in moves)
        {
            board.MakeMove(move);
            int eval = -DeepSearch(board, depth - 1);
            Console.WriteLine($"{move} -> {eval}");
            bool update = moveEval == null || eval >= moveEval.eval;

            if (update)
                moveEval = new MoveEval(move, eval);

            board.UndoMove(move);
        }
        return moveEval?.move ?? moves[0];
    }

    public int DeepSearch(Board board, int depth)
    {
        var moves = board.GetLegalMoves().OrderBy(_ => rnd.Next()).ToArray();

        if (depth == 0 || moves.Length == 0)
            return Evaluate(board); // Corrected: Removed negation here

        int bestEval = int.MinValue;

        foreach (var move in moves)
        {
            board.MakeMove(move);
            int eval = -DeepSearch(board, depth - 1);
            board.UndoMove(move);

            if (eval > bestEval)
                bestEval = eval;
        }
        return bestEval;
    }

    public int Evaluate(Board board)
    {
        if (board.IsInCheckmate())
        {
            // Return very low score for checkmated player
            int mateScore = -1000000 + 1000 * board.PlyCount;
            return mateScore;
        }

        if (board.IsDraw())
            return 0;

        int eval = 0;
        foreach (var pieceList in board.GetAllPieceLists())
        {
            // Corrected sign calculation: == instead of !=
            int sign = pieceList.IsWhitePieceList == board.IsWhiteToMove ? 1 : -1;
            eval += PieceValue(pieceList.TypeOfPieceInList) * pieceList.Count * sign;
        }

        if (board.IsInCheck())
            eval -= 50;

        return eval;
    }

    private static int PieceValue(PieceType piece)
    {
        return piece switch
        {
            PieceType.Pawn => 100,
            PieceType.Knight => 300,
            PieceType.Bishop => 300,
            PieceType.Rook => 500,
            PieceType.Queen => 900,
            _ => 0,
        };
    }
}