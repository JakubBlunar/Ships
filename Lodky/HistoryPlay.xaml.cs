using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ServerInterfaces;

namespace Lodky
{
    /// <summary>
    /// Window that will play game history. 
    /// Which player where hit and who wins.
    /// </summary>
    public partial class HistoryPlay
    {
        private readonly List<GameMove> _moves;

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _i;

        private Game Game { get; }

        /// <summary>
        /// Creates new window where the history will be played.
        /// </summary>
        /// <param name="g">game which will be played</param>
        public HistoryPlay(Game g)
        {
            InitializeComponent();
            Game = g;
            Title = g.Player1 + " vs " + g.Player2 + " - Game: " + g.Id;
            TbStatus.Text = "";

            for (var i = 0; i < 16; i++)
                for (var j = 0; j < 16; j++)
                {
                    var rec = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        Fill = Brushes.SkyBlue,
                        Height = 20,
                        Width = 20
                    };
                    Canvas1.Children.Add(rec);
                    Canvas.SetTop(rec, i*20);
                    Canvas.SetLeft(rec, j*20);
                }

            for (var i = 0; i < 16; i++)
                for (var j = 0; j < 16; j++)
                {
                    var rec = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        Fill = Brushes.SkyBlue,
                        Height = 20,
                        Width = 20
                    };
                    Canvas2.Children.Add(rec);
                    Canvas.SetTop(rec, i*20);
                    Canvas.SetLeft(rec, j*20);
                }

            _moves = new List<GameMove>(Game.Moves.Count);
            foreach (var move in Game.Moves)
                _moves.Add(move);

            _timer.Interval = TimeSpan.FromMilliseconds(2000);
            _timer.Tick += DrawMove;
        }

        
        /// <summary>
        /// Start presentation of game.
        /// </summary>
        public void Play()
        {
            _timer.Start();
        }

        /// <summary>
        /// Draws one game move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawMove(object sender, EventArgs e)
        {
            var move = _moves[_i];
            var rec = new Rectangle
            {
                Stroke = Brushes.Black,
                Height = 20,
                Width = 20
            };

            if (move.Result == 2)
                rec.Fill = Brushes.Black;
            else if (move.Result == 3)
                rec.Fill = Brushes.Red;

            if (move.PlayerName == Game.Player1)
                Canvas1.Children.Add(rec);
            else
                Canvas2.Children.Add(rec);
            Canvas.SetTop(rec, move.Y*20);
            Canvas.SetLeft(rec, move.X*20);

            TbStatus.Text = move.PlayerName + " x:" + move.X + " y: " + move.Y + (move.Result == 2 ? " miss" : " hit");

            _i++;
            if (_i >= _moves.Count)
            {
                _timer.Stop();
                if (Game.Player1Lives <= 0)
                    TbStatus.Text = Game.Player2 + " won.";
                else
                    TbStatus.Text = Game.Player1 + " won.";
            }
        }
    }
}