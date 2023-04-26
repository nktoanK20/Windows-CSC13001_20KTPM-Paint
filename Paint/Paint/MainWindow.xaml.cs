using Microsoft.Win32;
using ShapeAbilityContract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<string, IShape> _abilities = new Dictionary<string, IShape>();

        bool _isDrawing = false;
        IShape? _prototype = null;
        string _selectedType = "";
        Color _selectedColor = Colors.Black;
        int _selectedThickness = 2;


        Point _start;
        Point _end;

        List<IShape> _shapes = null;

        private static string _autoSavePath = "autoSave.dat"; // save the file in the project folder
        private bool btnOpenFlag = false;
        Image imageOpenedFromFile = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Tự scan chương trình nạp lên các khả năng của mình
            var domain = AppDomain.CurrentDomain;
            var folder = domain.BaseDirectory;
            var folderInfo = new DirectoryInfo(folder);
            var dllFiles = folderInfo.GetFiles("*.dll");

            foreach (var dll in dllFiles)
            {
                Debug.WriteLine(dll.FullName);
                var assembly = Assembly.LoadFrom(dll.FullName);

                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass && typeof(IShape).IsAssignableFrom(type))
                    {
                        var shape = Activator.CreateInstance(type) as IShape;
                        _abilities.Add(shape!.Name, shape);
                    }
                }
            }

            foreach (var ability in _abilities)
            {
                var button = new Button()
                {
                    Width = 80,
                    Height = 35,
                    Content = ability.Value.Name,
                    Tag = ability.Value.Name
                };
                button.Click += ability_Click;
                abilitiesStackPanel.Children.Add(button);
            }

            _shapes = LoadAutoSave();
            foreach (var shape in _shapes)
            {
                UIElement oldShape = shape.Draw(_selectedColor, _selectedThickness);
                actualCanvas.Children.Add(oldShape);
            }
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Save current list of drawn objects into binary file for continuing later
            if (btnOpenFlag == true)
            {
                _shapes.Clear();
            }
            AutoSave();
        }

        private void AutoSave()
        {
            FileStream fileStream = new FileStream(_autoSavePath, FileMode.Truncate, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fileStream);
            foreach (var shape in _shapes)
            {
                writer.Write(shape.Name);
                writer.Write(shape.GetStart().X);
                writer.Write(shape.GetStart().Y);
                writer.Write(shape.GetEnd().X);
                writer.Write(shape.GetEnd().Y);
            }

            writer.Close();
        }

        private List<IShape> LoadAutoSave()
        {
            List<IShape> result = new List<IShape>();
            try
            {
                FileStream fileStream = new FileStream(_autoSavePath, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);

                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    string name = binaryReader.ReadString();

                    double x = binaryReader.ReadDouble();
                    double y = binaryReader.ReadDouble();
                    Point p = new Point(x, y);
                    IShape shape = (IShape)_abilities[name].Clone();
                    shape.UpdateStart(p);

                    x = binaryReader.ReadDouble();
                    y = binaryReader.ReadDouble();
                    p = new Point(x, y);
                    shape.UpdateEnd(p);

                    result.Add(shape);
                }

                binaryReader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return result;
        }

        private void ability_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            string name = (string)button.Tag;
            _selectedType = name;
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedType.Trim().Length == 0)
            {
                return;
            }

            _isDrawing = true;
            _start = e.GetPosition(actualCanvas);

            _prototype = (IShape)_abilities[_selectedType].Clone();
            _prototype.UpdateStart(_start);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing || _prototype == null)
            {
                return;
            }

            actualCanvas.Children.Clear();

            if (btnOpenFlag == true)
            {
                actualCanvas.Children.Add(imageOpenedFromFile);
            }

            foreach (var shape in _shapes)
            {
                UIElement oldShape = shape.Draw(_selectedColor, _selectedThickness);
                actualCanvas.Children.Add(oldShape);
            }

            _end = e.GetPosition(actualCanvas);
            _prototype.UpdateEnd(_end);

            UIElement newShape = _prototype.Draw(_selectedColor, _selectedThickness);
            actualCanvas.Children.Add(newShape);
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing || _prototype == null)
            {
                return;
            }

            _shapes.Add((IShape)_prototype.Clone());
            _isDrawing = false;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _shapes.Clear();
            actualCanvas.Children.Clear();
            btnOpenFlag = false;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png";
            Nullable<bool> result = dlg.ShowDialog();
            string filename;
            if (result == true)
            {
                filename = dlg.FileName;
            }
            else
            {
                return;
            }
            btnOpenFlag = true;
            _shapes.Clear();
            actualCanvas.Children.Clear();

            imageOpenedFromFile = new Image();
            imageOpenedFromFile.Source = new BitmapImage(new Uri(filename, UriKind.Absolute));
            Canvas.SetLeft(imageOpenedFromFile, 0);
            Canvas.SetTop(imageOpenedFromFile, 0);
            actualCanvas.Children.Add(imageOpenedFromFile);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Files (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }


            Rect bounds = VisualTreeHelper.GetDescendantBounds(actualCanvas);
            double dpi = 96d;
            RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(actualCanvas);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);


            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                pngEncoder.Save(ms);
                ms.Close();
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, ms.ToArray());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
