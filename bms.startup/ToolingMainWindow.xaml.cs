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
    /// ToolingMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ToolingMainWindow : ModernWindow
    {
        public ToolingMainWindow()
        {
            InitializeComponent();
            this.DataContext = new ToolingViewModel(this);
        }
        protected override void OnClosed(EventArgs e)
        {
            System.Environment.Exit(0);
        }
        private void gdView_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}
