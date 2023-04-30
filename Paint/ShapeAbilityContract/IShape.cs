using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ShapeAbilityContract
{
    public interface IShape : ICloneable
    {

        string Name { get; }
        
        public void setRotateAngle(double angle);
        public double getRotateAngle();
        public Point2D getCenterPoint();
        Point getStart();
        Point getEnd();
        void UpdateStart(System.Windows.Point p);

        Point GetStart();
        Point GetEnd();

        void UpdateEnd(System.Windows.Point p);
        UIElement Draw(System.Windows.Media.Color color, int thickness, DoubleCollection StrokeType);
        bool IsHovering(double x, double y);
        public List<controlPoint> GetControlPoints();
        public UIElement controlOutline();

        int Thickness { get; set; }
        Color Color { get; set; }
        DoubleCollection StrokeType { get; set; }
    }
}
