using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ShapeAbilityContract;
using System.Windows.Shapes;

namespace RectangleAbility
{
    public class RectangleAbility : IShape
    {
        public const int QUARTER_TOP_RIGHT = 0;
        public const int QUARTER_BOTTOM_RIGHT = 1;
        public const int QUARTER_BOTTOM_LEFT = 2;
        public const int QUARTER_TOP_LEFT = 3;
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Rectangle";

        public void UpdateStart(Point p)
        {
            Start = p;
        }
        public void UpdateEnd(Point p)
        {
            End = p;
        }

        public UIElement Draw(Color color, int thickness)
        {
            double width = End.X - Start.X;
            double height = End.Y - Start.Y;
            int quarter = -1;

            if (width >= 0 && height >= 0)
            {
                quarter = QUARTER_BOTTOM_RIGHT;
            }
            else if (width >= 0 && height <= 0)
            {
                quarter = QUARTER_TOP_RIGHT;
            }
            else if (width <= 0 && height <= 0)
            {
                quarter = QUARTER_TOP_LEFT;
            }
            else if (width <= 0 && height >= 0)
            {
                quarter = QUARTER_BOTTOM_LEFT;
            }

            width = Math.Abs(width);
            height = Math.Abs(height);
            var shape = new Rectangle()
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness
            };

            switch (quarter)
            {
                case QUARTER_TOP_RIGHT:
                    Canvas.SetLeft(shape, Start.X);
                    Canvas.SetTop(shape, Start.Y - height);
                    break;
                case QUARTER_BOTTOM_RIGHT:
                    Canvas.SetLeft(shape, Start.X);
                    Canvas.SetTop(shape, Start.Y);
                    break;
                case QUARTER_BOTTOM_LEFT:
                    Canvas.SetLeft(shape, Start.X - width);
                    Canvas.SetTop(shape, Start.Y);
                    break;
                case QUARTER_TOP_LEFT:
                    Canvas.SetLeft(shape, Start.X - width);
                    Canvas.SetTop(shape, Start.Y - height);
                    break;
            }
            return shape;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Point GetStart()
        {
            return Start;
        }

        public Point GetEnd()
        {
            return End;
        }
    }
}
