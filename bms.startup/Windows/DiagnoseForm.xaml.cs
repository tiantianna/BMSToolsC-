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

namespace bms.startup.Windows
{
    /// <summary>
    /// DiagnoseInfo.xaml 的交互逻辑
    /// </summary>
    public partial class DiagnoseForm : ModernWindow
    {
        public DiagnoseForm(SlaveViewModel parent, int bmuindex)
        {
            InitializeComponent();
            this.DataContext = new DiagnoseViewModel(parent, bmuindex,this);
        }

    }
}
