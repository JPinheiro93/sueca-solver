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
                chosenCard = PIMC.Execute(_id, infoSet, 1, new List<int> { 50, 50, 50, 50, 50, 50, 50, 50, 20 , 20 }, new List<int> { 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000 });
            }

            return chosenCard;
        }
    }
}