using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnakeGame.Core
{
    internal class Food
    {
        public Food()
        {
            Ellipse = new Ellipse()
            {
                Width = Base.Size,
                Height = Base.Size,
                Fill = Brushes.Blue,
                Stroke = Brushes.Black
            };
            Top = random.Next(0, Base.MapSizeY) * Base.Size;
            Left = random.Next(0, Base.MapSizeX) * Base.Size;

            int rand = new Random().Next(0, 10);
            if (rand == 5)
            {
                IsExtra = true;
                Ellipse.Fill = Brushes.YellowGreen;
            }
        }

        private static readonly Random random = new();

        private double top;
        public double Top
        {
            get => top;
            set
            {
                top = value;
                Ellipse.SetValue(Canvas.TopProperty, top);
            }
        }

        private double left;
        public double Left
        {
            get => left;
            set
            {
                left = value;
                Ellipse.SetValue(Canvas.LeftProperty, left);
            }
        }

        public bool IsExtra { get; }

        public Ellipse Ellipse { get; }

        public override string ToString()
        {
            return $"T: {Top}; L: {Left}";
        }
    }
}
