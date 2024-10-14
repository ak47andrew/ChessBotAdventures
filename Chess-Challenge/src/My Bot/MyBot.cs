using System;
using ChessChallenge.API;
using ChessChallenge.Application;


public class MoveEval
{
    public Move move;
    public float eval;

    public MoveEval(Move move, float eval)
    {
        this.move = move;
        this.eval = eval;
    }
}


public class MyBot : IChessBot
{

    static Random rnd = new Random();

    public Move Think(Board board, Timer timer)
    {
        Console.WriteLine("[{0}]", string.Join(", ", board.GetLegalMoves()));
        return Search(board);
    }

    public Move Search(Board board){
        MoveEval? moveEval = null;

        foreach (var move in board.GetLegalMoves())
        {
            ConsoleHelper.Log(move.ToString(), col: ConsoleColor.Cyan);
            board.MakeMove(move);
            float eval = Evaluate(board) * (board.IsWhiteToMove ? -1 : 1);
            ConsoleHelper.Log($"{move}: {eval}", col: ConsoleColor.Cyan);
            if (moveEval == null || eval >= moveEval.eval)
            {
                moveEval = new MoveEval(move, eval);
            }
            board.UndoMove(move);
        }
        ConsoleHelper.Log("------------", col: ConsoleColor.Cyan);
        ConsoleHelper.Log($"Best move: {moveEval.move} - {moveEval.eval}", col: ConsoleColor.Cyan);
        ConsoleHelper.Log("------------", col: ConsoleColor.Cyan);
        return moveEval.move;
    }

    public float Evaluate(Board board) {
        float eval = 0f;

        foreach (var pieceList in board.GetAllPieceLists())
        {
            eval += PieceValue(pieceList.TypeOfPieceInList) * pieceList.Count * (pieceList.IsWhitePieceList ? 1f : -1f);
        }

        if (board.IsInCheckmate()) {
            eval = float.PositiveInfinity * (board.IsWhiteToMove ? -1f : 1f);
        }

        return eval;
    }

    private int PieceValue(PieceType piece) {
        switch (piece)
        {
            case PieceType.Pawn: return 100;
            case PieceType.Knight: return 300;
            case PieceType.Bishop: return 310;
            case PieceType.Rook: return 500;
            case PieceType.Queen: return 900;
            default: return 0;
        }
    }
}
