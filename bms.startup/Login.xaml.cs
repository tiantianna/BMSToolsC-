﻿using FirstFloor.ModernUI.Windows.Controls;
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
    /// login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : ModernWindow
    {
        public Login()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
        }
    }
}
