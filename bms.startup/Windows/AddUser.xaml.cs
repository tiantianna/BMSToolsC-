using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace bms.startup.Windows
{
    /// <summary>
    /// AddUser.xaml 的交互逻辑
    /// </summary>
    public partial class AddUser : ModernWindow
    {
        //power表示登录者权限，0超级用户，1管理员，2临时用户，3普通用户
        public AddUser(int power)
        {
            InitializeComponent();
            this.DataContext = new AddUserViewModel(power);
        }
      
        private void ComboBox_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("点击");
        }
    }
}
