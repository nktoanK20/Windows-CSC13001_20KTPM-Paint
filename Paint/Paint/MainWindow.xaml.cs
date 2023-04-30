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

using System.Collections;

using System.Globalization;
using System.Threading.Channels;
using System.Windows.Controls.Primitives;
using System.Linq.Expressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.ComponentModel;


namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<string, IShape> _abilities = new Dictionary<string, IShape>();

        bool _isDrawing = false;
        bool _isEditMode = true;
        IShape? _prototype = null;
        string _selectedType = "";
        int _selectedThickness = 2;
        private Color _selectedColor = Colors.Black;
        private DoubleCollection _selectedStrokeType = new DoubleCollection() { 1, 0 };


        Point _start;
        Point _end;

        private List<IShape> _shapes = new List<IShape>();
        private Stack<IShape> _buffer = new Stack<IShape>();
        private List<IShape> _buffer_copy = new List<IShape>();
        private List<IShape> _chosedShapes = new List<IShape>();
        private List<controlPoint> _controlPoints = new List<controlPoint>();
        private double editPreviousX = -1;
        private double editPreviousY = -1;
        private IShape _preview = null;
        private static string _autoSavePath = "autoSave.dat"; // save the file in the project folder
        private bool btnOpenFlag = false;
        Image imageOpenedFromFile = null;


        private readonly MatrixTransform _transform = new MatrixTransform();

        public float Zoomfactor { get; set; } = 1.1f;

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
                if (ability.Key == "Point")
                {
                    continue;
                }

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

            _prototype = _abilities["Line"];
            _shapes = LoadAutoSave();
            foreach (var shape in _shapes)
            {
                Debug.WriteLine($"{shape.getStart()} - {shape.getEnd()}");
                UIElement oldShape = shape.Draw(shape.Color, shape.Thickness, shape.StrokeType);
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
            //FileStream fileStream = null;
            //try
            //{
            //    fileStream = new FileStream(_autoSavePath, FileMode.Truncate, FileAccess.Write);
            //}
            //catch (Exception ex)
            //{
            //    fileStream = new FileStream(_autoSavePath, FileMode.Create, FileAccess.Write);
            //}



            //BinaryWriter writer = new BinaryWriter(fileStream);
            //foreach (var shape in _shapes)
            //{
            //    writer.Write(shape.Name);
            //    //writer.Write(shape.Color);
            //    //writer.Write(shape.StrokeType);
            //    //writer.Write(shape.Thickness);
            //    writer.Write(shape.GetStart().X);
            //    writer.Write(shape.GetStart().Y);
            //    writer.Write(shape.GetEnd().X);
            //    writer.Write(shape.GetEnd().Y);
            //}

            //writer.Close();

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(_autoSavePath, FileMode.Truncate, FileAccess.Write);
            }
            catch (Exception ex)
            {
                fileStream = new FileStream(_autoSavePath, FileMode.Create, FileAccess.Write);
            }

            BinaryWriter writer = new BinaryWriter(fileStream);
            foreach (var shape in _shapes)
            {
                // Lưu thông tin về hình dạng
                writer.Write(shape.Name);
                writer.Write(shape.Thickness);

                // Lưu giá trị của màu sử dụng ColorConverter
                Color color = shape.Color;
                writer.Write(color.A);
                writer.Write(color.R);
                writer.Write(color.G);
                writer.Write(color.B);

                // Lưu giá trị của StrokeDashArray (DoubleCollection)
                DoubleCollection dashArray = shape.StrokeType;
                if (dashArray != null)
                {
                    writer.Write(dashArray.Count);
                    foreach (double value in dashArray)
                    {
                        writer.Write(value);
                    }
                }
                else
                {
                    writer.Write(0); // Không có StrokeDashArray
                }

                // Lưu các thông tin khác về hình dạng (ví dụ: Start, End, v.v.)
                writer.Write(shape.GetStart().X);
                writer.Write(shape.GetStart().Y);
                writer.Write(shape.GetEnd().X);
                writer.Write(shape.GetEnd().Y);
            }

            writer.Close();
        }

        private List<IShape> LoadAutoSave()
        {
            //List<IShape> result = new List<IShape>();
            //try
            //{
            //    FileStream fileStream = new FileStream(_autoSavePath, FileMode.Open, FileAccess.Read);
            //    BinaryReader binaryReader = new BinaryReader(fileStream);

            //    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            //    {
            //        string name = binaryReader.ReadString();

            //        double x = binaryReader.ReadDouble();
            //        double y = binaryReader.ReadDouble();
            //        Point p = new Point(x, y);
            //        IShape shape = (IShape)_abilities[name].Clone();
            //        shape.UpdateStart(p);

            //        x = binaryReader.ReadDouble();
            //        y = binaryReader.ReadDouble();
            //        p = new Point(x, y);
            //        shape.UpdateEnd(p);

            //        result.Add(shape);
            //    }

            //    binaryReader.Close();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}

            //return result;

            List<IShape> result = new List<IShape>();
            try
            {
                FileStream fileStream = new FileStream(_autoSavePath, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);

                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    string name = binaryReader.ReadString();

                    int thickness = binaryReader.ReadInt32();

                    // Đọc giá trị màu ARGB từ file
                    byte a = binaryReader.ReadByte();
                    byte r = binaryReader.ReadByte();
                    byte g = binaryReader.ReadByte();
                    byte b = binaryReader.ReadByte();
                    Color color = Color.FromArgb(a, r, g, b);

                    // Đọc số lượng phần tử trong StrokeDashArray
                    int dashArrayCount = binaryReader.ReadInt32();
                    DoubleCollection dashArray = new DoubleCollection();
                    for (int i = 0; i < dashArrayCount; i++)
                    {
                        double value = binaryReader.ReadDouble();
                        dashArray.Add(value);
                    }

                    double startX = binaryReader.ReadDouble();
                    double startY = binaryReader.ReadDouble();
                    Point start = new Point(startX, startY);

                    double endX = binaryReader.ReadDouble();
                    double endY = binaryReader.ReadDouble();
                    Point end = new Point(endX, endY);

                    // Tạo hình dạng mới và cập nhật các thuộc tính
                    IShape shape = (IShape)_abilities[name].Clone();
                    shape.Thickness = thickness;
                    shape.Color = color;
                    shape.StrokeType = dashArray;
                    shape.UpdateStart(start);
                    shape.UpdateEnd(end);

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

        private void RedrawCanvas()
        {
            actualCanvas.Children.Clear();
            if (btnOpenFlag == true)
            {
                actualCanvas.Children.Add(imageOpenedFromFile);
            }

            foreach (var shape in _shapes)
            {
                UIElement oldShape = shape.Draw(shape.Color, shape.Thickness, shape.StrokeType);
                actualCanvas.Children.Add(oldShape);
            }
            //control Point display ontop
            //rework
            if (_isEditMode && _chosedShapes.Count > 0)
            {
                _chosedShapes.ForEach(shape =>
                {
                    IShape chosedShape = shape;
                    actualCanvas.Children.Add(chosedShape.controlOutline());

                    //if only chose one shape
                    if (_chosedShapes.Count == 1)
                    {
                        List<controlPoint> ctrlPoints = chosedShape.GetControlPoints();
                        this._controlPoints = ctrlPoints;
                        ctrlPoints.ForEach(K =>
                        {
                            actualCanvas.Children.Add(K.drawPoint(chosedShape.getRotateAngle(), chosedShape.getCenterPoint()));
                        });
                    }
                });
            }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (_selectedType.Trim().Length == 0)
            {
                return;
            }

            if (this._isEditMode)
            {
                if (Mouse.RightButton == MouseButtonState.Pressed)
                {
                    _chosedShapes.Clear();
                    RedrawCanvas();
                    return;
                }
                return;
            }

            _isDrawing = true;
            _start = e.GetPosition(actualCanvas);

            _prototype = (IShape)_abilities[_selectedType].Clone();
            _prototype.UpdateStart(_start);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            /*if (!_isDrawing || _prototype == null)
            {
                return;
            }*/
            //


            bool isChange = false;
            if (_chosedShapes.Count == 1)
            {
                IShape shape1 = _chosedShapes[0];
                Point currentPos1 = e.GetPosition(actualCanvas);
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    if (_controlPoints[i].isHovering(shape1.getRotateAngle(), currentPos1.X, currentPos1.Y))
                    {
                        switch (_controlPoints[i].getEdge(shape1.getRotateAngle()))
                        {
                            case "topleft" or "bottomright":
                                {
                                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                                    break;
                                }
                            case "topright" or "bottomleft":
                                {
                                    Mouse.OverrideCursor = Cursors.SizeNESW;
                                    break;
                                }
                            case "top" or "bottom":
                                {
                                    Mouse.OverrideCursor = Cursors.SizeNS;
                                    break;
                                }
                            case "left" or "right":
                                {
                                    Mouse.OverrideCursor = Cursors.SizeWE;
                                    break;
                                }
                        }

                        if (_controlPoints[i].type == "move" || _controlPoints[i].type == "rotate")
                            Mouse.OverrideCursor = Cursors.Hand;

                        isChange = true;
                        break;
                    }
                };

                if (!isChange)
                    Mouse.OverrideCursor = null;
            }


            if (this._isEditMode)
            {
                if (_chosedShapes.Count < 1)
                    return;

                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return;

                Point currentPos = e.GetPosition(actualCanvas);

                double dx, dy;

                if (editPreviousX == -1 || editPreviousY == -1)
                {
                    editPreviousX = currentPos.X;
                    editPreviousY = currentPos.Y;
                    return;
                }

                dx = currentPos.X - editPreviousX;
                dy = currentPos.Y - editPreviousY;

                if (_chosedShapes.Count > 1)
                {
                    //handle multiple shapes

                    _chosedShapes.ForEach(E =>
                    {
                        IShape K = (IShape)E;
                        Point Start = K.getStart();
                        Point End = K.getEnd();
                        Start.X = Start.X + dx;
                        Start.Y = Start.Y + dy;
                        End.X = End.X + dx;
                        End.Y = End.Y + dy;
                        K.UpdateStart(Start);
                        K.UpdateEnd(End);

                    });

                }
                else
                {
                    // handle only one shapes
                    /*
					Console.WriteLine($"dx {dx}| dy {dy}");
					Console.WriteLine($"currentPos {currentPos.X}| {currentPos.Y}");
					Console.WriteLine($"x {editPreviousX}| y {editPreviousY}");
					*/

                    //controlPoint detect part
                    IShape shape = (IShape)_chosedShapes[0];
                    _controlPoints.ForEach(ctrlPoint =>
                    {
                        List<cord> edges = new List<cord>()
                        {
                        new cord(new Point2D(shape.getStart().X, shape.getStart().Y)),      // 0 xt
                        new cordY(new Point2D(shape.getStart().X, shape.getStart().Y)),      // 1 yt
                        new cord(new Point2D(shape.getEnd().X, shape.getEnd().Y)),  // 2 xb
                        new cordY(new Point2D(shape.getEnd().X, shape.getEnd().Y))   // 3 yb
						};

                        List<int> rotate0 = new List<int>
                        {
                        0, 1, 2, 3
                        };
                        List<int> rotate90 = new List<int>
                        {
                        //xt, yt, xb, xb
                        3, 0, 1, 2
                        };
                        List<int> rotate180 = new List<int>
                        {
                        //xt, yt, xb, xb
                        2, 3, 0, 1
                        };
                        List<int> rotate270 = new List<int>
                        {
                        //xt, yt, xb, xb
                        1, 2, 3, 0
                        };

                        List<List<int>> rotateList = new List<List<int>>()
                        {
                        rotate0,
                        rotate90,
                        rotate180,
                        rotate270
                        };

                        double rot = shape.getRotateAngle();
                        int index = 0;

                        if (rot > 0)
                            while (true)
                            {
                                rot -= 90;
                                if (rot < 0)
                                    break;
                                index++;

                                if (index == 4)
                                    index = 0;
                            }
                        else
                            while (true)
                            {
                                rot += 90;
                                if (rot > 0)
                                    break;
                                index--;
                                if (index == -1)
                                    index = 3;
                            };

                        if (ctrlPoint.isHovering(shape.getRotateAngle(), currentPos.X, currentPos.Y))
                        {

                            switch (ctrlPoint.type)
                            {
                                case "rotate":
                                    {
                                        const double RotateFactor = 180.0 / 270;
                                        double alpha = Math.Abs(dx + dy);
                                        Point Start = shape.getStart();
                                        Point End = shape.getEnd();
                                        int indexShapeMove = -1;
                                        for (int i = 0; i < _shapes.Count; i++)
                                        {
                                            if (_shapes[i].getStart().X == Start.X)
                                            {
                                                indexShapeMove = i; break;
                                            }
                                        }
                                        Point2D v = shape.getCenterPoint();

                                        double xv = editPreviousX - v.X;
                                        double yv = editPreviousY - v.Y;

                                        double angle = Math.Atan2(dx * yv - dy * xv, dx * xv + dy * yv);

                                        if (angle > 0)
                                        {
                                            shape.setRotateAngle(shape.getRotateAngle() - alpha * RotateFactor);

                                        }
                                        else
                                        {
                                            shape.setRotateAngle(shape.getRotateAngle() + alpha * RotateFactor);

                                        }
                                        break;
                                    }

                                case "move":

                                    {
                                        Point Start = shape.getStart();
                                        Point End = shape.getEnd();
                                        int indexShapeMove = -1;
                                        for (int i = 0; i < _shapes.Count; i++)
                                        {
                                            if (_shapes[i].getStart().X == Start.X)
                                            {
                                                indexShapeMove = i; break;
                                            }
                                        }

                                        Start.X += dx;
                                        Start.Y += dy;
                                        End.X += dx;
                                        End.Y += dy;



                                        //shape.UpdateStart(Start);
                                        //shape.UpdateEnd(End);
                                        _shapes[indexShapeMove].UpdateStart(Start);
                                        _shapes[indexShapeMove].UpdateEnd(End);

                                        break;
                                    }

                                case "diag":
                                    {
                                        Point2D handledXY = ctrlPoint.handle(shape.getRotateAngle(), dx, dy);

                                        switch (index)
                                        {
                                            case 1:
                                                handledXY.X *= -1;
                                                break;
                                            case 2:
                                                {
                                                    handledXY.Y *= -1;
                                                    handledXY.X *= -1;
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    handledXY.Y *= -1;
                                                    break;
                                                }
                                        }


                                        switch (ctrlPoint.getEdge(shape.getRotateAngle()))
                                        {
                                            case "topleft":
                                            case "left":
                                            //case "bottomleft":
                                            case "top":

                                                {
                                                    Point Start = shape.getStart();
                                                    Point End = shape.getEnd();
                                                    int indexShapeMove = -1;
                                                    for (int i = 0; i < _shapes.Count; i++)
                                                    {
                                                        if (_shapes[i].getStart().X == Start.X)
                                                        {
                                                            indexShapeMove = i; break;
                                                        }
                                                    }
                                                    Start.X += dx;
                                                    Start.Y += dy;
                                                    End.X += dx;
                                                    End.Y += dy;
                                                    _shapes[indexShapeMove].UpdateStart(Start);

                                                    break;

                                                }
                                            //case "topright":
                                            case "right":
                                            case "bottomright":
                                            case "bottom":
                                                {
                                                    Point Start = shape.getStart();
                                                    Point End = shape.getEnd();
                                                    int indexShapeMove = -1;
                                                    for (int i = 0; i < _shapes.Count; i++)
                                                    {
                                                        if (_shapes[i].getStart().X == Start.X)
                                                        {
                                                            indexShapeMove = i; break;
                                                        }
                                                    }
                                                    Start.X += dx;
                                                    Start.Y += dy;
                                                    End.X += dx;
                                                    End.Y += dy;
                                                    _shapes[indexShapeMove].UpdateEnd(End);

                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                        }

                    });
                }


                editPreviousX = currentPos.X;
                editPreviousY = currentPos.Y;

                RedrawCanvas();
                return;
            }

            if (this._isDrawing)
            {
                actualCanvas.Children.Clear();

                if (btnOpenFlag == true)
                {
                    actualCanvas.Children.Add(imageOpenedFromFile);
                }


                foreach (var shape in _shapes)
                {
                    UIElement oldShape = shape.Draw(shape.Color, shape.Thickness, shape.StrokeType);
                    //UIElement oldShape = shape.Draw(_selectedColor, _selectedThickness, _selectedStrokeType);
                    actualCanvas.Children.Add(oldShape);
                }


                _end = e.GetPosition(actualCanvas);
                _prototype.UpdateEnd(_end);

                UIElement newShape = _prototype.Draw(_selectedColor, _selectedThickness, _selectedStrokeType);
                actualCanvas.Children.Add(newShape);
            }



        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!this._isEditMode)
            {
                //_shapes.Add((IShape)_prototype.Clone());
                IShape newShape = _prototype.Clone() as IShape;
                newShape.Thickness = _selectedThickness;
                newShape.StrokeType = _selectedStrokeType;
                newShape.Color = _selectedColor;
                _shapes.Add(newShape);
                _isDrawing = false;
            }

            if (this._isEditMode)
            {
                if (e.ChangedButton != MouseButton.Left)
                    return;
                Point currentPos = e.GetPosition(actualCanvas);
                for (int i = this._shapes.Count - 1; i >= 0; i--)
                {
                    ;
                    IShape temp = _shapes[i];



                    if (temp.IsHovering(Math.Abs(currentPos.X), Math.Abs(currentPos.Y)))
                    {

                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (!_chosedShapes.Contains(_shapes[i]))
                            {
                                this._chosedShapes.Add(_shapes[i]);
                            }
                            else
                                this._chosedShapes.Remove(_shapes[i]);
                        }
                        else
                        {
                            _chosedShapes.Clear();
                            this._chosedShapes.Add(_shapes[i]);
                            _shapes[i].Thickness = _selectedThickness;
                            _shapes[i].Color = _selectedColor;
                            _shapes[i].StrokeType = _selectedStrokeType;
                        }

                        RedrawCanvas();
                        break;
                    }
                }

                this.editPreviousX = -1;
                this.editPreviousY = -1;

                return;
            }

            Point pos = e.GetPosition(actualCanvas);

            // Ddd to shapes list & save it color + thickness


            // Draw new thing -> isSaved = false


            // Re-draw the canvas

            RedrawCanvas();
        }

        private void handleCheckedEdit(object sender, RoutedEventArgs e)
        {
            _isEditMode = true;
        }

        private void handleCheckedDraw(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
        }


        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _shapes.Clear();
            actualCanvas.Children.Clear();
            _chosedShapes.Clear();
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
            Uri uriSource = new Uri(filename, UriKind.Absolute);
            BitmapImage imgTemp = new BitmapImage();
            imgTemp.BeginInit();
            imgTemp.CacheOption = BitmapCacheOption.OnLoad;
            imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            imgTemp.UriSource = uriSource;
            imgTemp.EndInit();
            imageOpenedFromFile.Source = imgTemp;
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

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _chosedShapes.Count > 0)
            {
                if (_chosedShapes.Count == 1)
                {
                    // Choose one shape
                    Point Start = _chosedShapes[0].getStart();
                    int indexShapeCopy = -1;
                    for (int i = 0; i < _shapes.Count; i++)
                    {
                        if (_shapes[i].getStart().X == Start.X)
                        {
                            indexShapeCopy = i;
                        }
                    }

                    IShape shapeCopy = _shapes[indexShapeCopy].HardCopy();
                    Point startCopy = shapeCopy.getStart();
                    Point endCopy = shapeCopy.getEnd();
                    startCopy.X += 10;
                    startCopy.Y += 10;
                    endCopy.X += 10;
                    endCopy.Y += 10;
                    _shapes.Add(shapeCopy);
                    RedrawCanvas();
                }
                else
                {
                    // Choose multi shape
                    List<int> listIndexChooseShapeCopy = new List<int>();
                    _chosedShapes.ForEach(shape =>
                    {

                        Point startMulti = shape.getStart();
                        for (int i = 0; i < _shapes.Count; i++)
                        {
                            if (_shapes[i].getStart().X == startMulti.X)
                            {
                                IShape shapeCopyMulti = _shapes[i].HardCopy();
                                Point startCopy = shapeCopyMulti.getStart();
                                Point endCopy = shapeCopyMulti.getEnd();
                                startCopy.X += 10;
                                startCopy.Y += 10;
                                endCopy.X += 10;
                                endCopy.Y += 10;
                                _shapes.Add(shapeCopyMulti);
                            }
                        }
                    });
                    //_chosedShapes.Clear();
                    RedrawCanvas();

                }
            }
        }
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _chosedShapes.Count > 0)
            {
                if (_chosedShapes.Count == 1) {
                    // Choose one shape
                    Point Start = _chosedShapes[0].getStart();
                    int indexShapeRemove = -1;
                    for (int i = 0; i < _shapes.Count; i++)
                    {
                        if (_shapes[i].getStart().X == Start.X)
                        {
                            indexShapeRemove = i;
                        }
                    }

                    _shapes.RemoveAt(indexShapeRemove);
                    _chosedShapes.Clear();
                    RedrawCanvas();
                }
                else
                {
                    // Choose multi shape
                    List<int> listIndexChooseShapeRemove = new List<int>();
                    _chosedShapes.ForEach(shape =>
                    {

                        Point start = shape.getStart();
                        for (int i = 0; i < _shapes.Count; i++)
                        {
                            if (_shapes[i].getStart().X == start.X)
                            {
                                _shapes.RemoveAt(i);
                                listIndexChooseShapeRemove.Add(i);
                                //_chosedShapes.RemoveAt(i);
                            }
                        }
                    });
                    _chosedShapes.Clear();

                    RedrawCanvas();

                }
            }
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            e.Handled = true;
            float scaleFactor = Zoomfactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
            }

            Point mousePostion = e.GetPosition(this);

            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _transform.Matrix = scaleMatrix;

            actualCanvas.LayoutTransform = _transform;
            aboveCanvas.LayoutTransform = _transform;
            aboveCanvasThumb.Height *= 1f / scaleFactor;
            aboveCanvasThumb.Width *= 1f / scaleFactor;
        }

        private void aboveCanvasScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            actualCanvasScroll.ScrollToVerticalOffset(e.VerticalOffset);
            actualCanvasScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void aboveCanvasThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            //Move the Thumb to the mouse position during the drag operation
            var yadjust = aboveCanvas.Height + e.VerticalChange;
            var xadjust = aboveCanvas.Width + e.HorizontalChange;
            if ((xadjust >= 0) && (yadjust >= 0))
            {
                aboveCanvas.Width = xadjust;
                aboveCanvas.Height = yadjust;
                actualCanvas.Width = xadjust;
                actualCanvas.Height = yadjust;
                Canvas.SetRight(aboveCanvasThumb, 0);
                Canvas.SetBottom(aboveCanvasThumb, 0);
            }
        }

        private void aboveCanvasThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            aboveCanvasThumb.Background = Brushes.Orange;
        }

        private void aboveCanvasThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            aboveCanvasThumb.Background = Brushes.Blue;
        }


        //Change stroke type and pen width
        private void PenWidthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = penWidthComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string penWidthString = selectedItem.Content.ToString();
                int.TryParse(penWidthString, out _selectedThickness);
            }
        }

        private void StrokeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = strokeTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string tag = selectedItem.Tag as string;
                if (!string.IsNullOrEmpty(tag))
                {
                    string[] parts = tag.Split(',');
                    if (parts.Length > 0)
                    {
                        _selectedStrokeType = new DoubleCollection();
                        foreach (string part in parts)
                        {
                            double value;
                            if (double.TryParse(part, out value))
                            {
                                _selectedStrokeType.Add(value);
                            }
                        }
                    }
                }
            }

        }

        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color selectedColor = colorDialog.Color;
                Color wpfColor = Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B);
                _selectedColor = wpfColor;

                // Thay đổi màu nền của Button
                Button button = (Button)sender;
                button.Background = new SolidColorBrush(wpfColor);
            }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_shapes.Count == 0)
                return;
            if (_shapes.Count == 0 && _buffer.Count == 0)
                return;

            // Push last shape into buffer and remove it from final list, then re-draw canvas
            int lastIndex = _shapes.Count - 1;
            _buffer.Push(_shapes[lastIndex]);
            _shapes.RemoveAt(lastIndex);

            RedrawCanvas();
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (_buffer.Count == 0)
                return;
            if (_shapes.Count == 0 && _buffer.Count == 0)
                return;

            // Pop the last shape from buffer and add it to final list, then re-draw canvas
            _shapes.Add(_buffer.Pop());
            RedrawCanvas();

        }
    }
}
