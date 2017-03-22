using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ServerInterfaces;

namespace ServerService
{
    /// <summary>
    /// Main logic of server, handling inputs from user
    /// </summary>
    [ServiceBehavior(
         InstanceContextMode = InstanceContextMode.Single,
         ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal class Server : IServerService
    {
        private static readonly object SyncObject = new object();
        private readonly List<IServerServiceCallback> _callbacks = new List<IServerServiceCallback>();
        private readonly Dictionary<Guid, Game> _games = new Dictionary<Guid, Game>();
        private readonly List<string> _names = new List<string>();

        /// <summary>
        /// Log new connected user and set callback for his name.
        /// Logged player gets history of games and his saved games.
        /// Send others player information that player connected.
        /// </summary>
        /// <param name="name"></param>
        public void Connect(string name)
        {
            try
            {
                var callbackChannel =
                    OperationContext.Current.GetCallbackChannel<IServerServiceCallback>();

                lock (SyncObject)
                {
                    if (_names.Contains(name))
                    {
                        var e = new Event
                        {
                            Id = 11,
                            Result = false
                        };
                        callbackChannel.SendCallback(e);
                    }
                    else
                    {
                        if (!_callbacks.Contains(callbackChannel))
                        {
                            _names.Add(name);
                            _callbacks.Add(callbackChannel);
                            Console.WriteLine("Client connected: {0} {1}", name, callbackChannel.GetHashCode());

                            CheckDeadCallback();

                            var p = new List<string>();
                            _names.ForEach(x => p.Add(x));
                            foreach (var t in _callbacks)
                            {
                                var e = new Event
                                {
                                    Id = 1,
                                    Players = p
                                };

                                var e2 = new Event
                                {
                                    Id = 7,
                                    Guids = GetHistory()
                                };


                                t.SendCallback(e);
                                Console.WriteLine("Event raised: {0}, {1}", e.Id, t.GetHashCode());

                                t.SendCallback(e2);
                                Console.WriteLine("Event raised: {0}, {1}", e2.Id, t.GetHashCode());
                            }

                            var e3 = new Event
                            {
                                Id = 9,
                                Guids = GetPlayerSaves(name)
                            };

                            callbackChannel.SendCallback(e3);
                            Console.WriteLine("Event raised: {0}, {1}", e3.Id, callbackChannel.GetHashCode());
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Sends opponents of player that disconected information about disconecting and removes this game.
        /// </summary>
        public void Disconnect()
        {
            var callbackChannel =
                OperationContext.Current.GetCallbackChannel<IServerServiceCallback>();

            try
            {
                lock (SyncObject)
                {
                    var index = _callbacks.FindIndex(x => x == callbackChannel);
                    if (_callbacks.Remove(callbackChannel))
                    {
                        var name = _names[index];
                        _names.RemoveAt(index);

                        var gamesToRemove = new List<Guid>();

                        foreach (var x in _games)
                            if ((x.Value.Player1 == name) || (x.Value.Player2 == name))
                                gamesToRemove.Add(x.Key);

                        foreach (var game in gamesToRemove)
                        {
                            if (_games[game].State == GameState.End)
                                continue;

                            var e = new Event
                            {
                                Id = 5,
                                Guid = game
                            };

                            if (_games[game].Player1 == name)
                            {
                                var i = _names.FindIndex(x => x == _games[game].Player2);
                                _callbacks[i].SendCallback(e);
                                Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[i].GetHashCode());
                            }
                            else
                            {
                                var i = _names.FindIndex(x => x == _games[game].Player1);
                                _callbacks[i].SendCallback(e);
                                Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[i].GetHashCode());
                            }

                            _games.Remove(game);

                            Console.WriteLine("Removed Game: {0}", game);
                        }
                        Console.WriteLine("Removed Client: {0}", callbackChannel.GetHashCode());
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Creates new game between two players and send them information about game.
        /// </summary>
        /// <param name="player">name of player</param>
        /// <param name="opponent">name of his opponent</param>
        public void NewGame(string player, string opponent)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();

                    var g = new Game(player, opponent);
                    _games.Add(g.Id, g);

                    var e = new Event
                    {
                        Id = 3,
                        Game = g
                    };

                    for (var i = _callbacks.Count - 1; i >= 0; i--)
                        try
                        {
                            if ((_names[i] == g.Player1) || (_names[i] == g.Player2))
                                _callbacks[i].SendCallback(e);
                            Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[i].GetHashCode());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Service threw exception while communicating on Callback Channel: {0}",
                                _callbacks[i].GetHashCode());
                            Console.WriteLine("Exception Type: {0} Description: {1}", ex.GetType(), ex.Message);
                            _callbacks.RemoveAt(i);
                        }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Send other player request to play new game.
        /// </summary>
        /// <param name="player">player who want to play</param>
        /// <param name="oponent">player that recieve message</param>
        public void AskForGame(string player, string oponent)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();
                    var e = new Event
                    {
                        Id = 2,
                        Name = player
                    };
                    var indexOponent = _names.FindIndex(x => x == oponent);
                    if (indexOponent != -1)
                    {
                        _callbacks[indexOponent].SendCallback(e);
                        Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[indexOponent].GetHashCode());
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Choosing specific field in some game. Send other player result of this.
        /// </summary>
        /// <param name="playerName">name of player that choosed field</param>
        /// <param name="gameId"> id of game that player is playing</param>
        /// <param name="x">x cord of field</param>
        /// <param name="y">y cord of field</param>
        public void FieldChoose(string playerName, Guid gameId, int x, int y)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();
                    if (!_games.ContainsKey(gameId))
                        return;
                    var game = _games[gameId];

                    var e = new Event
                    {
                        Id = 4, // clicked
                        Game = game
                    };


                    game.FieldChoose(playerName, x, y);

                    List<Guid> newHistory = null;
                    if (game.State == GameState.End)
                    {
                        using (var db = new DatabaseContext())
                        {
                            db.History.Add(new History {Game = game, Id = game.Id, Date = DateTime.Now});
                            try
                            {
                                db.SaveChanges();
                                Console.WriteLine("Game : {0} has been saved into history.", game.Id);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.InnerException?.Message);
                            }
                        }

                        newHistory = GetHistory();
                    }


                    for (var i = _callbacks.Count - 1; i >= 0; i--)
                        try
                        {
                            if ((_names[i] == game.Player1) || (_names[i] == game.Player2))
                                _callbacks[i].SendCallback(e);

                            if (newHistory != null)
                                _callbacks[i].SendCallback(new Event
                                {
                                    Id = 7,
                                    Guids = newHistory
                                });
                            Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[i].GetHashCode());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Service threw exception while communicating on Callback Channel: {0}",
                                _callbacks[i].GetHashCode());
                            Console.WriteLine("Exception Type: {0} Description: {1}", ex.GetType(), ex.Message);
                            _callbacks.RemoveAt(i);
                        }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Send history of specific game to specific player
        /// </summary>
        /// <param name="guid">id of game</param>
        /// <param name="player">name of player</param>
        public void GetGameHistory(Guid guid, string player)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();

                    var g = CreateGameFromDb(guid);

                    var e = new Event
                    {
                        Id = 6,
                        Game = g
                    };

                    for (var i = _callbacks.Count - 1; i >= 0; i--)
                        try
                        {
                            if (_names[i] == player)
                                _callbacks[i].SendCallback(e);
                            Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[i].GetHashCode());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Service threw exception while communicating on Callback Channel: {0}",
                                _callbacks[i].GetHashCode());
                            Console.WriteLine("Exception Type: {0} Description: {1}", ex.GetType(), ex.Message);
                            _callbacks.RemoveAt(i);
                        }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Save specified game into database. 
        /// Sends informations to players that game was saved.
        /// </summary>
        /// <param name="gameId">id of game</param>
        /// <param name="player">name of player that want to save</param>
        public void SaveGame(Guid gameId, string player)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();

                    var e = new Event
                    {
                        Id = 8,
                        Guid = gameId
                    };


                    if (!_games.ContainsKey(gameId))
                    {
                        var index = _names.FindIndex(x => x == player);
                        if (index != -1)
                        {
                            e.Result = false;
                            _callbacks[index].SendCallback(e);
                            Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[index].GetHashCode());
                        }
                    }
                    else
                    {
                        var g = _games[gameId];
                        bool saved;
                        using (var context = new DatabaseContext())
                        {
                            try
                            {
                                context.GameMoves.RemoveRange(context.GameMoves.Where(x => x.GameId == gameId));
                                context.Saves.RemoveRange(context.Saves.Where(x => x.Id == gameId));
                                context.SaveChanges();

                                context.Saves.Add(g);
                                context.SaveChanges();
                                saved = true;
                            }
                            catch
                            {
                                saved = false;
                            }
                        }

                        e.Result = saved;

                        if (saved)
                        {
                            var index = _names.FindIndex(x => x == g.Player1);
                            if (index != -1)
                            {
                                _callbacks[index].SendCallback(e);
                                Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[index].GetHashCode());
                            }

                            index = _names.FindIndex(x => x == g.Player2);
                            if (index != -1)
                            {
                                _callbacks[index].SendCallback(e);
                                Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[index].GetHashCode());
                            }
                        }
                        else
                        {
                            var index = _names.FindIndex(x => x == player);
                            if (index != -1)
                            {
                                _callbacks[index].SendCallback(e);
                                Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[index].GetHashCode());
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// If both players are playing then load saved game between them.
        /// </summary>
        /// <param name="guid">id of game to be loaded</param>
        public void LoadGame(Guid guid)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();
                    var g = CreateGameFromDb(guid);
                    if (g != null)
                    {
                        var indexPlayer1 = _names.FindIndex(x => x == g.Player1);
                        var indexPlayer2 = _names.FindIndex(x => x == g.Player2);

                        if ((indexPlayer1 != -1) && (indexPlayer2 != -1))
                            using (var context = new DatabaseContext())
                            {
                                try
                                {
                                    context.GameMoves.RemoveRange(context.GameMoves.Where(x => x.GameId == g.Id));
                                    context.Saves.RemoveRange(context.Saves.Where(x => x.Id == g.Id));
                                    context.SaveChanges();
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                        if (indexPlayer1 != -1)
                        {
                            var e = new Event
                            {
                                Id = 9,
                                Guids = GetPlayerSaves(g.Player1)
                            };
                            _callbacks[indexPlayer1].SendCallback(e);
                        }

                        if (indexPlayer2 != -1)
                        {
                            var e = new Event
                            {
                                Id = 9,
                                Guids = GetPlayerSaves(g.Player1)
                            };
                            _callbacks[indexPlayer1].SendCallback(e);
                        }

                        if ((indexPlayer1 != -1) && (indexPlayer2 != -1))
                        {
                            var e = new Event
                            {
                                Id = 10,
                                Game = g,
                                Result = true
                            };
                            _games.Add(g.Id, g);
                            _callbacks[indexPlayer1].SendCallback(e);
                            _callbacks[indexPlayer2].SendCallback(e);
                        }
                        else
                        {
                            var e = new Event
                            {
                                Id = 10,
                                Game = g,
                                Result = false
                            };

                            if (indexPlayer1 != -1)
                                _callbacks[indexPlayer1].SendCallback(e);
                            else
                                _callbacks[indexPlayer2].SendCallback(e);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Returns to player all his saved games.
        /// </summary>
        /// <param name="playerName">name of player</param>
        public void GetSaves(string playerName)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();
                    var e = new Event
                    {
                        Id = 9,
                        Guids = GetPlayerSaves(playerName)
                    };

                    var indexPlayer = _names.FindIndex(x => x == playerName);
                    if (indexPlayer != -1)
                    {
                        _callbacks[indexPlayer].SendCallback(e);
                        Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[indexPlayer].GetHashCode());
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Returns all logged players.
        /// </summary>
        /// <param name="player">name of player where this information will be send</param>
        public void GetPlayers(string player)
        {
            try
            {
                lock (SyncObject)
                {
                    CheckDeadCallback();
                    var p = new List<string>();
                    _names.ForEach(x => p.Add(x));
                    var e = new Event
                    {
                        Id = 1,
                        Players = p
                    };

                    var indexPlayer = _names.FindIndex(x => x == player);
                    if (indexPlayer != -1)
                    {
                        _callbacks[indexPlayer].SendCallback(e);
                        Console.WriteLine("Event raised: {0}, {1}", e.Id, _callbacks[indexPlayer].GetHashCode());
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Load game form db
        /// </summary>
        /// <param name="guid">id of game to be loaded</param>
        /// <returns>Loaded game</returns>
        private Game CreateGameFromDb(Guid guid)
        {
            Game g = null;
            using (var context = new DatabaseContext())
            {
                var row = context.Saves
                    .FirstOrDefault(f => f.Id == guid);

                if (row != null)
                {
                    g = new Game
                    {
                        Id = row.Id,
                        Player1 = row.Player1,
                        Player2 = row.Player2,
                        Player1Turn = row.Player1Turn,
                        Player1Lives = row.Player1Lives,
                        Player2Lives = row.Player2Lives,
                        Board1S = row.Board1S,
                        Board2S = row.Board2S,
                        State = row.State,
                        Moves = new LinkedList<GameMove>()
                    };

                    var moves = (from b in context.GameMoves
                        where b.GameId == g.Id
                        orderby b.Id ascending
                        select b).ToList();

                    foreach (var b in moves)
                        g.Moves.AddLast(new GameMove
                        {
                            Id = b.Id,
                            PlayerName = b.PlayerName,
                            X = b.X,
                            Y = b.Y,
                            GameId = b.GameId,
                            Result = b.Result
                        });
                }
            }
            return g;
        }

        /// <summary>
        /// Check if some client is disconected and if yes then remove from list
        /// </summary>
        private void CheckDeadCallback()
        {
            for (var i = _callbacks.Count - 1; i >= 0; i--)
            {
                var communicationObject = _callbacks[i] as ICommunicationObject;
                if ((communicationObject != null) && (communicationObject.State != CommunicationState.Opened))
                {
                    Console.WriteLine("Detected Non-Open Callback Channel: {0}", _callbacks[i].GetHashCode());
                    _callbacks.RemoveAt(i);
                    _names.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Returns id of games that represents history
        /// </summary>
        /// <returns>history of games</returns>
        private List<Guid> GetHistory()
        {
            var history = new List<Guid>();
            using (var context = new DatabaseContext())
            {
                var ids = (from b in context.History
                    orderby b.Date descending
                    select b.Id).Take(30).ToList();

                history.AddRange(ids);
            }
            return history;
        }

        /// <summary>
        /// Load from db player saves
        /// </summary>
        /// <param name="player">name of player</param>
        /// <returns>list of saved games guids</returns>
        private List<Guid> GetPlayerSaves(string player)
        {
            var saves = new List<Guid>();
            using (var context = new DatabaseContext())
            {
                var ids = (from b in context.Saves
                    where ((b.Player1 == player) || (b.Player2 == player)) && (b.State != GameState.End)
                    select b.Id).ToList();

                saves.AddRange(ids);
            }
            return saves;
        }
    }
}