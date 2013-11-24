using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ABCTempTableGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TempTableViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
        }

        public TempTableViewModel VM
        {
            set
            {
                _vm = value;
                this.DataContext = _vm;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                VM = new TempTableViewModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WTF: How? {0}", ex.Message);
            }
        }

    }
}
