using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Encoder = System.Drawing.Imaging.Encoder;
using System.IO;
using System.Threading;
using WpfLogComponent;

namespace ClipboardImageCollectForm
{
    public partial class ToolForm : Form
    {
        private const int WM_CLIPBOARDUPDATE = 0x31D;

        private readonly LogContainer logger;

        public string SaveFolder
        {
            get
            {
                string exe = Application.ExecutablePath;
                string dir = Path.Combine( Directory.GetParent( exe ).FullName, "save" );

                return dir;
            }
        }

        private bool AutoClear { get; set; } = true;


        #region ctor/Load/Shown
        public ToolForm()
        {
            InitializeComponent();

            this.logger = new LogContainer();
        }
        private void ToolForm_Load(object sender, EventArgs e)
        {
            // WPF ElementHost
            this.wpfElementHost.Child = this.logger.View;


            // メニュー用のハンドラ（ローカルメソッド）
            void OnTopMostChanged(ToolStripMenuItem item)
            {
                item.Text      = this.TopMost ? "最前面に表示 ✅" : "最前面に表示";
                item.ForeColor = this.TopMost ? Color.Black : Color.Gray;
            }

            void OnAutoClearChanged(ToolStripMenuItem item)
            {
                item.Text      = this.AutoClear ? "ログを自動消去 ✅" : "ログを自動消去";
                item.ForeColor = this.AutoClear ? Color.Black : Color.Gray;
            }

            // メニュー部分を生成する
            var menu = new MenuStrip();
            this.Controls.Add( menu );
            #region TopMost メニュー
            {
                var item = new ToolStripMenuItem();
                item.Click += ( x, _ )=> {
                    this.TopMost = !this.TopMost;
                    OnTopMostChanged( item );
                };
                menu.Items.Add( item );

                // 初期設定
                OnTopMostChanged( item );
            }
            #endregion

            #region Clear log メニュー
            {
                var item = new ToolStripMenuItem();
                item.Click += (x, _) => {
                    this.AutoClear = !this.AutoClear;
                    OnAutoClearChanged( item );
                };
                menu.Items.Add( item );

                // 初期設定
                OnAutoClearChanged( item );
            }
            {
                var item = new ToolStripMenuItem();
                item.Text = "ログ消去";
                item.Click += (x, _) => {
                    this.logger.Clear();
                };
                menu.Items.Add( item );
            }
            #endregion

            #region 開く メニュー
            {
                var item = new ToolStripMenuItem();
                item.Text = "保存先を開く";
                item.Click += (x, _) => {
                    if ( Directory.Exists( this.SaveFolder ) )
                    {
                        System.Diagnostics.Process.Start( "EXPLORER.EXE", this.SaveFolder );
                    }
                    else
                    {
                        // 無いなら今作っちゃっても良い気がするけどね。
                        this.Log( LogType.Warn, "保存先フォルダがありません。" );
                    }
                };
                menu.Items.Add( item );
            }
            #endregion

#if DEBUG
            {
                void ThrowEx()
                {
                    throw new Exception( "例外発生の実験" );
                };

                var item = new ToolStripMenuItem();
                item.Text = "ERROR";
                item.ForeColor = Color.OrangeRed;
                item.Click += (x, _) => {
                    try
                    {
                        ThrowEx();
                    }
                    catch ( Exception ex )
                    {
                        this.Log( LogType.Error, ex.Message, ex.StackTrace );
                    }
                };
                menu.Items.Add( item );
            }
            {
                var item = new ToolStripMenuItem();
                item.Text = "TEST";
                item.ForeColor = Color.OrangeRed;
                item.Click += (x, _) => {
                    
                    this.Log( LogType.Warn, "hogehoge mogemoge piyopiyo:", @"hogehoge
mogemoge
piyopiyo");
                };
                menu.Items.Add( item );
            }
#endif
        }
        private void ToolForm_Shown(object sender, EventArgs e)
        {
            // 画面をロードした時に save フォルダを用意しておく。
#region saveフォルダの作成
            {
#warning デフォルトで相対 /save でいいとして、後で app.config で出力先を指定出来るようにしたいね。

                this.Log( LogType.Info, "folder:", $"- {this.SaveFolder}" );

                DirectoryInfo di = new DirectoryInfo( this.SaveFolder );
                if ( !di.Exists )
                {
                    di.Create();
                    this.Log( LogType.Info, "folder created." );
                }
            }
#endregion
        }
        #endregion



        // Window Proc.
        #region override WndProc
        protected override void WndProc(ref Message m)
        {
            // 参考実装
            if ( WM_CLIPBOARDUPDATE == m.Msg )
            {
                try
                {
                    this.OnClipboardChanged();
                }
                catch ( Exception ex )
                {
                    this.Log( LogType.Error, ex.Message, ex.StackTrace );
                    this.Activate();
                }
            }

            base.WndProc( ref m );
        }
#endregion

        #region OnClipboardChanged
        private void OnClipboardChanged()
        {
            if ( Clipboard.ContainsImage() )
            {
                // 画像が入っている場合、保存する。
                var img = this.GetClipboardImage();
                string path = this.CreateUniquePath();
                this.SaveImage( img, path );
            }
        }

        private Image GetClipboardImage()
        {
            var img = Clipboard.GetImage();
            if ( null != img ) return img;

            this.Log( LogType.Warn, "Clipboard.GetImage() null." );

            // 稀にContainsImageなのにGetImageでnullが返ってくるので、
            // 一瞬待ってから１回だけリトライする
            Thread.Sleep( 100 ); // 0.1秒で十分だよなぁ？
            img = Clipboard.GetImage(); // 取り直し
            if ( null != img ) return img;

            this.Log( LogType.Error, "Clipboard.GetImage() null." );

            // 流石にリトライしてなお取れないならもうエラーにする。
            throw new Exception( "ClipboardからImageが取得できませんでした。" );
        }

        private string CreateUniquePath(int retry = 2)
        {
            string exe = Application.ExecutablePath;
            string dir = Path.Combine( Directory.GetParent( exe ).FullName, "save" );


            // 秒までのタイムスタンプと、GUIDベース（UUIDv4相当）で生成したランダムIDでファイル名にしとく。
            string id = System.Guid.NewGuid().ToString( "N" ).Substring( 0, 6 );
            string filename = DateTime.Now.ToString( "yyyyMMdd_HHmmss_fff" ) + id + ".png";

            string path = Path.Combine( dir, filename );

            // 衝突しなければファイル名を返す。
            if ( !File.Exists( path ) )
            {
                return path;
            }
            // 万一衝突したらリトライする。（流石に連発は有り得ないはず）
            else
            {
                this.Log( LogType.Warn, "retry ( save filename collision )." );

                if ( retry <= 0 ) throw new Exception( "保存ファイル名が尽く衝突してるので無理無理カタツ無理。" );

                return this.CreateUniquePath( retry - 1 );
            }
        }

        private void SaveImage(Image img, string path)
        {
            // 直接img.saveするとGDI+汎用エラー（詳細不明）が発生するので、一旦Bitmapに落としてから保存する。
            var dst = new Bitmap( img.Size.Width, img.Size.Height );
            using ( var src = new Bitmap( img ) )
            {
                using ( var g = Graphics.FromImage( dst ) )
                {
                    g.Clear( Color.Black );
                    g.DrawImage( src, 0, 0 );
                    g.Save();
                }
            }

            dst.Save( path, ImageFormat.Png );
            this.Log( LogType.Info, "success clipboard image save:",
$@"- size : {img.Size}
- file : {Path.GetFileName(path)}" );
        }


        /// <seealso cref="https://docs.microsoft.com/ja-jp/dotnet/framework/winforms/advanced/how-to-set-jpeg-compression-level"/>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach ( ImageCodecInfo codec in codecs )
            {
                if ( codec.FormatID == format.Guid )
                {
                    return codec;
                }
            }
            return null;
        }
        #endregion


        // UI Log
        #region UILog制御
        private const int LOG_LIMMIT = 25;

        private void Log(LogType type, string message, string details = "")
        {
            // リミット制御
            this.LogAutoClear();

            // ログデータを追加。
            this.logger.Push( type, message, details );
            
            // スクロール表示
            this.logger.ScrollToEnd();
        }

        private void LogAutoClear()
        {
            if ( !this.AutoClear ) return;

            if ( LOG_LIMMIT <= this.logger.Count )
            {
                this.logger.Clear();
            }
        }
        #endregion

    }
}
