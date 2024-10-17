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
        return Search(board, 4);
    }

    public Move Search(Board board, int depth){
        MoveEval? moveEval = null;

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            float eval = -DeepSearch(board, depth - 1);
            if (
                moveEval == null || // If first accurance 
                (board.IsWhiteToMove && eval >= moveEval.eval) || // Or more, if playing white 
                (!board.IsWhiteToMove && eval <= moveEval.eval)) // Or less, if playing black
            {
                moveEval = new MoveEval(move, eval); // update the move
            }
            board.UndoMove(move);
        }
        return moveEval.move;
    }

    public float DeepSearch(Board board, int depth){
        if (depth == 0) {
            return Evaluate(board);
        }

        float bestEval = board.IsWhiteToMove ? float.NegativeInfinity : float.PositiveInfinity;
        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            float eval = DeepSearch(board, depth - 1);
            board.UndoMove(move);
            if (board.IsWhiteToMove)
            {
                bestEval = Math.Max(bestEval, eval);
            } else {
                bestEval = Math.Min(bestEval, eval);
            }
        }
        return bestEval;
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
