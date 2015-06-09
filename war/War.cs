using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuecaSolver
{
    public class War
    {

        static bool checkHands(List<List<int>> hands, int trump)
        {
            for (int i = 0; i < hands.Count; i++)
            {
                int points = 0;
                int trumps = 0;
                for (int j = 0; j < hands[i].Count; j++)
                {
                    int card = hands[i][j];
                    points += Card.GetValue(card);
                    if (Card.GetSuit(card) == trump)
                    {
                        trumps++;
                    }
                }

                if (trumps == 0 && points < 10)
                {
                    return false;
                }
            }
            return true;
        }

        static int[] processGames(int i, int[] localCount, int gameMode)
        {
            //int seed = -1150905530;
            //int seed = -373600003;
            int seed = Guid.NewGuid().GetHashCode();
            Random randomNumber = new Random(seed);
            string[] playersNames = new string[4];
            ArtificialPlayer[] players = new ArtificialPlayer[4];
            List<List<int>> playersHands;

            Deck deck = new Deck(randomNumber, seed);
            int trump = randomNumber.Next(0, 4);
            playersHands = deck.SampleHands(new int[] { 10, 10, 10, 10 });
            while (!checkHands(playersHands, trump))
            {
                playersHands = deck.SampleHands(new int[] { 10, 10, 10, 10 });
                localCount[3]++;
            }
            //Console.WriteLine("LOL: " + Guid.NewGuid().GetHashCode());

            //SuecaGame.PrintCards("p0", playersHands[0]);
            //SuecaGame.PrintCards("p1", playersHands[1]);
            //SuecaGame.PrintCards("p2", playersHands[2]);
            //SuecaGame.PrintCards("p3", playersHands[3]);
            //Console.WriteLine("----------------------------------------------");
            SuecaGame game = new SuecaGame(10, playersHands, trump, null, 0, 0);
            int currentPlayerID = i % 4;
            
            switch (gameMode)
            {
                case 1:
                    playersNames[0] = "Bot1";
                    players[0] = new SmartPlayer(0, playersHands[0], trump, randomNumber, seed);
                    playersNames[1] = "Random1";
                    players[1] = new RandomPlayer(1, playersHands[1], randomNumber);
                    playersNames[2] = "Random2";
                    players[2] = new RandomPlayer(2, playersHands[2], randomNumber);
                    playersNames[3] = "Random3";
                    players[3] = new RandomPlayer(3, playersHands[3], randomNumber);
                    break;
                case 2:
                    playersNames[0] = "Bot1";
                    players[0] = new SmartPlayer(0, playersHands[0], trump, randomNumber, seed);
                    playersNames[1] = "Random1";
                    players[1] = new RandomPlayer(1, playersHands[1], randomNumber);
                    playersNames[2] = "Bot2";
                    players[2] = new SmartPlayer(2, playersHands[2], trump, randomNumber, seed);
                    playersNames[3] = "Random2";
                    players[3] = new RandomPlayer(3, playersHands[3], randomNumber);
                    break;
                case 3:
                    playersNames[0] = "Bot1";
                    players[0] = new SmartPlayer(0, playersHands[0], trump, randomNumber, seed);
                    playersNames[1] = "Bot2";
                    players[1] = new SmartPlayer(1, playersHands[1], trump, randomNumber, seed);
                    playersNames[2] = "Bot3";
                    players[2] = new SmartPlayer(2, playersHands[2], trump, randomNumber, seed);
                    playersNames[3] = "Random1";
                    players[3] = new RandomPlayer(3, playersHands[3], randomNumber);
                    break;
                case 4:
                    playersNames[0] = "Bot1";
                    players[0] = new SmartPlayer(0, playersHands[0], trump, randomNumber, seed);
                    playersNames[1] = "Bot2";
                    players[1] = new SmartPlayer(1, playersHands[1], trump, randomNumber, seed);
                    playersNames[2] = "Bot3";
                    players[2] = new SmartPlayer(2, playersHands[2], trump, randomNumber, seed);
                    playersNames[3] = "Bot4";
                    players[3] = new SmartPlayer(3, playersHands[3], trump, randomNumber, seed);
                    break;
                default:
                    break;
            }

            for (int j = 0; j < 40; j++)
            {
                int chosenCard = players[currentPlayerID].Play();
                game.PlayCard(currentPlayerID, chosenCard);
                for (int k = 0; k < 4; k++)
                {
                    if (k != currentPlayerID)
                    {
                        players[k].AddPlay(currentPlayerID, chosenCard);
                    }
                }
                currentPlayerID = game.GetNextPlayerId();
            }
            int[] points = game.GetGamePoints();
            //Console.WriteLine("[" + System.Threading.Thread.CurrentThread.ManagedThreadId + "] ----------------- Game " + i + " -----------------");
            //Console.WriteLine("Team " + playersNames[0] + " and " + playersNames[2] + " - " + points[0] + " points");
            //Console.WriteLine("Team " + playersNames[1] + " and " + playersNames[3] + " - " + points[1] + " points");
            //Console.Out.Flush();

            if (points[0] == 60)
            {
                localCount[0]++;
            }
            else if (points[0] > 60)
            {
                localCount[1]++;
            }
            else
            {
                localCount[2]++;
            }
            return localCount;
        }

        public static void Main()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int numGames, gameMode, firstTeamWins = 0, secondTeamWins = 0, draws = 0, badGames = 0;
            Console.WriteLine("");
            Console.WriteLine("|||||||||||||||||||| SUECA TEST WAR ||||||||||||||||||||");
            Console.WriteLine("");
            Console.WriteLine("[1] - 1 Bot 3 Random");
            Console.WriteLine("[2] - 2 Bot 2 Random");
            Console.WriteLine("[3] - 3 Bot 1 Random");
            Console.WriteLine("[4] - 4 Bot 0 Random");
            Console.Write("Choose an option from 1 to 4: ");
            gameMode = 2;
            Console.WriteLine(gameMode);
            Console.Write("How many games: ");
            numGames = 500;
            Console.WriteLine(numGames);


            Parallel.For(0, numGames,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () => new int[4],

                (int i, ParallelLoopState state, int[] localCount) =>
                {
                    return processGames(i, localCount, gameMode);
                },

                (int[] localCount) =>
                {
                    draws += localCount[0];
                    firstTeamWins += localCount[1];
                    secondTeamWins += localCount[2];
                    badGames += localCount[3];
                });

            //for (int i = 0; i < numGames; i++)
            //{
            //    int[] localCount = new int[4];
            //    processGames(i, localCount, gameMode);
            //    draws += localCount[0];
            //    firstTeamWins += localCount[1];
            //    secondTeamWins += localCount[2];
            //    badGames += localCount[3];
            //}

            Console.WriteLine("");
            Console.WriteLine("----------------- Summary -----------------");
            Console.WriteLine("Team 0 won " + firstTeamWins + "/" + numGames);
            Console.WriteLine("Team 1 won " + secondTeamWins + "/" + numGames);
            Console.WriteLine("Draws " + draws + "/" + numGames);
            Console.WriteLine("BadGames " + badGames);
            
            sw.Stop();
            Console.WriteLine("Total Time taken by functions is {0} seconds", sw.ElapsedMilliseconds / 1000); //seconds
            Console.WriteLine("Total Time taken by functions is {0} minutes", sw.ElapsedMilliseconds / 60000); //minutes
        }
    }
}