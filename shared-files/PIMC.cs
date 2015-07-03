using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SuecaSolver
{
    public class PIMC
    {

        public int Execute(InformationSet infoSet, bool USE_CACHE = false)
        {
            infoSet.CleanCardValues();
            List<int> possibleMoves = infoSet.GetPossibleMoves();

            if (possibleMoves.Count == 1)
            {
                return possibleMoves[0];
            }

            int N, depthLimit, handSize = infoSet.GetHandSize();
            setNandDepthLimit(out N, out depthLimit, handSize);
           
            for (int i = 0; i < N; i++)
            {
                List<List<int>> playersHands = infoSet.Sample();

                SuecaGame game;
                int cardValueInTrick;

                for (int j = 0; j < possibleMoves.Count; j++)
                {
                    int card = possibleMoves[j];
                    game = new SuecaGame(handSize, playersHands, infoSet.Trump, infoSet.GetCardsOnTable(), infoSet.BotTeamPoints, infoSet.OtherTeamPoints, USE_CACHE);
                    cardValueInTrick = game.SampleGame(depthLimit, card);
                    infoSet.AddCardValue(card, cardValueInTrick);
                }
            }

            return infoSet.GetHighestCardIndex();
        }


        public int ExecuteWithTimeLimit(InformationSet infoSet, bool USE_CACHE = false)
        {

            infoSet.CleanCardValues();
            List<int> possibleMoves = infoSet.GetPossibleMoves();

            if (possibleMoves.Count == 1)
            {
                return possibleMoves[0];
            }

            int N, depthLimit, handSize = infoSet.GetHandSize();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long time = sw.ElapsedMilliseconds;
            setNandDepthLimit(out N, out depthLimit, handSize);
            for (; time < 2000; )
            {
                List<List<int>> playersHands = infoSet.Sample();

                SuecaGame game;
                int cardValueInTrick;

                for (int j = 0; j < possibleMoves.Count; j++)
                {
                    int card = possibleMoves[j];
                    game = new SuecaGame(handSize, playersHands, infoSet.Trump, infoSet.GetCardsOnTable(), infoSet.BotTeamPoints, infoSet.OtherTeamPoints, USE_CACHE);
                    cardValueInTrick = game.SampleGame(depthLimit, card);
                    infoSet.AddCardValue(card, cardValueInTrick);
                }
                time = sw.ElapsedMilliseconds;
            }

            sw.Stop();
            return infoSet.GetHighestCardIndex();
        }

        static void setNandDepthLimit(out int N, out int depthLimit, int handSize)
        {
            switch (handSize)
            {
                case 10:
                    N = 50;
                    depthLimit = 3;
                    break;
                case 9:
                    N = 50;
                    depthLimit = 3;
                    break;
                case 8:
                    N = 50;
                    depthLimit = 3;
                    break;
                case 7:
                    N = 100;
                    depthLimit = 3;
                    break;
                case 6:
                    N = 5;
                    depthLimit = 10;
                    break;
                case 5:
                    N = 30;
                    depthLimit = 10;
                    break;
                case 4:
                    N = 200;
                    depthLimit = 10;
                    break;
                case 3:
                    N = 2000;
                    depthLimit = 10;
                    break;
                case 2:
                    N = 10000;
                    depthLimit = 10;
                    break;
                default:
                    Console.WriteLine("PIMC.setNandDepthLimit: Invalid handSize.");
                    N = 0;
                    depthLimit = 0;
                    break;
            }
        }
    }
}