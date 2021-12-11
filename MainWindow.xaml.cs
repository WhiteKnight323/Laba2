using System.Windows;
using WpfApp1;

namespace WPFExcelView
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(true);
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow(false);
        }
        private void OpenWindow(bool b)
        {
            GreetingsWindow greet = new GreetingsWindow();
            greet.Show();
            this.Close();
            if (b) greet.OpenExcelFile();
            else greet.DownloadFile();
        }
    }


}
