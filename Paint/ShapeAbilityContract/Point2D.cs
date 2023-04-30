using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShapeAbilityContract
{
    public class Point2D : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }

        public string Icon { get; }
        public Point2D() { }

        public Point2D(double x, double y)
        {
            X = x; Y = y;
        }

        public SolidColorBrush Brush { get; set; }
        public DoubleCollection StrokeDash { get; set; }
        public string Name => "Point";
        public int Thickness { get; set; }

        public bool isHovering(double x, double y)
        {
            return false;
        }


        public void HandleStart(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void HandleEnd(double x, double y)
        {
            X = x;
            Y = y;
        }


        public UIElement Draw(SolidColorBrush brush, int thickness, DoubleCollection dash)
        {
            Line line = new Line()
            {
                X1 = X,
                Y1 = Y,
                X2 = X,
                Y2 = Y,
                StrokeThickness = thickness,
                Stroke = brush,
                StrokeDashArray = dash
            };

            return line;
        }


        public IShape Clone()
        {
            return new Point2D();
        }
        public Point2D deepCopy()
        {
            Point2D temp = new Point2D();
            temp.Y = Y;
            temp.X = X;
            return temp;
        }

        public void UpdateStart(Point p)
        {
            throw new NotImplementedException();
        }

        public void UpdateEnd(Point p)
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        public bool IsHovering(double x, double y)
        {
            throw new NotImplementedException();
        }

        public List<controlPoint> GetControlPoints()
        {
            throw new NotImplementedException();
        }

        public UIElement controlOutline()
        {
            throw new NotImplementedException();
        }

        public UIElement Draw(Color color, int thickness, DoubleCollection strokeType)
        {
            throw new NotImplementedException();
        }

        public void setRotateAngle(double angle)
        {
            throw new NotImplementedException();
        }

        public double getRotateAngle()
        {
            throw new NotImplementedException();
        }

        public Point2D getCenterPoint()
        {
            throw new NotImplementedException();
        }

        public Point getStart()
        {
            throw new NotImplementedException();
        }

        public Point getEnd()
        {
            throw new NotImplementedException();
        }

        public Point GetStart()
        {
            throw new NotImplementedException();
        }

        public Point GetEnd()
        {
            throw new NotImplementedException();
        }

        public IShape HardCopy()
        {
            throw new NotImplementedException();
        }

        public static implicit operator Point2D(Point v)
        {
            throw new NotImplementedException();
        }

        public Color Color { get; set; }

        public DoubleCollection StrokeType { get; set; }
    }
}
