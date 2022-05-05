using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace Battleship
{
    class BattleshipRunner : MonoBehaviour
    {
        [SerializeField]
        string[] args;
        [SerializeField]
        BattleshipEngine engine;
        [SerializeField]
        float delayBetweenPlayers = 0.25f;

        void Start()
        {
            List<BattleshipAgent> battleshipAgents = GameUtilities.CreateAgentList(args);

            if (battleshipAgents.Count == 2)
            {
                BattleshipAgent player1 = battleshipAgents[0];
                BattleshipAgent player2 = battleshipAgents[1];
                //BattleshipEngine engine = new BattleshipEngine(player1, player2);
                engine.SetAgents(player1, player2);
                string winner = engine.PlaySingleGame(true, delayBetweenPlayers);
                Debug.Log(winner);
            }
            else if (battleshipAgents.Count > 2)
            {
                int agentCount = battleshipAgents.Count;
                List<(int, int)> gameList = GameUtilities.GenerateGameList(agentCount);

                Dictionary<string, TournamentStanding> standings = new Dictionary<string, TournamentStanding>();
                foreach (BattleshipAgent agent in battleshipAgents)
                {
                    string name = agent.Nickname;
                    standings[name] = new TournamentStanding(name);
                }

                System.Random rng = new System.Random();
                while (gameList.Count > 0)
                {
                    (int, int) thisGame = GameUtilities.GetRandomGame(gameList, rng);
                    BattleshipAgent player1 = battleshipAgents[thisGame.Item1];
                    BattleshipAgent player2 = battleshipAgents[thisGame.Item2];

                    if (player2.Nickname == "Bozo the Clown")
                    {
                        string title = $"-= {player1.Nickname} vs. Bozo the Clown =-";
                        int padding = (80 - title.Length) / 2;
                        title = title.PadLeft(padding + title.Length);

                        Console.Clear();
                        Console.Write($"\n\n{title}");
                        //Console.ReadKey();
                        BattleshipEngine engine = new BattleshipEngine(player1, player2);
                        engine.PlaySingleGame(true, delayBetweenPlayers);
                        //Console.ReadKey();
                    }
                    else
                    {
                        string matchup = $"{player1.Nickname} vs {player2.Nickname}: ";
                        Console.Write($"{matchup,-30}");
                        BattleshipEngine engine = new BattleshipEngine(player1, player2);
                        int p1wins = 0;
                        int p2wins = 0;
                        int ties = 0;

                        for (int i = 0; i < 50; i++)
                        {
                            string winner = engine.PlaySingleGame(false, 0);
                            if (winner == null)
                            {
                                standings[player1.Nickname].T++;
                                standings[player2.Nickname].T++;
                                ties++;
                            }
                            else if (winner == player1.Nickname)
                            {
                                standings[player1.Nickname].W++;
                                standings[player2.Nickname].L++;
                                p1wins++;
                            }
                            else if (winner == player2.Nickname)
                            {
                                standings[player1.Nickname].L++;
                                standings[player2.Nickname].W++;
                                p2wins++;
                            }
                        }
                        Debug.Log($"{p1wins} to {p2wins} ({ties} ties)");
                    }
                }

                //Console.Clear();
                Console.WriteLine("\n\nFinal Results...\n");
                GameUtilities.DisplayStandings(standings);
            }

            print("done");
            return;
        }
    }

    static class GameUtilities
    {
        static public List<BattleshipAgent> CreateAgentList(string[] args)
        {
            List<BattleshipAgent> agentList = new List<BattleshipAgent>();

            if (args.Length == 2)
            {
                agentList.Add(BattleshipLoader.LoadSingleAgent(args[0]));
                agentList.Add(BattleshipLoader.LoadSingleAgent(args[1]));
            }

            else if (args.Length == 1)
            {
                if (File.Exists(args[0]))
                {
                    agentList.Add(BattleshipLoader.LoadSingleAgent(args[0]));
                }
                else
                {
                    agentList = BattleshipLoader.LoadAllAgents(args[0]);
                }
            }

            else if (args.Length == 0)
            {
                agentList = BattleshipLoader.LoadAllAgents(null);

                if (agentList.Count > 1)
                {
                    Debug.Log("Agent List");
                    Debug.Log("------------------------");
                    foreach (BattleshipAgent agent in agentList)
                    {
                        Debug.Log($"{agent.Nickname}");
                    }

                    Debug.Log("\nPress any key to begin game...");
                    //Console.ReadKey();
                    //Console.Clear();
                }
            }

            return agentList;
        }

        static public (int, int) GetRandomGame(List<(int, int)> gameList, System.Random rng = null)
        {
            if (rng == null)
            {
                rng = new System.Random();
            }

            int randomIndex = rng.Next(gameList.Count);
            (int, int) thisGame = gameList[randomIndex];
            gameList.RemoveAt(randomIndex);

            return thisGame;
        }

        static public List<(int, int)> GenerateGameList(int teamCount, bool generateGamesBothDirections = true)
        {
            List<(int, int)> gameList = new List<(int, int)>();

            for (int a = 0; a < teamCount; a++)
            {
                int innerLoopStart;
                if (generateGamesBothDirections)
                {
                    innerLoopStart = 0;
                }
                else
                {
                    innerLoopStart = a;
                }

                for (int b = innerLoopStart; b < teamCount; b++)
                {
                    if (a != b)
                    {
                        gameList.Add((a, b));
                    }
                }
            }

            return gameList;
        }

        static public void DisplayStandings(Dictionary<string, TournamentStanding> standings)
        {
            // view the results
            List<TournamentStanding> results = new List<TournamentStanding>();
            foreach (TournamentStanding record in standings.Values)
            {
                results.Add(record);
            }
            results.Sort();
            results.Reverse();

            Console.WriteLine($"-----------------------------------------");
            Console.WriteLine($"| {"Agent",-20} | {"Ws",4} {"Ls",4} {"Ts",4} |");
            Console.WriteLine($"-----------------------------------------");
            foreach (TournamentStanding record in results)
            {
                Console.WriteLine($"| {record.agent,-20} | {record.W,4} {record.L,4} {record.T,4} |");
            }
            Console.WriteLine($"-----------------------------------------");
        }
    }

    class TournamentStanding : IComparable
    {
        public string agent;
        public int W;
        public int L;
        public int T;

        public TournamentStanding(string name)
        {
            agent = name;
            W = 0;
            L = 0;
            T = 0;
        }

        public int CompareTo(object obj)
        {
            TournamentStanding rhs = obj as TournamentStanding;
            return W.CompareTo(rhs.W);
        }
    }
}
