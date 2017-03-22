using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel;
using System.Windows;
using ServerInterfaces;

namespace Lodky
{
    /// <summary>
    ///     Main window of client, handles callbacks from server and sending requests.
    /// </summary>
    public partial class MainWindow
    {
        private const string Uri = "http://localhost:7777/Ships/ServerService/";
        private readonly Dictionary<Guid, BoardWindow> _games;
        private DuplexServiceClient _proxy;
        public string PlayerName { get; }

        /// <summary>
        /// Creates new main window
        /// </summary>
        /// <param name="name">name of player that will be playing</param>
        public MainWindow(string name)
        {
            PlayerName = name;
            InitializeComponent();
            InitializeClient();

            _games = new Dictionary<Guid, BoardWindow>();

            TextBlock.Text = TextBlock.Text + " " + PlayerName;
        }

        /// <summary>
        /// Connect client to server and get all informations needed.
        /// </summary>
        private void InitializeClient()
        {
            if (_proxy != null)
                try
                {
                    _proxy.Close();
                }
                catch
                {
                    _proxy.Abort();
                }
                finally
                {
                    _proxy = null;
                }

            var callbackInstance = new DuplexCallback();
            callbackInstance.ServiceCallbackEvent += HandleServiceCallbackEvent;

            var instanceContext = new InstanceContext(callbackInstance);
            var dualHttpBinding = new WSDualHttpBinding(WSDualHttpSecurityMode.None);
            var endpointAddress = new EndpointAddress(Uri);
            _proxy = new DuplexServiceClient(instanceContext, dualHttpBinding, endpointAddress);
            _proxy.Open();
            _proxy.Connect(PlayerName);
        }

        /// <summary>
        /// Method for handling callbacks from server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event what happend with informations.</param>
        private void HandleServiceCallbackEvent(object sender, Event e)
        {
            switch (e.Id)
            {
                case 1: // changed player list
                    LbPlayers.Items.Clear();
                    foreach (var player in e.Players)
                        LbPlayers.Items.Add(player);
                    break;
                case 2: // want to play with you
                    var result = MessageBox.Show("Player " + e.Name + " want to play with you. Do you want to play?",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                        _proxy.NewGame(PlayerName, e.Name);
                    break;
                case 3: // game created
                    var board = new BoardWindow(this, e.Game);
                    _games.Add(e.Game.Id, board);
                    LbActiveGames.Items.Add(e.Game.Id.ToString());
                    board.UpdateGui();
                    board.Show();
                    break;
                case 4: // field choosen
                    _games[e.Game.Id].Game = e.Game;
                    _games[e.Game.Id].UpdateGui();

                    if ((e.Game.State == GameState.Play) && (e.Game.Moves.Count > 0))
                        if (e.Game.Moves.First.Value.PlayerName != PlayerName)
                            _games[e.Game.Id].HitFromPlayer(e.Game.Moves.First.Value.X, e.Game.Moves.First.Value.Y,
                                e.Game.Moves.First.Value.Result);

                    if (e.Game.State == GameState.End)
                    {
                        _games[e.Game.Id].Close();
                        _games.Remove(e.Game.Id);

                        LbActiveGames.Items.Clear();
                        foreach (var id in _games.Keys)
                            LbActiveGames.Items.Add(id.ToString());
                    }
                    break;
                case 5: // opponent disconected
                    MessageBox.Show(_games[e.Guid], "Another player disconected.");
                    _games[e.Guid].Close();
                    _games.Remove(e.Guid);
                    LbActiveGames.Items.Clear();
                    foreach (var id in _games.Keys)
                        LbActiveGames.Items.Add(id.ToString());
                    _proxy.GetSaves(PlayerName);
                    _proxy.GetPlayers(PlayerName);

                    break;
                case 6: // show history
                    if (e.Game != null)
                    {
                        var p = new HistoryPlay(e.Game);
                        p.Show();
                        p.Play();
                    }
                    else
                    {
                        MessageBox.Show("game dont exists");
                    }
                    break;
                case 7: // update history
                    var list = e.Guids;

                    LbHistory.Items.Clear();
                    foreach (var guid in list)
                        LbHistory.Items.Add(guid.ToString());
                    break;
                case 8: // save game
                    MessageBox.Show(_games[e.Guid], "Game " + (e.Result ? " has been saved. " : " failed to save. "));
                    break;
                case 9: // update save game list
                    var saves = e.Guids;
                    LbSaves.Items.Clear();
                    foreach (var guid in saves)
                        LbSaves.Items.Add(guid.ToString());
                    break;
                case 10: // load game between two players
                    if (e.Result)
                    {
                        var win = new BoardWindow(this, e.Game);
                        _proxy.GetSaves(PlayerName);
                        _games.Add(e.Game.Id, win);
                        LbActiveGames.Items.Add(e.Game.Id.ToString());
                        win.UpdateGui();
                        win.Load();
                        win.Show();
                    }
                    else
                    {
                        MessageBox.Show("Game canot be loaded, oponent dont play");
                    }
                    break;
                case 11:
                    MessageBox.Show("Name is already logged.");
                    Application.Current.Shutdown();
                    break;
            }
        }

        /// <summary>
        /// When closing close all games window and send server information about disconecting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_proxy != null)
                try
                {
                    foreach (var window in _games.Values)
                        window.Close();
                    _proxy.Disconnect();
                    _proxy.Close();
                }
                catch
                {
                    _proxy.Abort();
                }
        }

        /// <summary>
        /// Clicked on new game , send request for new game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click_NewGame(object sender, RoutedEventArgs e)
        {
            var opponent = LbPlayers.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(opponent) || (opponent == PlayerName))
                return;
            _proxy.AskForGame(PlayerName, opponent);
        }

        /// <summary>
        /// Send request to server to handle choosing field in some game.
        /// </summary>
        /// <param name="mainWindowPlayerName">name of player</param>
        /// <param name="gameId"> id of game</param>
        /// <param name="x">x coord of field</param>
        /// <param name="y">y coord of field</param>
        public void FieldChoose(string mainWindowPlayerName, Guid gameId, int x, int y)
        {
            _proxy.FieldChoose(mainWindowPlayerName, gameId, x, y);
        }

        /// <summary>
        /// Resumes closed game window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bShowGame_Click(object sender, RoutedEventArgs e)
        {
            if (LbActiveGames.SelectedIndex != -1)
                try
                {
                    var s = LbActiveGames.SelectedItem as string;
                    if (s != null)
                    {
                        var g = Guid.Parse(s);
                        _games[g].Show();
                    }
                }
                catch
                {
                    // ignored
                }
        }

        /// <summary>
        /// request to play some game history.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bHistory_Click(object sender, RoutedEventArgs e)
        {
            if (LbHistory.SelectedIndex != -1)
                try
                {
                    var s = LbHistory.SelectedItem as string;
                    if (s != null)
                    {
                        var g = Guid.Parse(s);
                        _proxy.GetGameHistory(g, PlayerName);
                    }
                }
                catch
                {
                    // ignored
                }
        }

        /// <summary>
        /// Request to save specified game.
        /// </summary>
        /// <param name="gameId">Id of game to be saved.</param>
        public void Save(Guid gameId)
        {
            _proxy.SaveGame(gameId, PlayerName);
        }

        /// <summary>
        /// Request for load saved game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BLoad_Click(object sender, RoutedEventArgs e)
        {
            if (LbSaves.SelectedIndex != -1)
                try
                {
                    var s = LbSaves.SelectedItem as string;
                    if (s != null)
                    {
                        var g = Guid.Parse(s);
                        _proxy.LoadGame(g);
                    }
                }
                catch
                {
                    // ignored
                }
        }

        /// <summary>
        /// If this window is closed. Dispose all other active windows with games.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}