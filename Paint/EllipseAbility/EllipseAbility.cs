using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using ShapeAbilityContract;
using Contract;
using System.Collections.Generic;

namespace EllipseAbility
{
    public class EllipseAbility : IShape
    {
        public const int QUARTER_TOP_RIGHT = 0;
        public const int QUARTER_BOTTOM_RIGHT = 1;
        public const int QUARTER_BOTTOM_LEFT = 2;
        public const int QUARTER_TOP_LEFT = 3;
        protected double _rotateAngle = 0;

        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Ellipse";

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
            var shape = new Ellipse()
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

            RotateTransform transform = new RotateTransform(this._rotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;

            shape.RenderTransform = transform;

            return shape;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool IsHovering(double x, double y)
        {
            return util.isBetween(x, this.End.X, this.Start.X)
                && util.isBetween(y, this.End.Y, this.Start.Y);
        }

        public List<controlPoint> GetControlPoints()
        {
            List<controlPoint> controlPoints = new List<controlPoint>();

            controlPoint diagPointTopLeft = new diagPoint();
            diagPointTopLeft.setPoint(this.Start.X, this.Start.Y);

            controlPoint diagPointBottomLeft = new diagPoint();
            diagPointBottomLeft.setPoint(this.Start.X, this.End.Y);

            controlPoint diagPointTopRight = new diagPoint();
            diagPointTopRight.setPoint(this.End.X, this.Start.Y);

            controlPoint diagPointBottomRight = new diagPoint();
            diagPointBottomRight.setPoint(this.End.X, this.End.Y);

            //one way control Point

            controlPoint diagPointRight = new oneSidePoint();
            diagPointRight.setPoint(this.End.X, (this.End.Y + this.Start.Y) / 2);

            controlPoint diagPointLeft = new oneSidePoint();
            diagPointLeft.setPoint(this.Start.X, (this.End.Y + this.Start.Y) / 2);

            controlPoint diagPointTop = new oneSidePoint();
            diagPointTop.setPoint((this.Start.X + this.End.X) / 2, this.Start.Y);

            controlPoint diagPointBottom = new oneSidePoint();
            diagPointBottom.setPoint((this.Start.X + this.End.X) / 2, this.End.Y);


            controlPoint angleControlPoint = new rotatePoint();
            angleControlPoint.setPoint((this.End.X + this.Start.X) / 2, Math.Min(this.End.Y, this.Start.Y) - 50);

            controlPoint moveControlPoint = new controlPoint();
            moveControlPoint.setPoint((this.Start.X + this.End.X) / 2, (this.Start.Y + this.End.Y) / 2);
            moveControlPoint.type = "move";

            controlPoints.Add(diagPointTopLeft);
            controlPoints.Add(diagPointTopRight);
            controlPoints.Add(diagPointBottomLeft);
            controlPoints.Add(diagPointBottomRight);

            controlPoints.Add(diagPointRight);
            controlPoints.Add(diagPointLeft);
            controlPoints.Add(diagPointBottom);
            controlPoints.Add(diagPointTop);

            controlPoints.Add(angleControlPoint);
            controlPoints.Add(moveControlPoint);

            return controlPoints;
        }

        public UIElement controlOutline()
        {
            var left = Math.Min(this.End.X, this.Start.X);
            var top = Math.Min(this.End.Y, this.Start.Y);

            var right = Math.Max(this.End.X, this.Start.X);
            var bottom = Math.Max(this.End.Y, this.Start.Y);

            var width = right - left;
            var height = bottom - top;

            var rect = new Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = 2,
                Stroke = Brushes.Black,
                StrokeDashArray = { 4, 2, 4 }
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);

            RotateTransform transform = new RotateTransform(this._rotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;

            rect.RenderTransform = transform;

            return rect;
        }

        public void setRotateAngle(double angle)
        {
            this._rotateAngle= angle;
        }

        public double getRotateAngle()
        {
            return this._rotateAngle;
        }

        public Point2D getCenterPoint()
        {
            Point2D centerPoint = new Point2D();
            var left = Math.Min(this.End.X, this.Start.X);
            var top = Math.Min(this.End.Y, this.Start.Y);

            centerPoint.X = ((this.Start.X + this.End.X) / 2);
            centerPoint.Y = ((this.Start.Y + this.End.Y) / 2);
            return centerPoint;
        }

        public Point getStart()
        {
            return this.Start;
        }

        public Point getEnd()
        {
            return this.End;
        }

        public Point GetStart()
        {
            return this.Start;
        }

        public Point GetEnd()
        {
            return this.Start;
        }

        public IShape HardCopy()
        {
            EllipseAbility temp = new EllipseAbility();
            temp.Start = this.Start;
            
            temp.End = this.End;
            temp._rotateAngle= this._rotateAngle;
            return temp;
        }
    }
}
