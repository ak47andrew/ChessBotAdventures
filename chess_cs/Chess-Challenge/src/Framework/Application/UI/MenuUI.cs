using Raylib_cs;
using System.Numerics;
using System;
using System.IO;
using System.Linq;
using ChessChallenge.Chess;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        public static void DrawButtons(ChallengeController controller)
        {
            Vector2 buttonPos = UIHelper.Scale(new Vector2(260, 210));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(260, 55));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

            // Game Buttons
            if (NextButtonInRow("Play"))
            {
                controller.trueTotalMovesPlayed = 0;
                controller.totalMovesPlayed = 0;
                if (controller.allBots[controller.bot1_index] == ChallengeController.PlayerType.Human ^ 
                    controller.allBots[controller.bot2_index] == ChallengeController.PlayerType.Human)
                {
                    ChallengeController.PlayerType bot;

                    if (controller.allBots[controller.bot1_index] != ChallengeController.PlayerType.Human){
                        bot = controller.allBots[controller.bot1_index];
                    } else {
                        bot = controller.allBots[controller.bot2_index];
                    }
                    
                    var whiteType = controller.HumanWasWhiteLastGame ? bot : ChallengeController.PlayerType.Human;
                    var blackType = !controller.HumanWasWhiteLastGame ? bot : ChallengeController.PlayerType.Human;
                    controller.StartNewGame(whiteType, blackType);
                } else {
                    controller.showDropdowns = false;
                    controller.StartNewBotMatch(controller.allBots[controller.bot1_index], controller.allBots[controller.bot2_index]);
                }
            }

            if (NextButtonInRow("Stop")) {
                controller.EndGame(GameResult.DrawByArbiter, log: false, autoStartNextBotMatch: false);
                controller.showDropdowns = true;
                // controller.PlayerWhite = controller.CreatePlayer(ChallengeController.PlayerType.Human);
            }

            // if (NextButtonInRow("MyBot vs MyBot"))
            // {
            //     controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.MyBot);
            // }
            // if (NextButtonInRow("MyBot vs EvilBot"))
            // {
            //     controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.EvilBot);
            // }

            // Fen manager
            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("Fast"))
            {
                controller.UpdateBotMatchStartFens("Fens fast.txt");
            }
            if (NextButtonInRow("Half"))
            {
                controller.UpdateBotMatchStartFens("Fens half.txt");
            }
            if (NextButtonInRow("Full"))
            {
                controller.UpdateBotMatchStartFens("Fens full.txt");
            }
            if (NextButtonInRow("Test"))
            {
                controller.UpdateBotMatchStartFens("Fens tests.txt");
            }

            // Page buttons
            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("Exit (ESC)"))
            {
                Environment.Exit(0);
            }

            bool NextButtonInRow(string name)
            {
                bool pressed = UIHelper.Button(name, buttonPos, buttonSize);
                buttonPos.Y += spacing;
                return pressed;
            }
        }
    }
}