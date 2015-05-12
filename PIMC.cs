using System;
using System.Collections.Generic;

namespace SuecaSolver
{
	public class PIMC
	{

		private int N;

		public PIMC(int n)
		{
			N = n;
		}

		public Card Execute(InformationSet infoSet)
		{
			int n = N;
			infoSet.CleanCardValues();
			List<Card> possibleMoves = infoSet.GetPossibleMoves();

			if (possibleMoves.Count == 1)
			{
				return possibleMoves[0];
			}

			for (int i = 0; i < n; i++)
			{
				List<List<Card>> players = infoSet.Sample();
				List<Card> p0 = players[0];
				List<Card> p1 = players[1];
				List<Card> p2 = players[2];
				List<Card> p3 = players[3];

				SuecaGame game = new SuecaGame(p0, p1, p2, p3, infoSet.Trump, infoSet.GetJustPlayed(), false);

				for (int cardValueInTrick, j = 0; j < possibleMoves.Count; j++)
				{
					Card card = possibleMoves[j];

					if (p0.Count > 5)
					{
						n = 1000;
						// n = 1;
						cardValueInTrick = game.SampleTrick(card);
					}
					else
					{
						n = 100;
						// n = 1;
						cardValueInTrick = game.SampleGame(card);
					}

					infoSet.AddCardValue(card, cardValueInTrick);
				}
			}

			// infoSet.PrintInfoSet();
			Card highestCard = infoSet.GetHighestCardIndex();
			return highestCard;
		}


		public void ExecuteTestVersion(InformationSet infoSet, List<Card> hand, int num)
		{
			List<Card> possibleMoves = infoSet.GetPossibleMoves();
			SuecaGame game;

			if (possibleMoves.Count == 1)
			{
				Console.WriteLine("Only one move available: " + possibleMoves[0]);
				return;
			}

			if (num == 10)
			{
				List<List<Card>> players = infoSet.Sample();
				List<Card> p0 = players[0];
				List<Card> p1 = players[1];
				List<Card> p2 = players[2];
				List<Card> p3 = players[3];
				game = new SuecaGame(p0, p1, p2, p3, infoSet.Trump, infoSet.GetJustPlayed(), false);
			}
			else {
				List<List<Card>> players = infoSet.SampleThree(num);
				List<Card> p0 = hand;
				List<Card> p1 = players[0];
				List<Card> p2 = players[1];
				List<Card> p3 = players[2];
				game = new SuecaGame(p0, p1, p2, p3, infoSet.Trump, infoSet.GetJustPlayed(), false);

				SuecaGame.PrintCards("p0", p0);
				SuecaGame.PrintCards("p1", p1);
				SuecaGame.PrintCards("p2", p2);
				SuecaGame.PrintCards("p3", p3);
			}


			// for (int j = 0; j < possibleMoves.Count; j++)
			// {
				Card card = possibleMoves[5];
				int cardValueInTrick = game.SampleGame(card);
				// int cardValueInTrick = game.SampleTrick(card);
				// Console.WriteLine("cardValueInTrick - " + card + " " + cardValueInTrick);
				infoSet.AddCardValue(card, cardValueInTrick);
			// }

			infoSet.PrintInfoSet();
		}
	}
}