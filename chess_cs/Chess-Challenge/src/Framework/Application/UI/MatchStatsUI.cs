using Raylib_cs;
using System.Numerics;
using System;
using System.Linq;

namespace ChessChallenge.Application
{
    public static class MatchStatsUI
    {
        public static void DrawMatchStats(ChallengeController controller)
        {
            if (controller.isPlaying &&  !(controller.PlayerWhite.IsHuman ^ controller.PlayerBlack.IsHuman))
            {
                int nameFontSize = UIHelper.ScaleInt(40);
                int regularFontSize = UIHelper.ScaleInt(35);
                int headerFontSize = UIHelper.ScaleInt(45);
                Color col = new(180, 180, 180, 255);
                Vector2 startPos = UIHelper.Scale(new Vector2(1500, 250));
                float spacingY = UIHelper.Scale(35);

                DrawNextText($"Game {controller.CurrGameNumber} of {controller.TotalGameCount}", headerFontSize, Color.WHITE);
                startPos.Y += spacingY * 2;

                DrawStats(controller.BotStatsA);
                startPos.Y += spacingY * 2;
                DrawStats(controller.BotStatsB);
           

                void DrawStats(ChallengeController.BotMatchStats stats)
                {
                    DrawNextText(stats.BotName + ":", nameFontSize, Color.WHITE);
                    DrawNextText($"Score: +{stats.NumWins} ={stats.NumDraws} -{stats.NumLosses}", regularFontSize, Color.WHITE);
                    DrawNextText($"Num Timeouts: {stats.NumTimeouts}", regularFontSize, col);
                    DrawNextText($"Num Illegal Moves: {stats.NumIllegalMoves}", regularFontSize, col);
                    DrawNextText($"Winrate: {(float)stats.NumWins / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, Color.GREEN);
                    DrawNextText($"Draw rate: {(float)stats.NumDraws / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, Color.WHITE);
                    DrawNextText($"Loss rate: {(float)stats.NumLosses / (controller.CurrGameNumber - 1) * 100}%", regularFontSize, Color.RED);
                }
                DrawNextText($"Average moves per game: {controller.trueTotalMovesPlayed / controller.CurrGameNumber - 1}", regularFontSize, Color.WHITE);
           
                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, startPos, fontSize, 1, col);
                    startPos.Y += spacingY;
                }
            } else {
                int regularFontSize = UIHelper.ScaleInt(35);
                Vector2 buttonPos = UIHelper.Scale(new Vector2(1500, 250));
                Vector2 buttonSize = UIHelper.Scale(new Vector2(260, 55));
                float spacing = buttonSize.Y * 1.2f;

                DrawNextText("Bot 1:", regularFontSize, Color.WHITE);
                Dropdown<ChallengeController.PlayerType> dropdown_bot1 = new Dropdown<ChallengeController.PlayerType>(controller.allBots.ToList(), buttonPos, buttonSize);
                buttonPos.Y += spacing * 1.1f;

                DrawNextText("Bot 2:", regularFontSize, Color.WHITE);
                Dropdown<ChallengeController.PlayerType> dropdown_bot2 = new Dropdown<ChallengeController.PlayerType>(controller.allBots.ToList(), buttonPos, buttonSize);
                buttonPos.Y += spacing * 1.1f;

                if (!controller.dropdownOpen_bot1)
                {
                    dropdown_bot2.Draw(ref controller.bot2_index, ref controller.dropdownOpen_bot2);
                }

                if (!controller.dropdownOpen_bot2)
                {
                    dropdown_bot1.Draw(ref controller.bot1_index, ref controller.dropdownOpen_bot1);
                }
                

                void DrawNextText(string text, int fontSize, Color col)
                {
                    UIHelper.DrawText(text, buttonPos, fontSize, 1, col);
                    buttonPos.Y += spacing * 0.5f;
                }
            }
        }
    }
}