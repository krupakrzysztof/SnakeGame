using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnakeGame.Core
{
    internal class Snake
    {
        public Snake(double top, double left)
        {
            Rectangle = new Rectangle()
            {
                Width = Base.Size,
                Height = Base.Size,
                Fill = Brushes.Red,
                Stroke = Brushes.Black
            };
            Top = top;
            Left = left;
        }

        private double top;
        public double Top
        {
            get => top;
            set
            {
                top = value;
                Rectangle.SetValue(Canvas.TopProperty, top);
            }
        }

        private double left;
        public double Left
        {
            get => left;
            set
            {
                left = value;
                Rectangle.SetValue(Canvas.LeftProperty, left);
            }
        }

        public Rectangle Rectangle { get; }

        public override string ToString()
        {
            return $"T: {Top}; L: {Left}";
        }
    }
}
