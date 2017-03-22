using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace ServerInterfaces
{
    /// <summary>
    /// State of game when it is crated
    /// </summary>
    public enum GameState
    {
        Init,
        Play,
        End
    }

    /// <summary>
    /// Game Ships between two players.
    /// </summary>
    [DataContract]
    public class Game
    {
        public Game()
        {
        }

        /// <summary>
        /// creates new game between two players
        /// </summary>
        /// <param name="player1">name of player 1</param>
        /// <param name="player2">name of player 2</param>
        public Game(string player1, string player2)
        {
            Id = Guid.NewGuid();
            State = GameState.Init;
            Player1 = player1;
            Player2 = player2;
            Board1 = new int[16*16];
            Board2 = new int[16*16];

            Player1Lives = 0;
            Player2Lives = 0;

            Player1Turn = true;
            Moves = new LinkedList<GameMove>();
        }

        [Key]
        [DataMember(Name = "gameId")]
        public Guid Id { get; set; }

        [DataMember(Name = "gameState")]
        public GameState State { get; set; }

        [DataMember(Name = "player1")]
        public string Player1 { get; set; }

        [DataMember(Name = "player2")]
        public string Player2 { get; set; }

        [DataMember(Name = "board1")]
        public int[] Board1 { get; set; }

        [DataMember(Name = "board2")]
        public int[] Board2 { get; set; }

        [DataMember(Name = "player1turn")]
        public bool Player1Turn { get; set; }

        [DataMember(Name = "player1lives")]
        public int Player1Lives { get; set; }

        [DataMember(Name = "player2lives")]
        public int Player2Lives { get; set; }

        [DataMember(Name = "moves")]
        public LinkedList<GameMove> Moves { get; set; }

        [DataMember(Name = "board1s")]
        public string Board1S
        {
            get { return string.Join(";", Board1.Select(p => p.ToString()).ToArray()); }
            set { Board1 = Array.ConvertAll(value.Split(';'), int.Parse); }
        }

        [DataMember(Name = "board2s")]
        public string Board2S
        {
            get { return string.Join(";", Board2.Select(p => p.ToString()).ToArray()); }
            set { Board2 = Array.ConvertAll(value.Split(';'), int.Parse); }
        }

        /// <summary>
        /// Logic for game, when some players hit some field.
        /// </summary>
        /// <param name="playerName">player that chooses field</param>
        /// <param name="x">x cord of field</param>
        /// <param name="y">y cord of field</param>
        public void FieldChoose(string playerName, int x, int y)
        {
            if (State == GameState.Init) // to set up own fields
            {
                if ((playerName == Player1) && (Player1Lives < 10))
                {
                    Board1[y*16 + x] = 1;
                    Player1Lives++;
                }

                if ((playerName == Player2) && (Player2Lives < 10))
                {
                    Board2[y*16 + x] = 1;
                    Player2Lives++;
                }

                if ((Player1Lives == 10) && (Player2Lives == 10))
                    State = GameState.Play;
            }
            else if (State == GameState.Play) // hitting opponents field
            {
                if ((playerName == Player1) && Player1Turn)
                {
                    if (Board2[y*16 + x] == 1)
                    {
                        Board2[y*16 + x] = 3;
                        Player2Lives--;
                        var m = new GameMove
                        {
                            Id = Moves.Count,
                            PlayerName = Player1,
                            Y = y,
                            X = x,
                            Result = 3,
                            GameId = Id
                        };
                        Moves.AddFirst(m);
                    }
                    else
                    {
                        Board2[y*16 + x] = 2;
                        var m = new GameMove
                        {
                            Id = Moves.Count,
                            PlayerName = Player1,
                            Y = y,
                            X = x,
                            Result = 2,
                            GameId = Id
                        };
                        Moves.AddFirst(m);
                    }
                    Player1Turn = false;
                }
                else if ((playerName == Player2) && !Player1Turn)
                {
                    if (Board1[y*16 + x] == 1)
                    {
                        Board1[y*16 + x] = 3;
                        Player1Lives--;
                        var m = new GameMove
                        {
                            Id = Moves.Count,
                            PlayerName = Player2,
                            Y = y,
                            X = x,
                            Result = 3,
                            GameId = Id
                        };
                        Moves.AddFirst(m);
                    }
                    else
                    {
                        Board1[y*16 + x] = 2;
                        var m = new GameMove
                        {
                            Id = Moves.Count,
                            PlayerName = Player2,
                            Y = y,
                            X = x,
                            Result = 2,
                            GameId = Id
                        };
                        Moves.AddFirst(m);
                    }
                    Player1Turn = true;
                }

                if ((Player1Lives <= 0) || (Player2Lives <= 0)) State = GameState.End;
            }
        }
    }
}