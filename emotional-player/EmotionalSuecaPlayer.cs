﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thalamus;
using SuecaSolver;
using SuecaMessages;
using SuecaTypes;
using EmoteCommonMessages;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using AssetManagerPackage;
using WellFormedNames;
using Utilities;
using IntegratedAuthoringTool.DTOs;
using EmotionalAppraisal.OCCModel;

namespace EmotionalPlayer
{
    public interface ISuecaPublisher : IThalamusPublisher, ISuecaActions, IFMLSpeech { }

    class EmotionalSuecaPlayer : ThalamusClient, ISuecaPerceptions
    {
        private IntegratedAuthoringToolAsset _iat;
        private Dictionary<string, RolePlayCharacterAsset> _rpc = new Dictionary<string, RolePlayCharacterAsset>();
        private List<Name> _events = new List<Name>();
        private Object rpcLock = new Object();
        private Object iatLock = new Object();
        private int i;
        public ISuecaPublisher SuecaPub;
        private RBOPlayer ai;
        private int id;
        private int nameId;
        private Random randomNumberGenerator;
        private string[] tags;
        private string[] meanings;
        private string _agentType;
        private List<Utterance> usedUtterances = new List<Utterance>();
        private bool robotHasPlayed = false;
        private bool initialyzing;
        private const string LOGS_PATH = "../../../Scenarios/Logs";

        public EmotionalSuecaPlayer(string clientName, string path, string type, string charactersNames = "") : base(clientName, charactersNames)
        {
            try
            {
                nameId = Int16.Parse("" + clientName[clientName.Length - 1]);
            }
            catch (Exception)
            {
                nameId = 1;
            }

            SetPublisher<ISuecaPublisher>();
            SuecaPub = new SuecaPublisher(Publisher);
            ai = null;
            randomNumberGenerator = new Random(System.Guid.NewGuid().GetHashCode());
            AssetManager.Instance.Bridge = new AssetManagerBridge();
            LoadScenario(new ScenarioData(path));
            _agentType = type;
            initialyzing = false;

        }

        public struct ScenarioData
        {
            public readonly string ScenarioPath;
            private IntegratedAuthoringToolAsset _iat;

            public IntegratedAuthoringToolAsset IAT { get { return _iat; } }

            public ScenarioData(string path)
            {
                ScenarioPath = path;
                _iat = IntegratedAuthoringToolAsset.LoadFromFile(ScenarioPath);
            }
        }

        private struct Utterance
        {
            public string text { get; set; }
            public int uses { get; set; }

            public Utterance(string utterance, int repetitions)
            {
                text = utterance;
                uses = repetitions;
            }

        }

        private void LoadScenario(ScenarioData data)
        {
            System.IO.Directory.CreateDirectory(LOGS_PATH);
            _iat = data.IAT;

            
            var characterSources = _iat.GetAllCharacterSources().ToList();
            foreach (var source in characterSources)
            {
                RolePlayCharacterAsset character = RolePlayCharacterAsset.LoadFromFile(source.Source);
                character.LoadAssociatedAssets();
                lock (iatLock)
                {
                    _iat.BindToRegistry(character.DynamicPropertiesRegistry);
                }
                lock (rpcLock)
                {
                    _rpc.Add(character.BodyName.ToString(), character);
                }
            }
            Task.Run(() => { UpdateCoroutine(); });
        }

        #region Emotional Agent

        private void UpdateCoroutine()
        {
            string currentBelief;
            lock (rpcLock)
            {
                currentBelief = _rpc[_agentType].GetBeliefValue("DialogueState(Board)");
            }
            while (currentBelief != "SessionEnd")
            {
                lock (rpcLock)
                {
                    _rpc[_agentType].Update();
                }

                Thread.Sleep(500);
                lock (rpcLock)
                {
                    currentBelief = _rpc[_agentType].GetBeliefValue("DialogueState(Board)");
                }
            }
        }

        private void PerceiveOnly()
        {
            lock (rpcLock)
            {
                foreach (var item in _events)
                {
                    Console.WriteLine(item.ToString());
                }
                _rpc[_agentType].Perceive(_events);
                _events.Clear();
            }
        }
        

        private void PerceiveAndDecide(string[] tags, string[] tagMeanings)
        {
            IEnumerable<ActionLibrary.IAction> actionRpc = null;

            lock (rpcLock)
            {
                //PERCEIVE PHASE
                foreach (var item in _events)
                {
                    Console.WriteLine(item.ToString());
                }
                _rpc[_agentType].Perceive(_events);
                _events.Clear();

                //DECIDE PHASE            
                actionRpc = _rpc[_agentType].Decide();
                Console.WriteLine("Agent's Mood: " + _rpc[_agentType].Mood);
                /*Console.WriteLine("Emotions being felt");
                foreach(var emotion in _rpc[_agentType].GetAllActiveEmotions())
                {
                    Console.WriteLine("\t\t" + emotion.Type);
                }*/
                //if (_rpc[_agentType].GetAllActiveEmotions().IsEmpty())
                //{
                //    Console.WriteLine("No active emnotions!");
                //}
                //else
                //{
                //    if (_rpc[_agentType].GetAllActiveEmotions().Where(em => em.Type == OCCEmotionType.Joy.Name || em.Type == OCCEmotionType.Distress.Name).IsEmpty())
                //    {
                //        Console.WriteLine("NO WELLBEING emnotions!");
                //    }
                //    if (_rpc[_agentType].GetAllActiveEmotions().Where(em => em.Type == OCCEmotionType.Shame.Name || em.Type == OCCEmotionType.Pride.Name || em.Type == OCCEmotionType.Reproach.Name || em.Type == OCCEmotionType.Admiration.Name).IsEmpty())
                //    {
                //        Console.WriteLine("NO ATTRIBUTION emnotions!");
                //    }
                //}
            }

            //ACTION PHASE
            if (actionRpc == null || actionRpc.IsEmpty())
            {
                Console.WriteLine("No action");
                return;
            }
            else
            {
                foreach (var item in actionRpc)
                {
                    foreach (var par in item.Parameters)
                    {
                        Console.WriteLine(" act_par: " + par);
                    }
                    Console.WriteLine("");
                    Console.WriteLine("----------------");
                }
                ActionLibrary.IAction chosenAction = actionRpc.FirstOrDefault();
                lock (rpcLock)
                {
                    _rpc[_agentType].SaveToFile(LOGS_PATH + "/log" + i + ".rpc");
                }
                i++;
                    
                switch (chosenAction.Key.ToString())
                {
                    case "Speak":
                        Name currentState = chosenAction.Parameters[0];
                        Name nextState = chosenAction.Parameters[1];
                        Name meaning = chosenAction.Parameters[2];
                        Name style = chosenAction.Parameters[3];

                        lock (iatLock)
                        {
                            
                                var dialogs = _iat.GetDialogueActions(IATConsts.AGENT, currentState, nextState, meaning, style);
                                var dialog = checkUsedUtterances(dialogs);
                                Console.WriteLine(dialog);
                                SuecaPub.PerformUtteranceWithTags("", dialog, tags, tagMeanings);
                        }

                        tags = new string[] { };
                        tagMeanings = new string[] { };
                        break;
                    case "Animation":
                        Name state = chosenAction.Parameters[0];
                        Name emotionName = chosenAction.Parameters[1];
                        if(emotionName == (Name) "positive" || emotionName == (Name)"negative")
                        {
                            Console.WriteLine("[ANIMATION] Soft reaction to " + state + " with " + emotionName + " mood");
                        }
                        else
                            Console.WriteLine("[ANIMATION] Soft reaction to " + state + " with the emotion " + emotionName);
                        break;
                    default:
                        Console.WriteLine("Unknown Action");
                        break;
                }
            }
        }

        private void AddActionEndEvent(string subject, string actionName, string target)
        {
            lock (rpcLock)
            {
                _events.Add(EventHelper.ActionEnd(subject, actionName, target));
            }
        }

        private void AddPropertyChangeEvent(string propertyName, string value, string subject)
        {
            lock (rpcLock)
            {
                _events.Add(EventHelper.PropertyChange(propertyName, value, subject));
            }
        }

        #endregion

        #region Sueca Publisher
        private class SuecaPublisher : ISuecaPublisher
        {
            dynamic publisher;

            public SuecaPublisher(dynamic publisher)
            {
                this.publisher = publisher;
            }

            public void Play(int id, string card, string playInfo)
            {
                this.publisher.Play(id, card, playInfo);
            }

            public void CancelUtterance(string id)
            {
                this.publisher.CancelUtterance(id);
            }

            public void PerformUtterance(string id, string utterance, string category)
            {
                this.publisher.PerformUtterance(id, utterance, category);
            }

            public void PerformUtteranceFromLibrary(string id, string category, string subcategory, string[] tagNames, string[] tagValues)
            {
                this.publisher.PerformUtteranceFromLibrary(id, category, subcategory, tagNames, tagValues);
            }

            public void PerformUtteranceWithTags(string id, string utterance, string[] tagNames, string[] tagValues)
            {
                this.publisher.PerformUtteranceWithTags(id, utterance, tagNames, tagValues);
            }
        }
        #endregion

        #region Game Actions

        #region Game Actions - Before or after the game

        public void SessionStart(int sessionId, int numGames, int[] agentsIds, int shouldGreet)
        {
            id = agentsIds[nameId - 1];
            Console.WriteLine("My id is " + id);
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "SessionStart", "Board");
            PerceiveAndDecide(new string[] { }, new string[] { });
        }

        public void GameStart(int gameId, int playerId, int teamId, string trumpCard, int trumpCardPlayer, string[] cards)
        {
            initialyzing = true;
            List<int> initialCards = new List<int>();
            foreach (string cardSerialized in cards)
            {
                SuecaTypes.Card card = JsonSerializable.DeserializeFromJson<SuecaTypes.Card>(cardSerialized);
                SuecaSolver.Rank myRank = (SuecaSolver.Rank)Enum.Parse(typeof(SuecaSolver.Rank), card.Rank.ToString());
                SuecaSolver.Suit mySuit = (SuecaSolver.Suit)Enum.Parse(typeof(SuecaSolver.Suit), card.Suit.ToString());
                int myCard = SuecaSolver.Card.Create(myRank, mySuit);
                initialCards.Add(myCard);
            }
            SuecaTypes.Card sharedTrumpCard = JsonSerializable.DeserializeFromJson<SuecaTypes.Card>(trumpCard);
            SuecaSolver.Rank trumpRank = (SuecaSolver.Rank)Enum.Parse(typeof(SuecaSolver.Rank), sharedTrumpCard.Rank.ToString());
            SuecaSolver.Suit trumpSuit = (SuecaSolver.Suit)Enum.Parse(typeof(SuecaSolver.Suit), sharedTrumpCard.Suit.ToString());
            int myTrumpCard = SuecaSolver.Card.Create(trumpRank, trumpSuit);

            ai = new RBOPlayer(playerId, initialCards, myTrumpCard, trumpCardPlayer);
            initialyzing = false;
        }

        public void Shuffle(int playerId)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "Shuffle", "Board");
            if (playerId == 3)
            {
                AddPropertyChangeEvent(Consts.PLAY_SHUFFLE, "Agent", "Board");
            }
            else
            {
                AddPropertyChangeEvent(Consts.PLAY_SHUFFLE, "Other", "Board");
            }
            PerceiveAndDecide(new string[] { "|playerId1|", "|playerId2|" }, new string[] { "0", "2" });
        }

        public void Cut(int playerId)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "Cut", "Board");
            if (playerId == 3)
            {
                AddPropertyChangeEvent(Consts.TRICK_CUT, "Agent", "Board");
            }
            else
            {
                AddPropertyChangeEvent(Consts.TRICK_CUT, "Other", "Board");
            }
            PerceiveAndDecide(new string[] {"|playerId1|","|playerId2|" }, new string[] {"0","2"});
        }

        public void Deal(int playerId)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "Deal", "Board");
            if (playerId == 3)
            {
                AddPropertyChangeEvent(Consts.TRICK_CUT, "Agent", "Board");
            }
            else
            {
                AddPropertyChangeEvent(Consts.TRICK_CUT, "Other", "Board");
            }
            PerceiveAndDecide(new string[] { "|playerId1|", "|playerId2|" }, new string[] { "0", "2" });
        }

        public void ReceiveRobotCards(int playerId)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "ReceiveCards", "Board");
        }

        public void TrumpCard(string trumpCard, int trumpCardPlayer)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "TrumpCard", "Board");
            PerceiveAndDecide(new string[] { "|playerId|" }, new string[] { trumpCardPlayer.ToString() });
        }

        public void Renounce(int playerId)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "GameEnd", "Board");
            AddPropertyChangeEvent(Consts.TRICK_RENOUNCE, checkTeam(playerId), checkTeam(playerId));

            PerceiveAndDecide(new string[] { }, new string[] { });
        }

        public void GameEnd(int team0Score, int team1Score)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "GameEnd", "Board");
            
            if (team0Score == 120)
            {
                AddPropertyChangeEvent(Consts.END_GAME, "LostQuad", "Board");
            }
            else if (team0Score > 90)
            {
                AddPropertyChangeEvent(Consts.END_GAME, "LostDouble", "Board");
            }
            else if (team0Score > 60)
            {
                AddPropertyChangeEvent(Consts.END_GAME, "LostSingle", "Board");
            }

            if (team1Score == 120)
            {
                AddPropertyChangeEvent(Consts.END_GAME, "WinQuad", "Board");
            }
            else if (team1Score > 90)
            {
                AddPropertyChangeEvent(Consts.END_GAME, "WinDouble", "Board");
            }
            else if (team1Score > 60)
            {
                AddPropertyChangeEvent(Consts.END_GAME, "WinSingle", "Board");
            }

            if (team0Score == team1Score)
            {
                AddPropertyChangeEvent(Consts.END_GAME, "Draw", "Board");
            }

            PerceiveAndDecide(new string[] { }, new string[] { });
        }

        public void SessionEnd(int sessionId, int team0Score, int team1Score)
        {
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "SessionEnd", "Board");
            if (team0Score > team1Score)
            {
                AddPropertyChangeEvent(Consts.END_SESSION, "Lost", "Board");
            }
            if (team0Score < team1Score)
            {
                AddPropertyChangeEvent(Consts.END_SESSION, "Win", "Board");
            }
            if (team0Score == team1Score)
            {
                AddPropertyChangeEvent(Consts.END_SESSION, "Draw", "Board");
            }

            PerceiveAndDecide(new string[] { }, new string[] { });
        }

        #endregion

        #region Game Actions - During the game

        public void NextPlayer(int id)
        {
            AddPropertyChangeEvent(Consts.NEXT_PLAYER, checkTeam(id), "Board");
            Console.WriteLine("The next player is {0}.", id);
            SuecaTypes.Rank msgRank = new SuecaTypes.Rank();
            SuecaTypes.Suit msgSuit = new SuecaTypes.Suit();

            //If a GameStart event has been received but not fully proccessed wait
            while (initialyzing) { }
            
            if (this.id == id && ai != null)
            {
                //Console.WriteLine("I am going to play...");

                int chosenCard = ai.Play();
                ai.AddPlay(id, chosenCard);

                SuecaSolver.Rank chosenCardRank = (SuecaSolver.Rank)SuecaSolver.Card.GetRank(chosenCard);
                SuecaSolver.Suit chosenCardSuit = (SuecaSolver.Suit)SuecaSolver.Card.GetSuit(chosenCard);
                msgRank = (SuecaTypes.Rank)Enum.Parse(typeof(SuecaTypes.Rank), chosenCardRank.ToString());
                msgSuit = (SuecaTypes.Suit)Enum.Parse(typeof(SuecaTypes.Suit), chosenCardSuit.ToString());
                string cardSerialized = new SuecaTypes.Card(msgRank, msgSuit).SerializeToJson();


                string playInfo = ai.GetLastPlayInfo();
                SuecaPub.Play(this.id, cardSerialized, playInfo);
                AddPropertyChangeEvent(Consts.PLAY_INFO, playInfo, "Board");
                Console.WriteLine(":::::::::::::::::::::::::::::::::::::::::::: Robot has played {0} - {1}.", SuecaSolver.Card.ToString(chosenCard), playInfo);
                //Console.WriteLine("PlayInfo: " + playInfo);
                AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "Playing", "Board");
                //Console.WriteLine("My play has been sent.");

                int currentPlayPoints = ai.GetCurrentTrickPoints();
                bool hasNewTrickWinner = ai.HasNewTrickTeamWinner();

                AddPropertyChangeEvent(Consts.TRICK_SCORE, currentPlayPoints.ToString(), checkTeam(id));

                if (hasNewTrickWinner)
                {
                    int currentWinnerID = ai.GetCurrentTrickWinner();
                    string lastPlayInfo = ai.GetLastPlayInfo();
                    if (lastPlayInfo == Sueca.PLAY_INFO_NEWTRICK)
                    {
                        AddPropertyChangeEvent(Consts.TRICK_WINNER, checkTeam(currentWinnerID), Sueca.PLAY_INFO_NEWTRICK);
                    }
                    else
                    {
                        AddPropertyChangeEvent(Consts.TRICK_WINNER, checkTeam(currentWinnerID), checkTeam(id));
                    }
                }

                int trickIncrease = ai.GetTrickIncrease();

                if (trickIncrease > 0)
                {
                    AddPropertyChangeEvent(Consts.TRICK_INCREASE_PROPERTY, trickIncrease.ToString(), checkTeam(id));
                }

                //PerceiveOnly();
                PerceiveAndDecide(new string[] { "|rank|", "|suit|", "|nextPlayerId|", "|playerId1|", "|playerId2|" }, new string[] { convertRankToPortuguese(msgRank.ToString()), convertSuitToPortuguese(msgSuit.ToString()), id.ToString(), "0", "2" });
                robotHasPlayed = true;
        }
            else
            {
                // Only speak NextPlayer dialogues when the next player is not himself
                Thread.Sleep(randomNumberGenerator.Next(2000, 3000));
                AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "NextPlayer", "Board");
                PerceiveAndDecide(new string[] { "|rank|", "|suit|", "|nextPlayerId|", "|playerId1|", "|playerId2|" }, new string[] { convertRankToPortuguese(msgRank.ToString()), convertSuitToPortuguese(msgSuit.ToString()), id.ToString(), "0", "2" });
            }
        }

        public void Play(int id, string card, string playInfo)
        {
            //Console.WriteLine("Player {0} is playing.", id);
            AddPropertyChangeEvent(Consts.CURRENT_PLAYER, checkTeam(id), "Board");

            if (ai != null && id != this.id)
            {
                SuecaTypes.Card c = JsonSerializable.DeserializeFromJson<SuecaTypes.Card>(card);
                SuecaSolver.Rank myRank = (SuecaSolver.Rank)Enum.Parse(typeof(SuecaSolver.Rank), c.Rank.ToString());
                SuecaSolver.Suit mySuit = (SuecaSolver.Suit)Enum.Parse(typeof(SuecaSolver.Suit), c.Suit.ToString());
                int myCard = SuecaSolver.Card.Create(myRank, mySuit);

                ai.AddPlay(id, myCard);
                Console.WriteLine(":::::::::::::::::::::::::::::::::::::::::::: Player {0} played {1}.", id, SuecaSolver.Card.ToString(myCard));

                AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "Play", "Board");

                int currentPlayPoints = ai.GetCurrentTrickPoints();
                bool hasNewTrickWinner = ai.HasNewTrickTeamWinner();
                bool lastPlayOfTrick = ai.IsLastPlayOfTrick();

                AddPropertyChangeEvent(Consts.TRICK_SCORE, currentPlayPoints.ToString(), checkTeam(id));

                if (hasNewTrickWinner)
                {
                    int currentWinnerID = ai.GetCurrentTrickWinner();
                    string lastPlayInfo = ai.GetLastPlayInfo();
                    if (lastPlayInfo == Sueca.PLAY_INFO_NEWTRICK)
                    {
                        AddPropertyChangeEvent(Consts.TRICK_WINNER, checkTeam(currentWinnerID), Sueca.PLAY_INFO_NEWTRICK);
                    }
                    else
                    {
                        AddPropertyChangeEvent(Consts.TRICK_WINNER, checkTeam(currentWinnerID), checkTeam(id));
                    }
                }

                //if (robotHasPlayed && !lastPlayOfTrick)
                if(!lastPlayOfTrick)
                {
                    Console.WriteLine("NAO E A ULTIMA JOGADA DA RONDA");
                    int trickIncrease = ai.GetTrickIncrease();

                    if (trickIncrease > 0)
                    {
                        AddPropertyChangeEvent(Consts.TRICK_INCREASE_PROPERTY, trickIncrease.ToString(), checkTeam(id));
                    }
                    tags = new string[] { "|rank|", "|suit|", "|playerId|", "|playerId1|", "|playerId1|" };
                    meanings = new string[] { convertRankToPortuguese(myRank.ToString()), convertSuitToPortuguese(mySuit.ToString()), id.ToString(), "0", "2" };
                    PerceiveAndDecide(tags, meanings);
                }
                else
                {
                    Console.WriteLine("EEE A ULTIMA JOGADA DA RONDA");
                    PerceiveOnly();
                }
            }
        }

        public void TrickEnd(int winnerId, int trickPoints)
        {
            robotHasPlayed = false;
            AddPropertyChangeEvent(Consts.DIALOGUE_STATE_PROPERTY, "TrickEnd", "Board");
            AddPropertyChangeEvent(Consts.TRICK_END, trickPoints.ToString(), checkTeam(winnerId));
            //PerceiveOnly();
            PerceiveAndDecide(new string[] {"|playerId|","|trickpoints|"}, new string[] {winnerId.ToString(),trickPoints.ToString()});
        }

        public void ResetTrick()
        {

        }

        #endregion

        #endregion

        #region Auxiliary Methods

        private string convertRankToPortuguese(string englishRank)
        {
            string portugueseRank = "";
            switch (englishRank)
            {
                case "Two":
                    portugueseRank = "um dois";
                    break;
                case "Three":
                    portugueseRank = "um três";
                    break;
                case "Four":
                    portugueseRank = "um quatro";
                    break;
                case "Five":
                    portugueseRank = "um cinco";
                    break;
                case "Six":
                    portugueseRank = "um seis";
                    break;
                case "Queen":
                    portugueseRank = "uma dama";
                    break;
                case "Jack":
                    portugueseRank = "um váléte";
                    break;
                case "King":
                    portugueseRank = "um rei";
                    break;
                case "Seven":
                    portugueseRank = "uma manilha";
                    break;
                case "Ace":
                    portugueseRank = "um ás";
                    break;
                default:
                    break;
            }
            return portugueseRank;
        }

        private string convertSuitToPortuguese(string englishSuit)
        {
            string portugueseSuit = "";
            switch (englishSuit)
            {
                case "Clubs":
                    portugueseSuit = "paus";
                    break;
                case "Diamonds":
                    portugueseSuit = "ouros";
                    break;
                case "Hearts":
                    portugueseSuit = "copas";
                    break;
                case "Spades":
                    portugueseSuit = "espadas";
                    break;
                default:
                    break;
            }
            return portugueseSuit;
        }

        private string checkTeam(int id)
        {
            string subject = "";
            switch (_agentType)
            {
                case "Group":
                    if (id == 1 || id == 3)
                        //Agent Team
                        subject = "T1";
                    if (id == 0 || id == 2)
                        //Opponent Team
                        subject = "T0";
                    break;
                case "Individual":
                    switch (id)
                    {
                        case 0:
                            subject = "P0";
                            break;
                        case 1:
                            subject = "P1";
                            break;
                        case 2:
                            subject = "P2";
                            break;
                        case 3:
                            subject = "P3";
                            break;
                        default:
                            Console.WriteLine("Unknown Player ID");
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Unknown Agent Type is playing");
                    break;
            }
            return subject;
        }

        private bool isPartner(int id)
        {
            return id == 1;
        }

        /// <summary>
        /// Checks if any of the dialogs is contained within a list of used dialogues.
        /// If not used yet it just returns that dialog and adds it to the used dialogue list.
        /// If been used it goes into a list of candidates to be reused (after 5 uses it becomes free again).
        /// If no unused dialogues, the method returns a random dialog with the least uses.
        /// </summary>
        /// <param name="dialogs">List of possible dialogues</param>
        /// <returns>An unused dialogue or, alternatively, a least used dialogue</returns>
        private string checkUsedUtterances(List<DialogueStateActionDTO> dialogs)
        {
            List<Utterance> candidates = new List<Utterance>();

            foreach (DialogueStateActionDTO dialog in dialogs)
            {
                int i = usedUtterances.FindIndex(o => string.Equals(dialog.Utterance, o.text, StringComparison.OrdinalIgnoreCase));

                if (i == -1)
                {
                    usedUtterances.Add(new Utterance(dialog.Utterance, 1));
                    return dialog.Utterance;
                }

                if (i > -1)
                {
                    if (usedUtterances[i].uses <= 5)
                    {
                        var temp = usedUtterances[i];
                        candidates.Add(temp);
                    }
                    else if (usedUtterances[i].uses > 5)
                    {
                        usedUtterances.RemoveAt(i);
                    }
                }
            }
            int min = candidates.Min(x => x.uses);
            var result = candidates.Where(t => t.uses == min).Shuffle().FirstOrDefault();
            int j = usedUtterances.FindIndex(o => string.Equals(result.text, o.text, StringComparison.OrdinalIgnoreCase));
            usedUtterances.RemoveAt(j);
            usedUtterances.Add(new Utterance(result.text, ++result.uses));

            return result.text;
        }
        #endregion
    }
}
