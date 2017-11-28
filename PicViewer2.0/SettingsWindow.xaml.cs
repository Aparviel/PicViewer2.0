using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace PicViewer2._0
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        /// <summary>
        /// Инициализирует окно настроек
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();

            this.ScaleUpSlider.Value = Properties.Settings.Default.ScaleMultiplierUp;
            this.ScaleUpLabel.Content = ScaleUpSlider.Value;

            this.ScaleDownSlider.Value = Properties.Settings.Default.ScaleMultiplierDown;
            this.ScaleDownLabel.Content = ScaleDownSlider.Value;
        }

        /// <summary>
        /// Реализует перемещение окна настроек
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Скрывает окно настроек и завершает программу
        /// </summary>
        private void ExitLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            Environment.Exit(0);
        }

        /// <summary>
        /// Сохраняет настройки программы
        /// </summary>
        private void SaveLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.ScaleMultiplierUp = Convert.ToDouble(ScaleUpLabel.Content);
            Properties.Settings.Default.ScaleMultiplierDown = Convert.ToDouble(ScaleDownLabel.Content);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Отображает текущее значение слайдера ScaleUpSlider
        /// </summary>
        private void ScaleUpSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ScaleUpLabel.Content = Math.Round(ScaleUpSlider.Value, 2);
        }

        /// <summary>
        /// Отображает текущее значение слайдера ScaleDownSlider
        /// </summary>
        private void ScaleDownSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ScaleDownLabel.Content = Math.Round(ScaleDownSlider.Value, 2);
        }

        /// <summary>
        /// Привязывает поддерживаемые расширения к приложению
        /// </summary>
        private void SetAsDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            SetFileAssociation(".jpg");
            SetFileAssociation(".jpeg");
            SetFileAssociation(".png");
            SetFileAssociation(".gif");
            SetFileAssociation(".tiff");
        }

        /// <summary>
        /// Редактирует реестр для привязки расширения к приложению
        /// </summary>
        public static void SetFileAssociation(string extension)
        {
            string _KeyName = "PicViewer2.0";
            string _OpenWith = Assembly.GetEntryAssembly().Location;
            string _FileDescription = extension.Replace(".", "").ToUpper();

            RegistryKey _BaseKey;
            RegistryKey _OpenMethod;
            RegistryKey _Shell;
            RegistryKey _CurrentUser;

            _BaseKey = Registry.ClassesRoot.CreateSubKey(extension);
            _BaseKey.SetValue("", _KeyName);

            _OpenMethod = Registry.ClassesRoot.CreateSubKey(_KeyName);
            _OpenMethod.SetValue("", "Файл " + '\u0022' + _FileDescription + '\u0022');
            _OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + _OpenWith + "\",0");
            _Shell = _OpenMethod.CreateSubKey("Shell");
            _Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + _OpenWith + "\"" + " \"%1\"");
            _Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + _OpenWith + "\"" + " \"%1\"");
            _BaseKey.Close();
            _OpenMethod.Close();
            _Shell.Close();

            _CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + extension, true);
            _CurrentUser.DeleteSubKey("UserChoice", false);
            _CurrentUser.Close();

            NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
