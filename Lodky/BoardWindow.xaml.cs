using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ServerInterfaces;

namespace Lodky
{
    /// <summary>
    /// Window that represents game between two players.
    /// Player can choose fields and hit opponent fields.
    /// </summary>
    public partial class BoardWindow
    {
        private readonly MainWindow _mainWindow;
        public Game Game { private get; set; }

        /// <summary>
        /// Create new window with game.
        /// </summary>
        /// <param name="m">main form</param>
        /// <param name="g"> game that will be shown by this window</param>
        public BoardWindow(MainWindow m, Game g)
        {
            InitializeComponent();
            _mainWindow = m;
            Game = g;
            Title = _mainWindow.PlayerName + "- Game: " + g.Id;

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
        }

        /// <summary>
        /// Updates window after game change.
        /// </summary>
        public void UpdateGui()
        {
            if (Game.State == GameState.Init)
            {
                Status.Text = "On right side choose 10 boats.";
                Player1Lives.Text = "";
                Player2Lives.Text = "";
            }
            else if (Game.State == GameState.Play)
            {
                if (Game.Player1Turn)
                    Status.Text = Game.Player1 + "'s turn";
                else
                    Status.Text = Game.Player2 + "'s turn";
                Player1Lives.Text = Game.Player1 + " lives: " + Game.Player1Lives;
                Player2Lives.Text = Game.Player2 + " lives: " + Game.Player2Lives;
            }
            else
            {
                var who = Game.Player1;
                if (Game.Player1Lives <= 0)
                    who = Game.Player2;
                Status.Text = who + " wins. Game ended!";
                MessageBox.Show(who + " wins");
            }
        }

        /// <summary>
        /// Event for clicking som field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int x, y;
            var lava = false;
            if (e.GetPosition(this).X > 360)
            {
                x = ((int) Math.Round(e.GetPosition(this).X) - 365)/20;
                y = ((int) Math.Round(e.GetPosition(this).Y) - 10)/20;
            }
            else
            {
                x = ((int) Math.Round(e.GetPosition(this).X) - 10)/20;
                y = ((int) Math.Round(e.GetPosition(this).Y) - 10)/20;
                lava = true;
            }

            if (Game == null)
                return;

            if ((Game.State == GameState.Init) && !lava)
            {
                if ((Game.Player1 == _mainWindow.PlayerName) && (Game.Player1Lives >= 10))
                    return;

                if ((Game.Player2 == _mainWindow.PlayerName) && (Game.Player2Lives >= 10))
                    return;

                if ((Game.Player1 == _mainWindow.PlayerName) && (Game.Board1[y*16 + x] != 1))
                {
                    _mainWindow.FieldChoose(_mainWindow.PlayerName, Game.Id, x, y);
                    var rect = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        Fill = Brushes.Green,
                        Height = 20,
                        Width = 20
                    };
                    Canvas2.Children.Add(rect);
                    Canvas.SetTop(rect, y*20);
                    Canvas.SetLeft(rect, x*20);
                }

                if ((Game.Player2 == _mainWindow.PlayerName) && (Game.Board2[y*16 + x] != 1))
                {
                    _mainWindow.FieldChoose(_mainWindow.PlayerName, Game.Id, x, y);
                    var rect = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        Fill = Brushes.Green,
                        Height = 20,
                        Width = 20
                    };
                    Canvas2.Children.Add(rect);
                    Canvas.SetTop(rect, y*20);
                    Canvas.SetLeft(rect, x*20);
                }
            }
            else if ((Game.State == GameState.Play) && lava)
            {
                if (Game.Player1Turn && (Game.Player1 == _mainWindow.PlayerName) && (Game.Board2[y*16 + x] < 2))
                {
                    _mainWindow.FieldChoose(_mainWindow.PlayerName, Game.Id, x, y);
                    var rect = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        Height = 20,
                        Width = 20,
                        Fill = Game.Board2[y*16 + x] == 1 ? Brushes.Red : Brushes.Black
                    };

                    Canvas1.Children.Add(rect);
                    Canvas.SetTop(rect, y*20);
                    Canvas.SetLeft(rect, x*20);
                    Game.Player1Turn = false;
                }

                if (!Game.Player1Turn && (Game.Player2 == _mainWindow.PlayerName) && (Game.Board1[y*16 + x] < 2))
                {
                    _mainWindow.FieldChoose(_mainWindow.PlayerName, Game.Id, x, y);
                    var rect = new Rectangle
                    {
                        Stroke = Brushes.Black,
                        Height = 20,
                        Width = 20,
                        Fill = Game.Board1[y*16 + x] == 1 ? Brushes.Red : Brushes.Black
                    };

                    Canvas1.Children.Add(rect);
                    Canvas.SetTop(rect, y*20);
                    Canvas.SetLeft(rect, x*20);
                    Game.Player1Turn = true;
                }
            }
        }

        /// <summary>
        /// When player gets hit from opponent.
        /// Shows when opponent hit 
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <param name="res">if ship was hit or not</param>
        public void HitFromPlayer(int x, int y, int res)
        {
            var rect = new Rectangle
            {
                Height = 20,
                Width = 20
            };
            if (res == 2)
            {
                rect.Stroke = Brushes.White;
                rect.Fill = Brushes.White;
            }
            else if (res == 3)
            {
                rect.Stroke = Brushes.Black;
                rect.Fill = Brushes.Red;
            }

            Canvas2.Children.Add(rect);
            Canvas.SetTop(rect, y*20);
            Canvas.SetLeft(rect, x*20);
        }

        /// <summary>
        /// Cancel closing this window if game dont end.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Game.State != GameState.End)
            {
                e.Cancel = true;
                Hide();
            }
        }

        /// <summary>
        /// When window is opened shows prompt to player
        /// </summary>
        public new void Show()
        {
            if (Game.State == GameState.Init)
                MessageBox.Show("On the right side choose 10 fields that will represent your ships.");
            base.Show();
        }

        /// <summary>
        /// Save actual game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BSave_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Save(Game.Id);
        }

        /// <summary>
        /// Set window to state of game to be loaded.
        /// </summary>
        public void Load()
        {
            if (Game.Player1 == _mainWindow.PlayerName)
                for (var i = 0; i < 16; i++)
                    for (var j = 0; j < 16; j++)
                    {
                        var rect = new Rectangle
                        {
                            Height = 20,
                            Width = 20
                        };

                        if (Game.Board1[j*16 + i] == 1)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Green;
                        }
                        else if (Game.Board1[j*16 + i] == 2)
                        {
                            rect.Stroke = Brushes.White;
                            rect.Fill = Brushes.White;
                        }
                        else if (Game.Board1[j*16 + i] == 3)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Red;
                        }

                        Canvas2.Children.Add(rect);
                        Canvas.SetTop(rect, j*20);
                        Canvas.SetLeft(rect, i*20);
                    }
            else
                for (var i = 0; i < 16; i++)
                    for (var j = 0; j < 16; j++)
                    {
                        var rect = new Rectangle
                        {
                            Height = 20,
                            Width = 20
                        };

                        if (Game.Board2[j*16 + i] == 1)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Green;
                        }
                        else if (Game.Board2[j*16 + i] == 2)
                        {
                            rect.Stroke = Brushes.White;
                            rect.Fill = Brushes.White;
                        }
                        else if (Game.Board2[j*16 + i] == 3)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Red;
                        }

                        Canvas2.Children.Add(rect);
                        Canvas.SetTop(rect, j*20);
                        Canvas.SetLeft(rect, i*20);
                    }


            for (var i = 0; i < 16; i++)
                for (var j = 0; j < 16; j++)
                {
                    if (Game.Player1 == _mainWindow.PlayerName)
                    {
                        var rect = new Rectangle
                        {
                            Height = 20,
                            Width = 20
                        };

                        if (Game.Board2[j*16 + i] == 2)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Black;
                        }
                        else if (Game.Board2[j*16 + i] == 3)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Red;
                        }

                        Canvas1.Children.Add(rect);
                        Canvas.SetTop(rect, j*20);
                        Canvas.SetLeft(rect, i*20);
                    }

                    if (Game.Player2 == _mainWindow.PlayerName)
                    {
                        var rect = new Rectangle
                        {
                            Height = 20,
                            Width = 20
                        };

                        if (Game.Board1[j*16 + i] == 2)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Black;
                        }
                        else if (Game.Board1[j*16 + i] == 3)
                        {
                            rect.Stroke = Brushes.Black;
                            rect.Fill = Brushes.Red;
                        }

                        Canvas1.Children.Add(rect);
                        Canvas.SetTop(rect, j*20);
                        Canvas.SetLeft(rect, i*20);
                    }
                }
        }
    }
}