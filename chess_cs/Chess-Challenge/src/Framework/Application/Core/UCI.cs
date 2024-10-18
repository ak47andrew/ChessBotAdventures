using ChessChallenge.API;
using ChessChallenge.Application;
using ChessChallenge.Application.APIHelpers;
using ChessChallenge.Chess;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ChessChallenge.UCI
{
    class UCIBot
    {
        IChessBot bot;
        ChallengeController.PlayerType type;
        Chess.Board board;
        APIMoveGen moveGen;
        static readonly string fenRegex = @"fen\s+([rnbqkpRNBQKP1-8]+\/[rnbqkpRNBQKP1-8]+\/[rnbqkpRNBQKP1-8]+\/[rnbqkpRNBQKP1-8]+\/[rnbqkpRNBQKP1-8]+\/[rnbqkpRNBQKP1-8]+\/[rnbqkpRNBQKP1-8]+\/[rnbqkpRNBQKP1-8]+\s+\w\s(?:\w+|-)\s(?:\w+|-)\s\d+\s\d+|startpos)";
        static readonly string movesRegex = @"moves\s+(\w+(?:\s+\w+)*)";


		public UCIBot(IChessBot bot, ChallengeController.PlayerType type)
        {
            this.bot = bot;
            this.type = type;
            moveGen = new APIMoveGen();
            board = new Chess.Board();
        }

        void PositionCommand(string[] args)
        {
            string s_args = string.Join(" ", args);

            Match fenMatch = Regex.Match(s_args, fenRegex, RegexOptions.IgnoreCase);
            Match movesMatch = Regex.Match(s_args, movesRegex, RegexOptions.IgnoreCase);

            if (fenMatch.Success) {
                string fen = fenMatch.Groups[1].Value;
                if (fen != "startpos") {
                    board.LoadPosition(fen);
                } else {
                    board.LoadStartPosition();
                }
            } else {
                board.LoadStartPosition();
            }

            if (movesMatch.Success) {
                string[] moveStrings = movesMatch.Groups[1].Value.Split(' ');
                foreach (var moveString in moveStrings)
                {
                    API.Move move = new API.Move(moveString, new API.Board(board));
                    board.MakeMove(new Chess.Move(move.RawValue), false);
                }
            }

            // TODO: rewrite this in the morning
            // int idx = Array.FindIndex(args, x => x == "moves");
            // if (idx == -1)
            // {
            //     if (args[1] == "startpos")
            //     {
            //         board.LoadStartPosition();
            //     }
            //     else
            //     {
            //         board.LoadPosition(String.Join(" ", args.AsSpan(2, args.Length - 2).ToArray()));
            //     }
            // }
            // else
            // {
            //     if (args[1] == "startpos")
			// 	{
			// 		board.LoadStartPosition();
			// 	}
            //     else
			// 	{
			// 		board.LoadPosition(String.Join(" ", args.AsSpan(1, idx - 1).ToArray()));
			// 	}

            //     for (int i = idx + 1; i < args.Length; i++)
            //     {
            //         // this is such a hack
            //         API.Move move = new API.Move(args[i], new API.Board(board));
            //         board.MakeMove(new Chess.Move(move.RawValue), false);
            //     }
            // }

            // string fen = FenUtility.CurrentFen(board);
        }

        void GoCommand(string[] args)
        {
            int wtime = 0, btime = 0;
            API.Board apiBoard = new API.Board(board);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "wtime")
                {
                    wtime = Int32.Parse(args[i + 1]);
                }
                else if (args[i] == "btime")
                {
                    btime = Int32.Parse(args[i + 1]);
                }
            }
            if (!apiBoard.IsWhiteToMove)
            {
                int tmp = wtime;
                wtime = btime;
                btime = tmp;
            }
            Timer timer = new Timer(wtime, btime, 0);
            API.Move move = bot.Think(apiBoard, timer);
            Log($"bestmove {move.ToString().Substring(7, move.ToString().Length - 8)}");
        }

        void ExecCommand(string line)
        {
            // default split by whitespace
            var tokens = line.Split();

            if (tokens.Length == 0)
                return;

            switch (tokens[0])
            {
                case "uci":
                    Log("id name Chess Challenge");
                    Log("id author AspectOfTheNoob, Sebastian Lague");
                    Log("uciok");
                    break;
                case "ucinewgame":
                    bot = ChallengeController.CreateBot(type);
                    break;
                case "position":
                    PositionCommand(tokens);
                    break;
                case "isready":
                    Log("readyok");
                    break;
                case "go":
                    GoCommand(tokens);
                    break;
            }
        }

        public void Run()
        {
            while (true)
            {
                string line = Console.ReadLine();
                Log(line, false);

                if (line == "quit" || line == "exit")
                    return;
                ExecCommand(line);
            }
        }

        public void Log(string text, bool cout = true)
        {
            if (cout)
            {
                Console.WriteLine(text);
            }
            File.AppendAllText("file.txt", (cout ? "> " : "< ") + text + Environment.NewLine);
        }
    }
}