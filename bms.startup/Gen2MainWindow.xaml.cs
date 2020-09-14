using FirstFloor.ModernUI.Windows.Controls;
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
using System.Windows.Shapes;

namespace bms.startup
{
    /// <summary>
    /// Gen2MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Gen2MainWindow : ModernWindow
    {
        public Gen2MainWindow()
        {
            InitializeComponent();
            this.DataContext = new Gen2ViewModel(this);
        }
        public Gen2MainWindow(string[] itemlist)
        {
            InitializeComponent();
            this.DataContext = new Gen2ViewModel(this, itemlist);
        }

        protected override void OnClosed(EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
