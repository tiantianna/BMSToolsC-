using bms.startup.Model;
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

    public class BMUEventArgs : EventArgs
    {

        private List<BMUConfigModel> bmuconfigList;
        public List<BMUConfigModel> BmuconfigList
        {
            get { return bmuconfigList; }
            set { bmuconfigList = value; }
        }

    }

    /// <summary>
    /// AddBMUForm.xaml 的交互逻辑
    /// </summary>
    public partial class AddBMUForm : ModernWindow
    {

        private List<BMUConfigModel> bmuconfigList = new List<BMUConfigModel>();

        public event EventHandler<BMUEventArgs> AddBMUEvent;
        public AddBMUForm()
        {
            InitializeComponent();
        }

        private void createbmu_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            bool result = int.TryParse(this.bmu.Text, out count);
            if (result)
            {
                if (this.sp.Children.Count > 0) return;
                for (int i = 0; i < count; i++)
                {
                    StackPanel temp = new StackPanel();
                    temp.Orientation = Orientation.Horizontal;
                    temp.Margin = new Thickness(0, 5, 0, 0);

                    TextBlock bmuindex = new TextBlock();
                    bmuindex.Foreground = Brushes.Purple;
                    bmuindex.Text = "从机号：";
                    bmuindex.Width = 60;
                    bmuindex.VerticalAlignment = VerticalAlignment.Center;
                    bmuindex.HorizontalAlignment = HorizontalAlignment.Right;
                    temp.Children.Add(bmuindex);

                    TextBox bmu = new TextBox();
                    bmu.Name = "bmu" + (i + 1);
                    bmu.Width = 60;
                    bmu.Text = (i + 1).ToString();
                    temp.Children.Add(bmu);

                    this.sp.Children.Add(temp);
                }

            }
        }

        private void deletebmu_Click(object sender, RoutedEventArgs e)
        {
            if (this.sp.Children.Count > 0)
            {
                this.sp.Children.Clear();
            }
        }

        private void savebmu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bmuconfigList.Count > 0) bmuconfigList.Clear();
                //遍历sp数据
                if (this.sp.Children.Count > 0)
                {
                    foreach (StackPanel item in this.sp.Children)
                    {
                        BMUConfigModel model = new BMUConfigModel();
                        foreach (var elm in item.Children)
                        {
                            TextBox tb = elm as TextBox;
                            if (tb != null && tb.Name.Contains("bmu"))
                            {
                                model.Bmuindex = int.Parse(tb.Text);
                            }

                            if (tb != null && tb.Name.Contains("cellcounts"))
                            {
                                model.Cellcouts = int.Parse(tb.Text);
                            }
                            if (tb != null && tb.Name.Contains("tempcounts"))
                            {
                                model.Tempcounts = int.Parse(tb.Text);
                            }
                        }
                        bmuconfigList.Add(model);
                    }

                    if (bmuconfigList.Count > 0)
                    {
                        var duplicateValues = bmuconfigList.GroupBy(x => x.Bmuindex).Where(x => x.Count() > 1);
                        if (duplicateValues != null && duplicateValues.Count() > 0)
                        {
                            ModernDialog.ShowMessage("BMU数值重复！", "提示", MessageBoxButton.OK);
                            return;
                        }
                    }



                    //通知主界面创建相关单体和温感值
                    if (AddBMUEvent != null)
                    {
                        BMUEventArgs args = new BMUEventArgs();
                        args.BmuconfigList = bmuconfigList;
                        AddBMUEvent(null, args);
                    }
                    this.Close();
                }

            }
            catch
            {

            }
        }
    }
}
