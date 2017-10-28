using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
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

        static int ScreenWidth = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
        static int ScreenHeight = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);

        const int SmoothResizeSteps = 4; 

        bool Oversize = false;
        bool Undersize = false;

        bool Resizing = false;
        
        double WidthDouble, HeightDouble;
        Bitmap LoadedImage, MaxNativeSizeImage;
        
        bool IsCanvasShown = false;
        System.Windows.Point CurrentPoint;


        /// <summary>
        /// Реализует анимацию изменения размеров окна
        /// </summary>
        private void SmoothResize(double startX, double startY, double targetX, double targetY,
                                  double startWidth, double startHeight, double targetWidth, double targetHeight)
        {
            if (Resizing == false)
            {
                Resizing = true;

                double _StepX = (startX - targetX) / SmoothResizeSteps,
                       _StepY = (startY - targetY) / SmoothResizeSteps,
                       _StepWidth = (targetWidth - startWidth) / SmoothResizeSteps,
                       _StepHeight = (targetHeight - startHeight) / SmoothResizeSteps;

                double[] _X = new double[SmoothResizeSteps],
                         _Y = new double[SmoothResizeSteps],
                         _Width = new double[SmoothResizeSteps],
                         _Height = new double[SmoothResizeSteps];

                _X[0] = startX - _StepX;
                _Y[0] = startY - _StepY;
                _Width[0] = startWidth + _StepWidth;
                _Height[0] = startHeight + _StepHeight;

                for (int i = 1; i < SmoothResizeSteps; i++)
                {
                    _X[i] = _X[i - 1] - _StepX;
                    _Y[i] = _Y[i - 1] - _StepY;
                    _Width[i] = _Width[i - 1] + _StepWidth;
                    _Height[i] = _Height[i - 1] + _StepHeight;
                }

                SetWindowSizeAndPosition(_Height, _Width, _X, _Y);

                Resizing = false;
            }
        }


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
        /// Загружает изображение из файла в LoadedImage(Bitmap)
        /// </summary>
        private void LoadImageFromFile(string path)
        {
            LoadedImage = new Bitmap(path);
        }


        /// <summary>
        /// Устанавливает фоном окна изображение(тип BitmapImage)
        /// </summary>
        private void SetBackBitmapImage(BitmapImage image)
        {
            this.Background = new ImageBrush(image);
        }


        /// <summary>
        /// Перемещает окно на заданную позицию
        /// </summary>
        private void SetWindowPosition(double x, double y)
        {
            this.Top = y;
            this.Left = x;
        }
        

        /// <summary>
        /// Изменяет размер окна на заданный
        /// </summary>
        private void SetWindowSize(double height, double width)
        {
            this.Height = height;
            this.Width = width;
        }
     

        /// <summary>
        /// Плавно меняет параметры окна проходя по массивам значений
        /// </summary>
        private void SetWindowSizeAndPosition(double[] height, double[] width, double[] x, double[] y)
        {
            for (int i = 0; i < SmoothResizeSteps; i++)
            {
                this.Top = y[i];
                this.Height = height[i];
                this.Left = x[i];
                this.Width = width[i];
            }
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
        /// Запускает анимацию исчезновения окна
        /// </summary>
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FadeOutAnimation();
        }


        /// <summary>
        /// Закрывает приложение после окончания анимации исчезновения
        /// </summary>
        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            this.Hide();
            Environment.Exit(0);
        }


        /// <summary>
        /// Перемещает главное окно при зажатии на нём ЛКМ
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsCanvasShown == false)
            {
                this.DragMove();
            }
        }


        //ГЛАВНОЕ ОКНО
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


        //ГЛАВНОЕ ОКНО после инициализации
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
            SetWindowPosition(_Mouse.X - (WidthDouble / 2d), _Mouse.Y - (HeightDouble / 2d));
            SetWindowSize(HeightDouble, WidthDouble);
            SetBackBitmapImage(ConvertToBitmapImage(MaxNativeSizeImage));
        }


        //ГЛАВНОЕ ОКНО после загрузки
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FadeInAnimation();
        }


        /// <summary>
        /// Расчитывает размер окна при увеличении
        /// </summary>
        private void ScaleUpWindowCalculation(double width, double height)
        {
            //Оверсайз по ширине
            if (((WidthDouble * ScaleMultiplierUp) > ScreenWidth) || Oversize == true)
            {
                HeightDouble += (HeightDouble / WidthDouble) * (ScreenWidth - WidthDouble);
                WidthDouble = ScreenWidth;

                Oversize = true;
                return;
            }
            else
            {
                WidthDouble = width * ScaleMultiplierUp;
                HeightDouble = height * ScaleMultiplierUp;
            }

            //Оверсайз по высоте
            if (((HeightDouble * ScaleMultiplierUp) > ScreenHeight) || Oversize == true)
            {
                WidthDouble += (WidthDouble / HeightDouble) * (ScreenHeight - HeightDouble);
                HeightDouble = ScreenHeight;

                Oversize = true;
                return;
            }
            else
            {
                WidthDouble = width * ScaleMultiplierUp;
                HeightDouble = height * ScaleMultiplierUp;
            }
        }
        /// <summary>
        /// Изменяет размер окна при уменьшении
        /// </summary>
        private void ScaleUpWindow()
        {
            if (Oversize == false && IsCanvasShown == false)
            {
                ScaleUpWindowCalculation(WidthDouble, HeightDouble);

                SmoothResize(this.Left, this.Top, this.Left - ((WidthDouble - this.Width) / 2d), this.Top - ((HeightDouble - this.Height) / 2d),
                             this.Width, this.Height, WidthDouble, HeightDouble);

                if (Undersize == true)
                {
                    Undersize = false;
                }
            }
        }


        /// <summary>
        /// Расчитывает размер окна при уменьшении
        /// </summary>
        private void ScaleDownWindowCalculation(double width, double height)
        {
            if (((WidthDouble * ScaleMultiplierDown) < MinimumSize) || Undersize == true)
            {
                Undersize = true;
                return;
            }
            else
            {
                WidthDouble = width * ScaleMultiplierDown;
                HeightDouble = height * ScaleMultiplierDown;
            }

            if (((HeightDouble * ScaleMultiplierDown) < MinimumSize) || Undersize == true)
            {
                Undersize = true;
                return;
            }
            else
            {
                WidthDouble = width * ScaleMultiplierDown;
                HeightDouble = height * ScaleMultiplierDown;
            }
        }
        /// <summary>
        /// Изменяет размер окна при уменьшении
        /// </summary>
        private void ScaleDownWindow()
        {
            if (Undersize == false && IsCanvasShown == false)
            {
                ScaleDownWindowCalculation(WidthDouble, HeightDouble);

                SmoothResize(this.Left, this.Top, this.Left + ((this.Width - WidthDouble) / 2d), this.Top + ((this.Height - HeightDouble) / 2d),
                             this.Width, this.Height, WidthDouble, HeightDouble);

                if (Oversize == true)
                {
                    Oversize = false;
                }
            }
        }


        //Обработка прокрутки колеса
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ScaleUpWindow();
            }
            else
            {
                ScaleDownWindow();
            }
        }


        //Обработка нажатия клавиш
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    ScaleUpWindow();
                    break;

                case Key.Down:
                    ScaleDownWindow();
                    break;

                case Key.S:
                    this.Hide();
                    SettingsWindow _SettingsWindow = new SettingsWindow();
                    System.Windows.Point _Mouse = NativeMethods.GetMousePosition();
                    _SettingsWindow.Show();
                    _SettingsWindow.Top = _Mouse.Y - (_SettingsWindow.Height / 2d);
                    _SettingsWindow.Left = _Mouse.X - (_SettingsWindow.Width / 2d);
                    break;

                case Key.C:
                    ShowCanvas();
                    break;

                case Key.V:
                    HideCanvas();
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// Рисует линию на canvas
        /// </summary>
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line
                {
                    Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0)),
                    StrokeThickness = 6,
                    X1 = CurrentPoint.X,
                    Y1 = CurrentPoint.Y,
                    X2 = e.GetPosition(this).X,
                    Y2 = e.GetPosition(this).Y,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                CurrentPoint = e.GetPosition(this);
                DrawingCanvas.Children.Add(line);
            }
        }


        /// <summary>
        /// Получает координаты курсора при нажатии ЛКМ по canvas
        /// </summary>
        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                CurrentPoint = e.GetPosition(this);
            }
        }


        /// <summary>
        /// Устанавливает фон Canvas, удаляет фон формы и разворачивает Canvas
        /// </summary>
        private void ShowCanvas()
        {
            if (IsCanvasShown == false)
            { 
                DrawingCanvas.Background = this.Background;
                this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                DrawingCanvas.Height = this.Height;
                DrawingCanvas.Width = this.Width;

                IsCanvasShown = true;
            }
        }


        /// <summary>
        /// Скрывает и очищает Canvas, возвращает фон формы
        /// </summary>
        private void HideCanvas()
        {
            if (IsCanvasShown == true)
            {
                SetBackBitmapImage(ConvertToBitmapImage(MaxNativeSizeImage));
                DrawingCanvas.Children.Clear();
                DrawingCanvas.Height = 0;
                DrawingCanvas.Width = 0;

                IsCanvasShown = false;
            }
        }
    }
}
