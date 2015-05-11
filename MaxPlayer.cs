using System;
using System.Collections.Generic;

namespace SuecaSolver
{
	public class MaxPlayer : Player
	{

		public MaxPlayer(int id, List<Card> hand) : base(id, hand)
		{
		}

		override public int PlayGame(GameState gameState, int alpha, int beta, int lol, Card card = null)
		{
			int v = Int32.MinValue;
			List<Card> moves;

			if (gameState.IsEndGame())
			{
				return gameState.EvalGame();
			}

			if (card == null)
			{
				moves = SuecaGame.PossibleMoves(Hand, gameState.GetLeadSuit());
				moves = gameState.orderPossibleMoves(moves, Id);
			} else {
				moves = new List<Card>();
				moves.Add(card);
			}


			lol++;
			if (lol == 10)
			{
				 System.Environment.Exit(1);
			}


			foreach (Card move in moves)
			{
				gameState.ApplyMove(new Move(Id, move));
				HasSuit[move.Suit]--;
				Console.WriteLine("Max player played " + move);
				int moveValue = gameState.GetNextPlayer().PlayGame(gameState, alpha, beta, lol);

				if (moveValue > v)
				{
					v = moveValue;
				}

				gameState.UndoMove();
				HasSuit[move.Suit]++;

				if (v >= beta)
				{
					return v;
				}

				if (v > alpha)
				{
					alpha = v;
				}
			}

			return v;
		}


		override public int PlayTrick(GameState gameState, int alpha, int beta, Card card = null)
		{
			int v = Int32.MinValue;
			List<Card> moves;

			if (gameState.IsEndFirstTrick())
			{
				return gameState.GetFirstTrickPoints();
			}

			if (card == null)
			{
				moves = SuecaGame.PossibleMoves(Hand, gameState.GetLeadSuit());
			} else {
				moves = new List<Card>();
				moves.Add(card);
			}

			foreach (Card move in moves)
			{
				gameState.ApplyMove(new Move(Id, move));
				int moveValue = gameState.GetNextPlayer().PlayTrick(gameState, alpha, beta);

				if (moveValue > v)
				{
					v = moveValue;
				}

				gameState.UndoMove();

				if (v >= beta)
				{
					return v;
				}

				if (v > alpha)
				{
					alpha = v;
				}
			}

			return v;
		}
	}
}