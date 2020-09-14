using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;




namespace bms.startup
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow(string[] itemlist, int userpower)
        {
            InitializeComponent();
            this.DataContext = new SlaveViewModel(this, itemlist, userpower);
        }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new SlaveViewModel(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
   
        }
    }
}
