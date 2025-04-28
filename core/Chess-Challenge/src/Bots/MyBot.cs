using System;
using System.Linq;
using ChessChallenge.API;

namespace ChessChallenge.Bots;

public class MyBot : IChessBot
{
    static Random rnd = new Random();

    public Move Think(Board board, Timer timer)
    {
        return board.GetLegalMoves().OrderBy(m => rnd.Next()).FirstOrDefault();
    }
}