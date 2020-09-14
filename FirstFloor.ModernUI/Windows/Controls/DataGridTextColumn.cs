using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FirstFloor.ModernUI.Windows.Controls
{
    /// <summary>
    /// A DataGrid text column using default Modern UI element styles.
    /// </summary>
    public class DataGridTextColumn
        : System.Windows.Controls.DataGridTextColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridTextColumn"/> class.
        /// </summary>
        public DataGridTextColumn()
        {
            
            this.ElementStyle = Application.Current.Resources["DataGridTextStyle"] as Style;
            this.EditingElementStyle = Application.Current.Resources["DataGridEditingTextStyle"] as Style;
        }

        public static readonly DependencyProperty IsHeaderCheckProperty = DependencyProperty.Register("IsHeaderCheck", typeof(bool), typeof(DataGridTextColumn), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None, IsHeaderCheckChanged));

        private static void IsHeaderCheckChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridTextColumn ctl = d as DataGridTextColumn;
            if (d != null)
            {
                ctl.IsHeaderCheck = (bool)e.NewValue;
            }
        }

        public bool IsHeaderCheck
        {
            get { return (bool)GetValue(IsHeaderCheckProperty); }
            set { SetValue(IsHeaderCheckProperty, value); }
        }

        //private bool isHeaderCheck = true;

        //public bool IsHeaderCheck
        //{
        //    get { return isHeaderCheck; }
        //    set { isHeaderCheck = value; RaisePropertyChanged("IsHeaderCheck"); }
        //}

        //public event PropertyChangedEventHandler PropertyChanged;

        //public void RaisePropertyChanged(string propertyName)
        //{
        //    if (this.PropertyChanged != null)
        //    {
        //        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}
    }
}
