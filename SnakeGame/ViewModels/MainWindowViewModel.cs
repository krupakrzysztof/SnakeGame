using DevExpress.Mvvm;
using SnakeGame.Core;
using SnakeGame.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SnakeGame.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            gameTime = new TimeSpan(0);
            _ = TimeSpan.TryParse(RegistryHelper.GetRegistry("TotalGameTime"), out totalGameTime);
            string hiScore = RegistryHelper.GetRegistry("HiScore");
            if (!string.IsNullOrWhiteSpace(hiScore))
            {
                _ = int.TryParse(hiScore, out topScore);
                RaisePropertyChanged(nameof(TopScore));
            }
            LoadedCommand = new DelegateCommand<Canvas>(Loaded);
            SetFlowCommand = new DelegateCommand<Flow>(SetFlow);
            PauseCommand = new DelegateCommand(Pause);

            Snakes.Add(new Snake(Base.Size, Base.Size));
            Snakes[0].Rectangle.Fill = Brushes.Wheat;

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 75)
            };
            timer.Tick += Timer_Tick;

            gameTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 75)
            };
            gameTimer.Tick += (sender, e) =>
            {
                if (isPause || flow == Flow.UnSet)
                {
                    return;
                }
                gameTime = gameTime.Add(new TimeSpan(0, 0, 0, 0, 75));
                totalGameTime = totalGameTime.Add(new TimeSpan(0, 0, 0, 0, 75));
                RaisePropertyChanged(nameof(GameTimeStr));
                RaisePropertyChanged(nameof(TotalGameTimeStr));
            };
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isPause || flow == Flow.UnSet)
            {
                return;
            }
            canMove = !canMove;
            if (!canMove)
            {
                return;
            }

            for (int i = 0; i < Snakes.Count - 1; i++)
            {
                Snakes[i].Top = Snakes[i + 1].Top;
                Snakes[i].Left = Snakes[i + 1].Left;
            }

            Snake lastSnake = Snakes[^1];
            switch (flow)
            {
                case Flow.UnSet:
                    break;
                case Flow.Right:
                    lastSnake.Left += Base.Size;
                    break;
                case Flow.Top:
                    lastSnake.Top -= Base.Size;
                    break;
                case Flow.Left:
                    lastSnake.Left -= Base.Size;
                    break;
                case Flow.Bottom:
                    lastSnake.Top += Base.Size;
                    break;
                default:
                    break;
            }
            List<Snake> copySnakes = new(Snakes);
            _ = copySnakes.Remove(lastSnake);
            if (copySnakes.Any(x => x.Left == lastSnake.Left && x.Top == lastSnake.Top))
            {
                GameOver();
            }
            if (lastSnake.Top < 0 || lastSnake.Left < 0 || lastSnake.Top >= Base.Size * Base.MapSizeY || lastSnake.Left >= Base.Size * Base.MapSizeX)
            {
                GameOver();
            }
            if (Snakes[^1].Left == Food.Left && Snakes[^1].Top == Food.Top)
            {
                EatFood();
            }
        }

        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer gameTimer;
        private Canvas canvas;
        private Flow flow;
        private bool canMove = true;
        private bool isPause;
        private int topScore;
        private readonly List<int> scoreHistory = new();

        private List<Snake> Snakes { get; set; } = new List<Snake>();

        private Food Food;
        public string Score => $"Wynik: {Snakes.Count - 1}";
        public string TopScore => $"Najlepszy wynik: {topScore}";
        public string AverageScore => $"Średni wynik: {(scoreHistory.Count > 0 ? Math.Round(scoreHistory.Average(), 2) : 0)} na {scoreHistory.Count} gier";

        private TimeSpan gameTime;
        private TimeSpan totalGameTime;

        public string GameTimeStr => $"Czas gry {gameTime:hh\\:mm\\:ss}";

        public string TotalGameTimeStr => $"Łączny czas gry {(totalGameTime.ToString(totalGameTime.Days > 0 ? "dd\\:hh\\:mm\\:ss" : "hh\\:mm\\:ss"))}";

        private UserControl view;
        public UserControl View
        {
            get => view;
            set => SetProperty(ref view, value, nameof(View));
        }

        public ICommand LoadedCommand { get; private set; }
        public ICommand SetFlowCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }

        private void EatFood()
        {
            if (flow == Flow.UnSet)
            {
                return;
            }
            int count = Food.IsExtra ? 2 : 1;
            if (flow == Flow.Right)
            {
                for (int i = 0; i < count; i++)
                {
                    Snakes.Insert(0, new Snake(Snakes[^1].Top, Snakes[^1].Left - Base.Size));
                }
            }
            if (flow == Flow.Top)
            {
                for (int i = 0; i < count; i++)
                {
                    Snakes.Insert(0, new Snake(Snakes[^1].Top + Base.Size, Snakes[^1].Left));
                }
            }
            if (flow == Flow.Left)
            {
                for (int i = 0; i < count; i++)
                {
                    Snakes.Insert(0, new Snake(Snakes[^1].Top, Snakes[^1].Left + Base.Size));
                }
            }
            if (flow == Flow.Bottom)
            {
                for (int i = 0; i < count; i++)
                {
                    Snakes.Insert(0, new Snake(Snakes[^1].Top - Base.Size, Snakes[^1].Left));
                }
            }

            // dodanie wszystkich wystąpień węża do widoku
            for (int i = 0; i < count; i++)
            {
                canvas.Children.Insert(0, Snakes[i].Rectangle);
            }

            canvas.Children.Remove(Food.Ellipse);
            AddFood(canvas);
            RaisePropertyChanged(nameof(Score));
        }

        private void Loaded(Canvas obj)
        {
            canvas = obj;
            AddFood(canvas);

            _ = canvas.Children.Add(Snakes.FirstOrDefault().Rectangle);
            timer.Start();
            gameTimer.Start();
        }

        private void SetFlow(Flow flow)
        {
            if (this.flow == flow)
            {
                Timer_Tick(null, null);
            }
            if (isPause || (flow == Flow.Bottom && this.flow == Flow.Top) || (flow == Flow.Top && this.flow == Flow.Bottom) || (flow == Flow.Left && this.flow == Flow.Right) || (flow == Flow.Right && this.flow == Flow.Left))
            {
                return;
            }
            this.flow = flow;
        }

        private void Pause()
        {
            isPause = !isPause;
        }

        private void GameOver()
        {
            flow = Flow.UnSet;
            _ = MessageBox.Show($"Przegałeś twój wynik to: {Snakes.Count - 1}");
            scoreHistory.Add(Snakes.Count - 1);
            RaisePropertyChanged(nameof(AverageScore));
            Snakes.ForEach(x =>
            {
                canvas.Children.Remove(x.Rectangle);
            });

            canvas.Children.Remove(Food.Ellipse);
            Food = new Food();
            _ = canvas.Children.Add(Food.Ellipse);

            if (Snakes.Count - 1 > topScore)
            {
                topScore = Snakes.Count - 1;
                RaisePropertyChanged(nameof(TopScore));
                RegistryHelper.WriteRegistry("HiScore", $"{topScore}");
            }

            Snakes = new List<Snake>()
            {
                new Snake(Base.Size, Base.Size)
            };
            Snakes[0].Rectangle.Fill = Brushes.Wheat;
            _ = canvas.Children.Add(Snakes.FirstOrDefault().Rectangle);
            RaisePropertyChanged(nameof(Score));
            gameTime = new TimeSpan(0);
            RaisePropertyChanged(nameof(GameTimeStr));

            totalGameTime = new TimeSpan(totalGameTime.Days, totalGameTime.Hours, totalGameTime.Minutes, totalGameTime.Seconds);
            RegistryHelper.WriteRegistry("TotalGameTime", $"{totalGameTime:dd\\:hh\\:mm\\:ss}");
        }

        private void AddFood(Panel panel)
        {
            if (Food != null && panel.Children.OfType<Food>().FirstOrDefault(x => x.Left == Food.Left && x.Top == Food.Top) != null)
            {
                panel.Children.Remove(Food.Ellipse);
            }

            Food = new Food();
            _ = panel.Children.Add(Food.Ellipse);
        }
    }
}
