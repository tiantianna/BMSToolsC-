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
    /// UDSTestMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UDSTestMainWindow : ModernWindow
    {
        public UDSTestMainWindow()
        {
            InitializeComponent();
            this.DataContext = new UDSTestViewModel(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void didComboxBox_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}