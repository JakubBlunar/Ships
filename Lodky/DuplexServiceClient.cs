using System;
using System.ServiceModel;
using ServerInterfaces;

namespace Lodky
{
    /// <summary>
    /// Service of client that sends messages to server
    /// </summary>
    internal class DuplexServiceClient : DuplexClientBase<IServerService>, IServerService
    {
        public DuplexServiceClient(InstanceContext callbackInstance, WSDualHttpBinding binding,
            EndpointAddress endpointAddress)
            : base(callbackInstance, binding, endpointAddress)
        {
        }

        /// <summary>
        /// Client want to connect
        /// </summary>
        /// <param name="name"></param>
        public void Connect(string name)
        {
            Channel.Connect(name);
        }

        /// <summary>
        /// Client discconected
        /// </summary>
        public void Disconnect()
        {
            Channel.Disconnect();
        }

        /// <summary>
        /// When player want ask another for game
        /// </summary>
        /// <param name="player">name of actual player</param>
        /// <param name="oponent">his opponent</param>
        public void AskForGame(string player, string oponent)
        {
            Channel.AskForGame(player, oponent);
        }

        /// <summary>
        /// Opponent confirm game, game has to be created
        /// </summary>
        /// <param name="player"></param>
        /// <param name="opponent"></param>
        public void NewGame(string player, string opponent)
        {
            Channel.NewGame(player, opponent);
        }

        /// <summary>
        /// Player hit some field on board
        /// </summary>
        /// <param name="playerName">name of player</param>
        /// <param name="gameId">id of game</param>
        /// <param name="x">x coord </param>
        /// <param name="y">y coord</param>
        public void FieldChoose(string playerName, Guid gameId, int x, int y)
        {
            Channel.FieldChoose(playerName, gameId, x, y);
        }

        /// <summary>
        /// Client want game history.
        /// </summary>
        /// <param name="guid">history of game to be returned</param>
        /// <param name="player">name of player</param>
        public void GetGameHistory(Guid guid, string player)
        {
            Channel.GetGameHistory(guid, player);
        }

        /// <summary>
        /// Player wants save game
        /// </summary>
        /// <param name="gameId">game id to be saved.</param>
        /// <param name="player">name of player</param>
        public void SaveGame(Guid gameId, string player)
        {
            Channel.SaveGame(gameId, player);
        }

        /// <summary>
        /// Player wants load some game.
        /// </summary>
        /// <param name="guid">Game to be loaded.</param>
        public void LoadGame(Guid guid)
        {
            Channel.LoadGame(guid);
        }

        /// <summary>
        /// Get all player saved games
        /// </summary>
        /// <param name="playerName">name of player</param>
        public void GetSaves(string playerName)
        {
            Channel.GetSaves(playerName);
        }

        /// <summary>
        /// Player want list of all players connected.
        /// </summary>
        /// <param name="player">name of player</param>
        public void GetPlayers(string player)
        {
            Channel.GetPlayers(player);
        }
    }
}