using Contract;
using System;
using System.Collections.Generic;
using System.Windows;

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
        UIElement Draw(System.Windows.Media.Color color, int thickness);
        bool IsHovering(double x, double y);
        public List<controlPoint> GetControlPoints();
        public UIElement controlOutline();
        public IShape HardCopy();
    }
}
