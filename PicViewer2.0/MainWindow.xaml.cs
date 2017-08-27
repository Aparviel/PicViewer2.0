using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;

namespace PicViewer2._0
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        uint MaxStartSize = Properties.Settings.Default.MaxStartSize;
        uint MinimumSize = Properties.Settings.Default.MinimumSize;
        double ScaleMultiplierUp = Properties.Settings.Default.ScaleMultiplierUp;
        double ScaleMultiplierDown = Properties.Settings.Default.ScaleMultiplierDown;

        int ScreenWidth = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth);
        int ScreenHeight = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight);
        bool Oversize = false;
        bool Undersize = false;

        double WidthDouble, HeightDouble;
        //int WidthInt, HeightInt;
        Bitmap LoadedImage, ScaledImage, MaxNativeSizeImage;


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref System.Drawing.Point pt);
        /// <summary>
        /// Возвращает позицию курсора
        /// </summary>
        private static System.Windows.Point GetMousePosition()
        {
            System.Drawing.Point pt = new System.Drawing.Point();
            GetCursorPos(ref pt);
            return new System.Windows.Point(pt.X, pt.Y);
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
            //P.S. С этим всем надо что-то делать
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
            this.MaxHeight = height;
            this.MaxWidth = width;
            this.Height = height;
            this.Width = width;
            this.MinHeight = height;
            this.MinWidth = width;  
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


        //Закрывает программу при нажатии ПКМ по главному окну
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            Environment.Exit(0);
        }


        //Перемещает главное окно при зажатии на нём ЛКМ
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        //ГЛАВНОЕ ОКНО до инициализации
        public MainWindow()
        {
            string[] args = Environment.GetCommandLineArgs();

            LoadImageFromFile(args[1]);

            //this.Background = new ImageBrush(new BitmapImage(new Uri(args[1])));
            //SetBackImage
            //LoadImageFromFile(args[0]);
            //LoadImageFromFile("C:/Users/Андрей/Pictures/2.png");
            //LoadImageFromFile("C:/Users/Андрей/Pictures/test.png");
            //LoadImageFromFile("C:/Users/Андрей/Pictures/1-3-0-2.png");
            //LoadImageFromFile("C:/Users/Андрей/Pictures/novec.png");
            //LoadImageFromFile("C:/Users/Андрей/Pictures/tracert 2.png");
            //LoadImageFromFile("C:/Users/Андрей/Pictures/DeskBack/12.jpeg");
            //LoadImageFromFile("C:/Users/Андрей/Desktop/_/Projects/1_Personal/Ruler Command Seal/промежуточные/ruler.png");
            
            InitializeComponent();
            this.Title = "PicViewer(v2.0.4.13) - " + Path.GetFileName(args[1]);
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

            //WidthInt = Convert.ToInt32(WidthDouble);
            //HeightInt = Convert.ToInt32(HeightDouble);

            //ScaledImage = new Bitmap(LoadedImage, WidthInt, HeightInt);

            System.Windows.Point _Mouse = GetMousePosition();
            SetWindowPosition(_Mouse.X - (WidthDouble / 2d), _Mouse.Y - (HeightDouble / 2d));

            SetWindowSize(HeightDouble, WidthDouble);
            SetBackBitmapImage(ConvertToBitmapImage(MaxNativeSizeImage));
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
            if (Oversize == false)
            {
                ScaleUpWindowCalculation(WidthDouble, HeightDouble);
                //WidthInt = Convert.ToInt32(WidthDouble);
                //HeightInt = Convert.ToInt32(HeightDouble);

                //SetBackBitmapImage(ConvertToBitmapImage(ScaledImage = new Bitmap(LoadedImage, WidthInt, HeightInt)));
                SetWindowPosition(this.Left - ((WidthDouble - this.Width) / 2d),
                                  this.Top - ((HeightDouble - this.Height) / 2d));
                SetWindowSize(HeightDouble, WidthDouble);

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
            if (Undersize == false)
            {
                ScaleDownWindowCalculation(WidthDouble, HeightDouble);
                //WidthInt = Convert.ToInt32(WidthDouble);
                //HeightInt = Convert.ToInt32(HeightDouble);
                double cachew = this.Width;
                double cacheh = this.Height;

                SetWindowSize(HeightDouble, WidthDouble);


                SetWindowPosition(this.Left + ((cachew - WidthDouble) / 2d),
                                  this.Top + ((cacheh - HeightDouble) / 2d));
                //SetBackBitmapImage(ConvertToBitmapImage(ScaledImage = new Bitmap(LoadedImage, WidthInt, HeightInt)));

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


        //Обработка нажатия стрелок ВВЕРХ и ВНИЗ
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ScaleUpWindow();
            }

            if (e.Key == Key.Down)
            {
                ScaleDownWindow();
            }
        }


    }
}
