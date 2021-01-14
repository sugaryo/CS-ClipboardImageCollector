using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfLogComponent
{
    public class LogContainer
    {
        public ScrollViewer View { get; private set; }
        private StackPanel Panel { get; }

        public int Count { get { return this.Panel.Children.Count; } }

        private readonly DispatcherTimer timer;

        #region ctor
        public LogContainer()
        {
            // View-Panel の生成
            this.Panel = new StackPanel();
            this.View = new ScrollViewer();
            this.View.Content = this.Panel;
            this.View.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            // 疑似アニメーションスクロールの為のタイマー
            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds( 5 );
            timer.Tick += (s, args) =>
            {
                // スクロール移動
                this.View.ScrollToVerticalOffset( this.View.VerticalOffset + 10 );

                // スクロール限界に来たらタイマーストップ。
                if ( this.View.ScrollableHeight <= this.View.VerticalOffset )
                {
                    timer.Stop();
                }
            };
        }
        #endregion

        public void ScrollToEnd()
        {
            timer.Start();
        }

        #region Push
        public void Push(LogType type, string message)
        {
            Panel.Children.Add( new LogItem( type, message, "" ) );
        }
        public void Push(LogType type, string message, string details)
        {
            Panel.Children.Add( new LogItem( type, message, details ) );
        }
        #endregion

        #region Pop
        public void Pop()
        {
            if ( 0 != this.Panel.Children.Count )
            {
                this.Panel.Children.RemoveAt( 0 );
            }
        }
        #endregion

        #region Clear
        public void Clear()
        {
            this.Panel.Children.Clear();
        }
        #endregion
    }
}
