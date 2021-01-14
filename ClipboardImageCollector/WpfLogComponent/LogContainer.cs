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

        public void ScrollToEnd()
        {
            if ( this.View.VerticalOffset < this.View.ScrollableHeight )
            {
                double d = this.View.ScrollableHeight - this.View.VerticalOffset;

                // 末尾までの差分が少ない場合はタイマー使わずBottomに直行させる。
                if ( d < 10 )
                {
                    this.View.ScrollToBottom();
                }
                // ある程度距離がある場合はタイマーで動かして疑似アニメーション。
                else
                {
                    timer.Start();
                }
            }
        }

        public void Clear()
        {
            this.Panel.Children.Clear();
        }
    }
}
