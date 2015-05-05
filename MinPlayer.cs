using System;
using System.Collections.Generic;

namespace SuecaSolver
{
	public class MinPlayer : Player
	{

		public MinPlayer(int id, List<Card> hand) : base(id, hand)
		{
		}

		override public int PlayGame(GameState gameState, int alpha, int beta, Card card = null)
		{
			int worstMove = Int32.MaxValue;
			List<Card> moves;

			if (gameState.IsEndGame())
			{
				return gameState.EvalGame();
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
				int moveValue = gameState.GetNextPlayer().PlayGame(gameState, alpha, beta);

				if (moveValue < worstMove)
				{
					worstMove = moveValue;
					if (moveValue < beta)
					{
						beta = moveValue;
					}
				}

				gameState.UndoMove();

				if (beta <= alpha)
				{
					return beta;
				}
			}

			return worstMove;
		}


		override public int PlayTrick(GameState gameState, int alpha, int beta, Card card = null)
		{
			int worstMove = Int32.MaxValue;
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

				if (moveValue < worstMove)
				{
					worstMove = moveValue;
					if (moveValue < beta)
					{
						beta = moveValue;
					}
				}

				gameState.UndoMove();

				if (beta <= alpha)
				{
					return beta;
				}
			}

			return worstMove;
		}
	}
}