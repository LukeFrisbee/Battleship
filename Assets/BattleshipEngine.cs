using System;
using System.Collections;
using System.Threading;
using UnityEngine;


namespace Battleship
{
    /// <summary>
    /// The primary Battleship game engine
    /// </summary>
    public class BattleshipEngine : MonoBehaviour
    {

        /* Constants for drawing the gameboards */
        private const int Board0LeftPosition = -11;
        private const int Board1LeftPosition = 2;
        private const int marqueeWidth = 33;
        private const int marqueeLastLine = 24;
        private const int marqueeLeft = 23;
        private const int marqueeTop = 0;

        /* Arrays holding the important data for each player */
        private BattleshipAgent[] agents;
        private BattleshipBoard[] boards;

        /* Run all agent code in a separate thread to defend against infinite loops */
        private bool safetyThreadThrewException;
        public string exceptionMessage;

        /* No synchornization necessary because main thread waits for background thread to finish */
        private GridSquare gridSquare;
        private char shipDamage;
        public string winner;

        public static BattleshipEngine engine;

        private void Start()
        {
            print("Engine Start");
        }

        public BattleshipEngine(BattleshipAgent player1, BattleshipAgent player2)
        {
            SetAgents(player1, player2);
        }

        public void SetAgents(BattleshipAgent player1, BattleshipAgent player2)
        {
            player1.Nickname = player1.Nickname.Substring(0, Math.Min(20, player1.Nickname.Length));
            player2.Nickname = player2.Nickname.Substring(0, Math.Min(20, player2.Nickname.Length));
            agents = new BattleshipAgent[2] { player1, player2 };
        }


        public string PlaySingleGame(bool showThisGame = true, float delayBetweenPlayers = 250)
        {
            //IEnumerator coroutine = Courintine.PlaySingleGameCoroutine(showThisGame, delayBetweenPlayers);
            var winner = StartCoroutine(PlaySingleGameCoroutine(showThisGame, delayBetweenPlayers));
            return winner.ToString();
        }

        /// <summary>
        /// Plays a single game of Battleship
        /// </summary>
        /// <param name="showGame">Sets whether the game is played in the Console/Terminal</param>
        /// <param name="delay">The amount of time between players turns, in milliseconds</param>
        /// <returns>The agent that won the game</returns>
        private IEnumerator PlaySingleGameCoroutine(bool showThisGame = true, float delayBetweenPlayers = 250)
        {
            boards = new BattleshipBoard[2] { new BattleshipBoard(agents[0].Nickname, 0, Board0LeftPosition),
                                              new BattleshipBoard(agents[1].Nickname, 1, Board1LeftPosition) };

            exceptionMessage = null;
            winner = null;

            try
            {
                /* Initialize the two agents: these are likely to succeed */

                if (!InitializeAgent(0))
                {
                    winner = agents[1].Nickname;
                    yield return winner;
                }
                if (!InitializeAgent(1))
                {
                    winner = agents[0].Nickname;
                    yield return winner;
                }

                /* Position the two fleets: might fail if ships are out-of-bounds */

                if (!PositionFleet(0))
                {
                    winner = agents[1].Nickname;
                    yield return winner;
                }
                if (!PositionFleet(1))
                {
                    winner = agents[0].Nickname;
                    yield return winner;
                }

                /* Display the gameboards, if playing a live game */

                if (showThisGame)
                {
                    MarqueePrintPlayerNames();
                    boards[0].DrawBoard();
                    boards[1].DrawBoard();
                }

                /* Main Game Loop
                 * 
                 *   The gameboard has 100 slots but we allow for 120 turns to 
                 *   accomodate agents that shoot off the board or shoot the same
                 *   square multiple times.
                 */

                while (boards[0].ShotCount < 120 && boards[1].ShotCount < 120)
                {
                    /* Let the first agent play its turn */

                    if (!PlayAgentsTurn(0))
                    {
                        winner = agents[1].Nickname;
                        yield return winner;
                    }

                    if (showThisGame)
                    {
                        MarqueePlayerAttack(gridSquare.x, gridSquare.y, shipDamage, 0);
                        boards[1].DrawShot(gridSquare.x, gridSquare.y);
                    }

                    if (boards[1].ShipLives == 0)
                    {
                        winner = agents[0].Nickname;
                        yield return winner;
                    }

                    yield return new WaitForSeconds(delayBetweenPlayers);

                    /* Let the second agent play its turn */

                    if (!PlayAgentsTurn(1))
                    {
                        winner = agents[0].Nickname;
                        yield return winner;
                    }

                    if (showThisGame)
                    {
                        MarqueePlayerAttack(gridSquare.x, gridSquare.y, shipDamage, 1);
                        boards[0].DrawShot(gridSquare.x, gridSquare.y);
                    }

                    if (boards[0].ShipLives == 0)
                    {
                        winner = agents[1].Nickname;
                        yield return winner;
                    }

                    //Thread.Sleep(delayBetweenPlayers);
                    yield return new WaitForSeconds(delayBetweenPlayers);
                }
            }
            finally
            {
                if (showThisGame)
                {
                    MarqueePrintWinner(winner);
                    //Console.WriteLine($"{exceptionMessage}");
                    Debug.Log($"{exceptionMessage}");
                }
            }

            yield return winner;
        }

        private bool InitializeAgent(int agentNumber = 0)
        {
            InitThread(agentNumber);
            return true;

            /*Thread myThread = new Thread(new ParameterizedThreadStart(InitThread));
            string nickname = agents[agentNumber].Nickname;

            safetyThreadThrewException = false;
            myThread.Start(agentNumber);
            if (!myThread.Join(500))
            {
                myThread.Abort();
                exceptionMessage = $"Agent '{nickname}' timed out";
                return false;
            }
            if (safetyThreadThrewException)
            {
                return false;
            }

            return true;*/
        }
        
        private void InitThread(object param)
        {
            int player = (int)param;

            try
            {
                agents[player].Initialize();

            }
            catch (Exception e)
            {
                exceptionMessage = e.Message;
                safetyThreadThrewException = true;
            }
        }

        private bool PositionFleet(int agentNumber = 0)
        {
            PositionThread(agentNumber);
            return true;

            /*Thread myThread = new Thread(new ParameterizedThreadStart(PositionThread));
            string nickname = agents[agentNumber].Nickname;

            safetyThreadThrewException = false;
            myThread.Start(agentNumber);
            if (!myThread.Join(500))
            {
                myThread.Abort();
                exceptionMessage = $"Agent '{nickname}' timed out";
                return false;
            }
            if (safetyThreadThrewException)
            {
                return false;
            }

            return true;*/
        }

        private void PositionThread(object param)
        {
            int player = (int)param;
            int opponent = (player + 1) % 2;

            try
            {
                agents[player].SetOpponent(agents[opponent].Nickname);
                BattleshipFleet myFleet = agents[player].PositionFleet();
                boards[player].PlacePlayerShips(myFleet);
            }
            catch (Exception e)
            {
                exceptionMessage = e.Message;
                safetyThreadThrewException = true;
            }
        }

        private bool PlayAgentsTurn(int agentNumber = 0)
        {
            TakeTurnThread(agentNumber);
            return true;

            /*Thread myThread = new Thread(new ParameterizedThreadStart(TakeTurnThread));
            string nickname = agents[agentNumber].Nickname;

            if (nickname == "Human Debugger")
            {
                MarqueePromptPlayer(0);
            }

            safetyThreadThrewException = false;
            myThread.Start(agentNumber);
            if (!myThread.Join(500))
            {
                myThread.Abort();
                exceptionMessage = $"Agent '{nickname}' timed out";
                return false;
            }
            if (safetyThreadThrewException)
            {
                return false;
            }

            return true;*/
        }
        
        private void TakeTurnThread(object param)
        {
            int player = (int)param;
            int opponent = (player + 1) % 2;

            safetyThreadThrewException = false;
            try
            {
                gridSquare = agents[player].LaunchAttack();
                shipDamage = boards[opponent].ProcessPlayersShot(gridSquare.x, gridSquare.y);
                agents[player].DamageReport(shipDamage);
            }
            catch (Exception e)
            {
                exceptionMessage = e.Message;
                safetyThreadThrewException = true;
            }
        }

        private string CenteredString(string str, bool promptUser = false)
        {
            int padding = (marqueeWidth - str.Length) / 2;
            if (promptUser)
            {
                return str.PadLeft(padding + str.Length);
            }
            else
            {
                return str.PadLeft(padding + str.Length).PadRight(marqueeWidth);
            }
        }

        private void MarqueePrintPlayerNames(int offset = 2)
        {
            Console.CursorVisible = false;
            Console.Clear();

            string text1 = CenteredString($"-={agents[0].Nickname}=-");
            string text2 = CenteredString($"-={agents[1].Nickname}=-");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset);
            Console.Write(text1);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset + 9);
            Console.Write(text2);
        }

        private void MarqueePromptPlayer(int playerNumber = 0)
        {
            int offset = (playerNumber == 0 ? 4 : 13);

            string clear = CenteredString("");
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset);
            Console.Write(clear);
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset + 1);
            Console.Write(clear);
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset + 2);
            Console.Write(clear);
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset + 3);
            Console.Write(clear);
            string prompt = CenteredString("Launch Attack:   ", true);
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset);
            Console.Write(prompt);
        }

        private void MarqueePlayerAttack(int x, int y, char result, int playerNumber = 0)
        {
            int offset = (playerNumber == 0 ? 4 : 13);
            if (agents[playerNumber].Nickname == "Human Debugger")
            {
                offset += 1;
            }

            char xChar = (char)('A' + x);
            string text1 = CenteredString($"Fires at");
            string arrow = (playerNumber == 0 ? "========>" : "<========");
            string text2 = (playerNumber == 0 ? CenteredString($"{arrow} {xChar}{y}") :
                                                CenteredString($"{xChar}{y} {arrow}"));
            string text3 = (result == ShipType.None ? CenteredString("Missed") :
                                                      CenteredString($"Hit the {ShipType.Name(result)}"));

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset);
            Console.Write(text1);

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset + 1);
            Console.Write(text2);

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset + 2);
            Console.Write(text3);
        }

        private void MarqueePrintWinner(string winner, int offset = 19)
        {
            string text;
            if (string.IsNullOrEmpty(winner))
            {
                text = CenteredString($"tie game");
            }
            else
            {
                text = CenteredString($"{winner} wins the game!");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(marqueeLeft, marqueeTop + offset);
            Console.Write(text);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(0, marqueeLastLine);

        }
    }

    /// <summary>
    /// Tracks and draws a board for Battleship
    /// </summary>
    class BattleshipBoard
    {
        private const int boardDimension = 10;      // We are playing on a 10x10 Battleship board
        private const int boardPrintingWidth = 22;  // Accounts for the grid squares and the interior/exterior table borders
        private const int newGameShipLives = 17;    // Carrier(5) Battleship(4) Destroyer(3) Submarine(3) PatrolBoat(2)
        private readonly string nickname;           // The name that this player goes by
        private readonly int playerNumber;
        private readonly int boardAnchorLeft;       // Anchor point for drawing this board
        private readonly int boardAnchorTop;        // Anchor point for drawing this board
        private BattleshipGridSquare[,] gameboard;
        public int ShipLives { get; private set; }
        public int ShotCount { get; private set; }

        public GameObject submarine;

        public BattleshipBoard(string name, int playerTurn, int left = 0, int top = 0)
        {
            nickname = name.Trim(new char[] { '-', '=' });
            playerNumber = playerTurn;
            boardAnchorLeft = left;
            boardAnchorTop = top;
            gameboard = new BattleshipGridSquare[boardDimension, boardDimension];
            InitializeGameboard();
        }


        private void InitializeGameboard()
        {
            ShotCount = 0;
            for (int i = 0; i < gameboard.GetLength(0); i++)
            {
                for (int j = 0; j < gameboard.GetLength(1); j++)
                {
                    gameboard[i, j] = new BattleshipGridSquare();
                }
            }
        }


        /// <summary>
        /// Places a players Battleship ships on the playing board to start the game
        /// </summary>
        /// <param name="playerBoard">Structure that identifies the top-left location of each ship and its horizontal/vertical orientation</param>
        /// <returns>True if all five ships were placed in legal positions, false otherwise</returns>
        public void PlacePlayerShips(BattleshipFleet fleet)
        {
            // Clear the game board and then initialize the player's lives
            // We don't track individually boats, just count the total number of boat squares
            gameboard.Initialize();
            ShipLives = newGameShipLives;

            // Blindly places a single boat on the board *WITHOUT* checking whether they overlap
            PlaceSingleShip(fleet.Carrier, ShipType.Carrier);
            PlaceSingleShip(fleet.Battleship, ShipType.Battleship);
            PlaceSingleShip(fleet.Destroyer, ShipType.Destroyer);
            PlaceSingleShip(fleet.Submarine, ShipType.Submarine);
            PlaceSingleShip(fleet.PatrolBoat, ShipType.PatrolBoat);

            // Count how many grid squares are occupied by ships... if there are less than 17
            // then it means that some of the ships overlap because the player placed them wrong
            int gridSquaresWithShips = 0;
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (gameboard[x, y].Ship != ShipType.None)
                    {
                        gridSquaresWithShips++;
                    }
                }
            }

            // If there's a difference, it means the player arranged the fleet incorrectly
            if (gridSquaresWithShips != newGameShipLives)
            {
                throw new ArgumentException($"Could not place {nickname}'s ships correctly on the gameboard");
            }

            return;
        }
        private void PlaceSingleShip(ShipPosition position, char shipType)
        {
            int x = position.left;
            int y = position.top;

            int horizontal;
            int vertical;
            Quaternion rotation; 
            if (position.rotation == ShipRotation.Horizontal)
            {
                horizontal = 1;
                vertical = 0;
                rotation = Quaternion.Euler(0, -90, 0);
            }
            else
            {
                horizontal = 0;
                vertical = 1;
                rotation = Quaternion.Euler(0, 0, 0);
            }

            int shipLength = 0;
            switch (shipType)
            {
                case ShipType.Carrier:
                    shipLength = 5;
                    break;
                case ShipType.Battleship:
                    shipLength = 4;
                    break;
                case ShipType.Destroyer:
                case ShipType.Submarine:
                    shipLength = 3;
                    break;
                case ShipType.PatrolBoat:
                    shipLength = 2;
                    break;
                default:
                    string msg = $"ShipType {shipType} is invalid";
                    throw new ArgumentException(msg);
            }

            submarine = GameObject.Find("sub");
            GameObject sub = GameObject.Instantiate(submarine, new Vector3(boardAnchorLeft + x, 0, y), rotation);

            for (int i = 0; i < shipLength; i++)
            {
                if (x < 0 || x > 9 || y < 0 || y > 9)
                {
                    string msg = $"Ship {shipType} placed incorrectly at {position}";
                    throw new ArgumentOutOfRangeException("position", msg);
                }

                gameboard[x, y].Ship = shipType;
                //GameObject cube = GameObject.Instantiate(GameObject.FindGameObjectWithTag("Cube"));
                //cube.transform.position = new Vector3(boardAnchorLeft + x, 0, y);
                x += horizontal;
                y += vertical;
            }
        }


        /// <summary>
        /// Draws a single Battleship gameboard that is rooted at the left, top coordinates
        /// </summary>
        public void DrawBoard()
        {
            // Print this player's board in bright white
            Console.ForegroundColor = (playerNumber % 2 == 0 ? ConsoleColor.Green : ConsoleColor.Yellow);
            Console.SetCursorPosition(boardAnchorLeft, boardAnchorTop);
            Console.Write(CenteredString($"-={nickname}=-"));

            // Print the top axis (x-axis) in dark gray
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.SetCursorPosition(boardAnchorLeft, boardAnchorTop + 2);
            Console.Write(" |");
            for (int x = 0; x < 10; x++)
            {
                char colNum = (char)('A' + x);
                Console.Write($"{colNum}|");
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.SetCursorPosition(boardAnchorLeft, boardAnchorTop + 3);
            Console.Write("-+-+-+-+-+-+-+-+-+-+-+");

            // Print each row of the actual board, one at at time
            for (int y = 0; y < gameboard.GetLength(0); y++)
            {
                // These first two characters are the y-axis in dark gray
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.SetCursorPosition(boardAnchorLeft, boardAnchorTop + 4 + (2 * y));
                Console.Write($"{y}|");

                // Now print the individual grid cells in this row
                for (int x = 0; x < gameboard.GetLength(1); x++)
                {
                    Console.ForegroundColor = GetColorForGrid(x, y);
                    Console.Write($"{gameboard[x, y]}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("|");
                }

                // Print the bottom grid for this row
                Console.SetCursorPosition(boardAnchorLeft, boardAnchorTop + 4 + (2 * y) + 1);
                Console.Write("-+-+-+-+-+-+-+-+-+-+-+");
            }
            Console.WriteLine();
        }
        public void DrawShot(int x, int y)
        {
            if (gameboard[x, y].Ship == ShipType.None)
            {
                Console.SetCursorPosition(boardAnchorLeft + 2 + (2 * x), boardAnchorTop + 4 + (2 * y));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"X");
            }
            else
            {
                Console.SetCursorPosition(boardAnchorLeft + 2 + (2 * x), boardAnchorTop + 4 + (2 * y));
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"{gameboard[x, y]}");
            }
            GameObject attack = GameObject.Instantiate(GameObject.FindGameObjectWithTag("Attack"));
            attack.transform.position = new Vector3(boardAnchorLeft + x, 0, y);
        }
        private string CenteredString(string str)
        {
            int padding = (boardPrintingWidth - str.Length) / 2;
            return str.PadLeft(padding + str.Length);
        }
        private ConsoleColor GetColorForGrid(int x, int y)
        {
            // Empty sea has a color of gray
            if (gameboard[x, y].Ship == ShipType.None)
            {
                return ConsoleColor.Gray;
            }

            // We have a ship at this location, so use either white/red depending on if the ship has been hit
            else
            {
                if (gameboard[x, y].Hit == true)
                {
                    return ConsoleColor.DarkRed;
                }
                else
                {
                    return ConsoleColor.White;
                }
            }
        }



        /// <summary>
        /// Determine if a player hits one of the opponent's ships. Keeps track of all record keeping and drawing.
        /// </summary>
        /// <param name="shooter">nickname of the player doing the shooting</param>
        /// <param name="x">x-coordinate of the shot</param>
        /// <param name="y">y-coordinate of the shot</param>
        /// <returns></returns>
        public char ProcessPlayersShot(int x, int y)
        {
            // We use A-J for the x-axis rather than numbers
            char xChar = (char)('A' + x);

            // Error, the player shoots off the board
            if (x < 0 || x >= boardDimension || y < 0 && y >= boardDimension)
            {
                return ShipType.None;
            }

            // The player hit a ship (although it may be a double-hit)
            if (!gameboard[x, y].Hit && gameboard[x, y].Ship != ShipType.None)
            {
                gameboard[x, y].Hit = true;
                ShipLives--;
            }

            ShotCount++;
            return gameboard[x, y].Ship;
        }
    }


    /// <summary>
    /// Represents a single square on the Battleship game board.
    /// </summary>
    public class BattleshipGridSquare
    {
        private GridSquare grid;
        private char ship;
        private bool hit;

        public BattleshipGridSquare()
        {
            this.grid = new GridSquare(0, 0);
            this.ship = ShipType.None;
            this.hit = false;
        }

        public BattleshipGridSquare(int x, int y, bool hit = false, char ship = ShipType.None)
        {
            this.grid = new GridSquare(x, y);
            this.ship = ship;
            this.hit = hit;
        }

        public char Ship
        {
            get { return ship; }
            set { ship = value; }
        }

        public bool Hit
        {
            get { return hit; }
            set { hit = value; }
        }

        public int X
        {
            get { return grid.x; }
            set { grid.x = value % 10; }
        }

        public int Y
        {
            get { return grid.y; }
            set { grid.y = value % 10; }
        }

        public GridSquare Pos
        {
            get { return grid; }
            set { X = value.x; Y = value.y; }
        }

        public override string ToString()
        {
            switch (ship)
            {
                case ShipType.Carrier:
                    return "C";
                case ShipType.Battleship:
                    return "B";
                case ShipType.Destroyer:
                    return "D";
                case ShipType.Submarine:
                    return "S";
                case ShipType.PatrolBoat:
                    return "P";
            }

            if (hit == true)
            {
                return "X";
            }
            else
            {
                return " ";
            }
        }
    }
}
