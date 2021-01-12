using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfLogComponent
{
    public class LogContainer
    {
        public ScrollViewer View { get; private set; }
        private StackPanel Panel { get; }

        public int Count { get { return this.Panel.Children.Count; } }

        #region ctor
        public LogContainer()
        {
            this.View = new ScrollViewer();
            this.Panel = new StackPanel();

            this.View.Content = this.Panel;
            this.View.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }
        #endregion

        public void Push(LogType type, string message)
        {
            Panel.Children.Add( new LogItem( type, message ) );
        }

        public void Pop()
        {
            if ( 0 != this.Panel.Children.Count )
            {
                this.Panel.Children.RemoveAt( 0 );
            }
        }

        public void Clear()
        {
            this.Panel.Children.Clear();
        }
    }
}
