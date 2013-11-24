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

namespace ArduinoBikeComputer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        IABCSampleSource _sampleSource = new ArduinoABCSampleSource();
        ABCSampleViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
        }

        public ABCSampleViewModel VM
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
                VM = new ABCSampleViewModel(_sampleSource);
                //_sampleSource.SampleArrived += _vm._source_SampleArrived;

            }
            catch (Exception ex)
            {
                Console.WriteLine("WTF: How? {0}", ex.Message);
            }
        }
    }
}
