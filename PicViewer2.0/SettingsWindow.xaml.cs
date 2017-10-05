using System;
using System.Windows;
using System.Windows.Input;

namespace PicViewer2._0
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            this.ScaleUpSlider.Value = Properties.Settings.Default.ScaleMultiplierUp;
            this.ScaleUpLabel.Content = ScaleUpSlider.Value;

            this.ScaleDownSlider.Value = Properties.Settings.Default.ScaleMultiplierDown;
            this.ScaleDownLabel.Content = ScaleDownSlider.Value;
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        private void ExitLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            Environment.Exit(0);
        }


        private void SaveLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.ScaleMultiplierUp = Convert.ToDouble(ScaleUpLabel.Content);
            Properties.Settings.Default.ScaleMultiplierDown = Convert.ToDouble(ScaleDownLabel.Content);
            Properties.Settings.Default.Save();
        }


        private void ScaleUpSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ScaleUpLabel.Content = Math.Round(ScaleUpSlider.Value, 2);
        }


        private void ScaleDownSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ScaleDownLabel.Content = Math.Round(ScaleDownSlider.Value, 2);
        }
    }
}
