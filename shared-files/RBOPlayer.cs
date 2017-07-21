using System;
using System.Collections.Generic;

namespace SuecaSolver
{
    public class RBOPlayer : ArtificialPlayer
    {
        private InformationSet infoSet;

        public RBOPlayer(int id, List<int> initialHand, int trumpCard, int trumpPlayerId)
            : base(id)
        {
            infoSet = new InformationSet(id, initialHand, trumpCard, trumpPlayerId);
        }

        override public void AddPlay(int playerID, int card)
        {
            infoSet.AddPlay(playerID, card);
        }


        override public int Play()
        {
            int chosenCard;

            if (infoSet.GetHandSize() > 10)
            {
                chosenCard = infoSet.RuleBasedDecision();
            }
            else
            {
                //chosenCard = PIMC.Execute(_id, infoSet, 3, new List<int> { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 });
                chosenCard = PIMC.Execute(_id, infoSet, 3, new List<int> { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 });
                //chosenCard = PIMC.Execute(_id, infoSet, 3, new List<int> { 200, 200, 200, 200, 200, 200, 200, 200, 200, 200 });
                //chosenCard = PIMC.Execute(_id, infoSet, 3, new List<int> { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 });
            }

            return chosenCard;
        }

        public int[] GetWinnerAndPointsAndTrickNumber()
        {
            return infoSet.GetWinnerAndPointsAndTrickNumber();
        }

        public int GetCurrentTrickWinner()
        {
            return infoSet.GetCurrentTrickWinner();
        }

        public int GetCurrentTrickPoints()
        {
            return infoSet.GetCurrentTrickPoints();
        }

        public bool HasNewTrickWinner()
        {
            return infoSet.HasNewTrickWinner();
        }

        public bool HasNewTrickTeamWinner()
        {
            return infoSet.HasNewTrickTeamWinner();
        }

        public int GetTrickIncrease()
        {
            return infoSet.GetTrickIncrease();
        }
        
        public float PointsPercentage()
        {
            float alreadyMadePoints = infoSet.MyTeamPoints + infoSet.OtherTeamPoints;
            if (alreadyMadePoints == 0.0f)
            {
                return 0.5f;
            }
            return infoSet.MyTeamPoints / alreadyMadePoints;
        }

        public int GetHandSize()
        {
            return infoSet.GetHandSize();
        }

        public string GetLastPlayInfo()
        {
            return infoSet.GetLastPlayInfo();
        }

        public bool IsLastPlayOfTrick()
        {
            return infoSet.IsLastPlayOfTrick();
        }
    }
}