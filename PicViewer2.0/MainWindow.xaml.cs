using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PicViewer2._0
{
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        static uint MaxStartSize = Properties.Settings.Default.MaxStartSize;
        static uint MinimumSize = Properties.Settings.Default.MinimumSize;
        static double ScaleMultiplierUp = Properties.Settings.Default.ScaleMultiplierUp;
        static double ScaleMultiplierDown = Properties.Settings.Default.ScaleMultiplierDown;

        static int ScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
        static int ScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;

        bool Oversize = false;
        bool IsBackgroundTransparent = true;

        double WidthDouble, HeightDouble;
        Bitmap LoadedImage, MaxNativeSizeImage;

        System.Windows.Point CurrentPoint;

        double GridZoomValue = 1.0;
        double CanvasZoomValue = 1.0;


        /// <summary>
        /// Реализует анимацию появления окна
        /// </summary>
        private void FadeInAnimation()
        {
            DoubleAnimation _FadeIn = new DoubleAnimation(1, new TimeSpan(0, 0, 0, 0, 300));
            this.BeginAnimation(OpacityProperty, _FadeIn);
        }


        /// <summary>
        /// Реализует анимацию закрытия окна
        /// </summary>
        private void FadeOutAnimation()
        {
            DoubleAnimation _FadeOut = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 100));
            _FadeOut.Completed += FadeOutAnimation_Completed;
            this.BeginAnimation(OpacityProperty, _FadeOut);
        }


        /// <summary>
        /// Конвертирует Bitmap в BitmapImage 
        /// </summary>
        private BitmapImage ConvertToBitmapImage(Bitmap picture)
        {
            MemoryStream _MemStr = new MemoryStream();
            picture.Save(_MemStr, ImageFormat.Png);
            _MemStr.Position = 0;
            BitmapImage _Out = new BitmapImage();
            _Out.BeginInit();
            _Out.StreamSource = _MemStr;
            _Out.EndInit();
            return _Out;
        }


        /// <summary>
        /// Загружает изображение из файла в LoadedImage
        /// </summary>
        private void LoadImageFromFile(string path)
        {
            LoadedImage = new Bitmap(path);
        }


        /// <summary>
        /// Устанавливает фоном канвы изображение
        /// </summary>
        private void SetBackBitmapImage(BitmapImage image)
        {
            DrawingCanvas.Background = new ImageBrush(image);
        }


        /// <summary>
        /// Передаёт изображение максимального нативного разрешения в MaxNativeSizeImage
        /// </summary>
        private void CreateMaxNativeSizeImage()
        {
            if ((LoadedImage.Height > ScreenHeight) && (LoadedImage.Height >= LoadedImage.Width))
            {
                MaxNativeSizeImage = new Bitmap(LoadedImage, Convert.ToInt32((Convert.ToDouble(ScreenHeight) / Convert.ToDouble(LoadedImage.Height)) * LoadedImage.Width), ScreenHeight);
                return;
            }

            if ((LoadedImage.Width > ScreenWidth) && (LoadedImage.Width >= LoadedImage.Height))
            {
                MaxNativeSizeImage = new Bitmap(LoadedImage, ScreenWidth, Convert.ToInt32((Convert.ToDouble(ScreenWidth) / Convert.ToDouble(LoadedImage.Width)) * LoadedImage.Height));
                return;
            }

            MaxNativeSizeImage = LoadedImage;
        }


        /// <summary>
        /// Закрывает приложение после окончания анимации исчезновения
        /// </summary>
        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            this.Hide();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path += "\\PicViewer\\";
            Directory.CreateDirectory(path);
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                { }
            }

            Environment.Exit(0);
        }


        /// <summary>
        /// Перемещает главное окно при зажатии на нём ЛКМ
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        /// <summary>
        /// Загружает изображение и запускает инициализацию главного окна
        /// </summary>
        public MainWindow()
        {
            string[] args = Environment.GetCommandLineArgs();

            try
            {
                LoadImageFromFile(args[1]);
            }
            catch
            {
                Environment.Exit(0);
            }

            InitializeComponent();
            this.Title = "PicViewer2.0 - " + System.IO.Path.GetFileName(args[1]);
        }


        /// <summary>
        /// Рассчитывает первоначальные размеры изображения, устанавливает фон и перемещает на место курсора
        /// </summary>
        private void Window_Initialized(object sender, EventArgs e)
        {
            double _LoadedImageHeight = LoadedImage.Height;
            double _LoadedImageWidth = LoadedImage.Width;

            CreateMaxNativeSizeImage();

            if (_LoadedImageWidth > _LoadedImageHeight)
            {
                if (_LoadedImageWidth > MaxStartSize)
                {
                    WidthDouble = MaxStartSize;
                }
                else
                {
                    WidthDouble = LoadedImage.Width;
                }

                HeightDouble = _LoadedImageHeight / (_LoadedImageWidth / WidthDouble);
            }
            else
            {
                if (_LoadedImageHeight > MaxStartSize)
                {
                    HeightDouble = MaxStartSize;
                }
                else
                {
                    HeightDouble = LoadedImage.Height;
                }

                WidthDouble = _LoadedImageWidth / (_LoadedImageHeight / HeightDouble);
            }

            System.Windows.Point _Mouse = NativeMethods.GetMousePosition();
            this.Top = _Mouse.Y - (HeightDouble / 2d);
            this.Left = _Mouse.X - (WidthDouble / 2d);

            Grid.Height = HeightDouble;
            Grid.Width = WidthDouble;

            SetBackBitmapImage(ConvertToBitmapImage(MaxNativeSizeImage));
        }


        /// <summary>
        /// Запускает анимацию появления
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FadeInAnimation();
        }


        /// <summary>
        /// Определяет приведёт ли увеличение к выходу за максимальные границы
        /// </summary>
        private bool WillScaleUpOversize(double zoomStep)
        {
            if ((WidthDouble * (GridZoomValue + zoomStep)) > ScreenWidth)
            {
                return true;
            }

            if ((HeightDouble * (GridZoomValue + zoomStep)) > ScreenHeight)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Определяет приведёт ли уменьшение к выходу за минимальные границы
        /// </summary>
        private bool WillScaleDownUndersize(double zoomStep)
        {
            if ((WidthDouble * (GridZoomValue - zoomStep)) < MinimumSize)
            {
                return true;
            }

            if ((HeightDouble * (GridZoomValue - zoomStep)) < MinimumSize)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Если type = 1 то Сохраняет 2.png вместе с нарисованными элементами в AppData/Roaming/PicViewer
        /// Если type != 1 то выводит окно для выбора места сохранения
        /// </summary>
        private void SaveDrawingToPNG(int type)
        {
            RenderTargetBitmap _RenderTargetBitmap = new RenderTargetBitmap((int)LoadedImage.Width, (int)LoadedImage.Height, 96d, 96d, PixelFormats.Default);

            DrawingCanvas.Background = new SolidColorBrush(Colors.Transparent);
            DrawingCanvas.UpdateLayout();

            DrawingVisual _DrawingVisual = new DrawingVisual();
            using (DrawingContext _DrawingContext = _DrawingVisual.RenderOpen())
            {
                _DrawingContext.DrawRectangle(new VisualBrush(DrawingCanvas), null, new Rect(new System.Windows.Point(), new System.Windows.Size(LoadedImage.Width, LoadedImage.Height)));
            }
            _RenderTargetBitmap.Render(_DrawingVisual);

            if (Oversize == true)
            {
                SetBackBitmapImage(ConvertToBitmapImage(LoadedImage));
            }
            else
            {
                SetBackBitmapImage(ConvertToBitmapImage(MaxNativeSizeImage));
            }

            BitmapFrame.Create(_RenderTargetBitmap);
            BitmapEncoder _PNGEncoder = new PngBitmapEncoder();
            _PNGEncoder.Frames.Add(BitmapFrame.Create(_RenderTargetBitmap));

            MemoryStream _MemoryStream = new MemoryStream();
            _PNGEncoder.Save(_MemoryStream);
                
            Bitmap _Source2 = new Bitmap(_MemoryStream);
            Bitmap _Source1 = LoadedImage;
                
            Bitmap _TargetBitmap = new Bitmap(_Source1.Width, _Source1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics _Graphics = Graphics.FromImage(_TargetBitmap);
            _Graphics.CompositingMode = CompositingMode.SourceOver;

            _Source1.SetResolution(_Source2.HorizontalResolution, _Source2.VerticalResolution);
            _Graphics.DrawImage(_Source1, 0, 0);
            _Graphics.DrawImage(_Source2, 0, 0);

            if (type == 1)
            {
                string _Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PicViewer\\2.png";

                try
                {
                    _TargetBitmap.Save(_Path, ImageFormat.Png);
                }
                catch
                { }
            }
            else
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Document";
                dlg.DefaultExt = ".png";
                dlg.Filter = "(.PNG)|*.png";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    try
                    {
                        _TargetBitmap.Save(dlg.FileName, ImageFormat.Png);
                    }
                    catch
                    { }
                }
            }
        }


        /// <summary>
        /// Выводит на печать 2.png из AppData/Roaming/PicViewer
        /// </summary>
        private void Print()
        {
            SaveDrawingToPNG(1);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PicViewer\\2.png";
            Process _print = new Process();
            _print.StartInfo.FileName = path;
            _print.StartInfo.Verb = "Print";
            _print.Start();
        }

       
        /// <summary>
        /// Обрабатывает нажатия кнопок клавиатуры
        /// </summary>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    this.Hide();
                    SettingsWindow _SettingsWindow = new SettingsWindow();
                    System.Windows.Point _Mouse = NativeMethods.GetMousePosition();
                    _SettingsWindow.Show();
                    _SettingsWindow.Top = _Mouse.Y - (_SettingsWindow.Height / 2d);
                    _SettingsWindow.Left = _Mouse.X - (_SettingsWindow.Width / 2d);
                    break;

                case Key.S:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        SaveDrawingToPNG(0);
                    }
                    break;
                
                case Key.P:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    { 
                        Print();
                    }
                    break;

                case Key.F1:
                    HelpWindow _HelpWindow = new HelpWindow();
                    _Mouse = NativeMethods.GetMousePosition();
                    _HelpWindow.Top = _Mouse.Y - (_HelpWindow.Height / 2d);
                    _HelpWindow.Left = _Mouse.X - (_HelpWindow.Width / 2d);
                    _HelpWindow.Show();
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// Рисует линию на canvas при зажатии ПКМ
        /// </summary>
        private void DrawingCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Line line = new Line
                {
                    Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(PenSettings.R, PenSettings.G, PenSettings.B)),
                    StrokeThickness = PenSettings.PenThickness,
                    X1 = CurrentPoint.X,
                    Y1 = CurrentPoint.Y,
                    X2 = e.GetPosition(DrawingCanvas).X,
                    Y2 = e.GetPosition(DrawingCanvas).Y,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                CurrentPoint = e.GetPosition(DrawingCanvas);
                DrawingCanvas.Children.Add(line);
            }
        }


        /// <summary>
        /// Закрывает программу при двойном клике ЛКМ
        /// Очищает канву при двойном клике ПКМ
        /// </summary>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    FadeOutAnimation();
                    break;
                case MouseButton.Right:
                    DrawingCanvas.Children.Clear();
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Перемещает окно при зажатии ЛКМ
        /// </summary>
        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        /// <summary>
        /// Перемещает полосы прокрутки при приближении и отдалении
        /// </summary>
        private void ChangeScrollOffsetsToCoursor(double x, double y)
        {
            ScrollViewer.UpdateLayout();
            ScrollViewer.ScrollToHorizontalOffset((ScrollViewer.ExtentWidth - ScrollViewer.ViewportWidth) * (x / WidthDouble));
            ScrollViewer.ScrollToVerticalOffset((ScrollViewer.ExtentHeight - ScrollViewer.ViewportHeight) * (y / HeightDouble));
            ScrollViewer.UpdateLayout();
        }


        /// <summary>
        /// Обрабатывает прокрутку колеса, для изменения размеров окна
        /// </summary>
        private void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (WillScaleUpOversize(0.1) == true)
                {
                    if (Oversize == false)
                    {
                        Oversize = true;
                        SetBackBitmapImage(ConvertToBitmapImage(LoadedImage));
                    }

                    double _PosX = e.GetPosition(DrawingCanvas).X;
                    double _PosY = e.GetPosition(DrawingCanvas).Y;

                    CanvasZoomValue += 0.1;
                    ScaleTransformDrawingCanvas(e);

                    ChangeScrollOffsetsToCoursor(_PosX, _PosY);
                }
                else
                {
                    GridZoomValue += 0.1;
                    this.Top -= ((HeightDouble * GridZoomValue) - this.Height) / 2;
                    this.Left -= ((WidthDouble * GridZoomValue) - this.Width) / 2;
                    ScaleTransformGrid(e);
                }
            }
            else
            {
                if (WillScaleUpOversize(0.1) == true && CanvasZoomValue > 1)
                {
                    double _PosX = e.GetPosition(DrawingCanvas).X;
                    double _PosY = e.GetPosition(DrawingCanvas).Y;

                    CanvasZoomValue -= 0.1;
                    ScaleTransformDrawingCanvas(e);

                    ChangeScrollOffsetsToCoursor(_PosX, _PosY);
                }
                else
                {
                    if (WillScaleDownUndersize(0.1) == false)
                    {
                        GridZoomValue -= 0.1;
                        this.Top += (this.Height - (HeightDouble * GridZoomValue)) / 2;
                        this.Left += (this.Width - (WidthDouble * GridZoomValue)) / 2;
                        ScaleTransformGrid(e);
                    }
                }
            }
        }


        /// <summary>
        /// Переключет фон окна
        /// </summary>
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                if (IsBackgroundTransparent == true)
                {
                    this.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                    IsBackgroundTransparent = false;
                }
                else
                {
                    this.Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                    IsBackgroundTransparent = true;
                } 
            }
        }


        /// <summary>
        /// Получает позицию мыши при зажатой ПКМ для рисования
        /// </summary>
        private void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                CurrentPoint = e.GetPosition(DrawingCanvas);
            }
        }


        /// <summary>
        /// Изменяет размер DrawingCanvas
        /// </summary>
        private void ScaleTransformDrawingCanvas(MouseWheelEventArgs e)
        {
            ScaleTransform _scale = new ScaleTransform(CanvasZoomValue, CanvasZoomValue);
            
            DrawingCanvas.LayoutTransform = _scale;
            e.Handled = true;
        }


        /// <summary>
        /// Изменяет размер сетки окна
        /// </summary>
        private void ScaleTransformGrid(MouseWheelEventArgs e)
        {
            ScaleTransform _scale = new ScaleTransform(GridZoomValue, GridZoomValue);

            Grid.LayoutTransform = _scale;
            e.Handled = true;
        }
    }
}
