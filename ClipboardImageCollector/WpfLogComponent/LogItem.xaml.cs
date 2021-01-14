using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfLogComponent
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class LogItem : UserControl
    {
        public LogItem()
        {
            InitializeComponent();

            this.LabelTimestamp.Content = DateTime.Now.ToString();
        }

        public LogItem(LogType type, string message, string details)
        {
            InitializeComponent();

            this.LabelTimestamp.Content = DateTime.Now.ToString( "HH:mm:ss" );
            this.TextMessage.Text = message;
            this.TextDetails.Text = details ?? "";
            switch ( type )
            {
                case LogType.Info:
                    this.LabelLogType.Content = "[INFO]";
                    this.LabelLogType.Foreground = Brushes.CornflowerBlue;
                    break;

                case LogType.Warn:
                    // this.Background = Brushes.Orange;
                    this.LabelLogType.Content = "[WARN]";
                    this.LabelLogType.Foreground = Brushes.OrangeRed;
                    this.TextMessage.Foreground = Brushes.OrangeRed;
                    break;

                case LogType.Error:
                    this.Background = Brushes.Yellow;
                    this.LabelLogType.Content = "[ERROR]";
                    this.LabelLogType.Foreground = Brushes.Red;
                    this.TextMessage.Foreground = Brushes.Red;
                    break;

                default:
                    break;
            }
        }

    }
}
