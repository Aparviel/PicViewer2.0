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
            if(Resizing == false)
            {
                Resizing = true;
                SetWindowSizeAndPositionBySteps((targetHeight - startHeight) / SmoothResizeSteps,
                                     (targetWidth - startWidth) / SmoothResizeSteps,
                                     (targetX - startX) / SmoothResizeSteps,
                                     (targetY - startY) / SmoothResizeSteps,
                                     targetX, targetY, targetWidth, targetHeight);
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
        /// Загружает изображение из файла в LoadedImage
        /// </summary>
        private void LoadImageFromFile(string path)
        {
            LoadedImage = new Bitmap(path);
        }


        /// <summary>
        /// Устанавливает фоном окна изображение
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
        /// Плавно меняет параметры окна по шагам
        /// </summary>
        private void SetWindowSizeAndPositionBySteps(double heightStep, double widthStep, double xStep, double yStep,
                                                     double targetX, double targetY, double targetWidth, double targetHeight)
        {
            for (int i = 0; i < SmoothResizeSteps - 1; i++)
            {
                this.Top += yStep;
                this.Left += xStep;
                this.Height += heightStep;
                this.Width += widthStep;
            }

            this.Top = targetY;
            this.Left = targetX;
            this.Height = targetHeight;
            this.Width = targetWidth;
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
            SetWindowPosition(_Mouse.X - (WidthDouble / 2d), _Mouse.Y - (HeightDouble / 2d));
            SetWindowSize(HeightDouble, WidthDouble);
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
            //Андерсайз по ширине
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

            //Андерсайз по высоте
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


        /// <summary>
        /// Обрабатывает прокрутку колеса мыши
        /// </summary>
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


        /// <summary>
        /// Обрабатывает нажатия кнопок мыши
        /// </summary>
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
