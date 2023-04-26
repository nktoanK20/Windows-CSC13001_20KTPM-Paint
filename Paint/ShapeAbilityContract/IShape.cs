using System;
using System.Windows;

namespace ShapeAbilityContract
{
    public interface IShape : ICloneable
    {
        string Name { get; }
        void UpdateStart(System.Windows.Point p);

        Point GetStart();
        Point GetEnd();

        void UpdateEnd(System.Windows.Point p);
        UIElement Draw(System.Windows.Media.Color color, int thickness);
    }
}
