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

namespace ClipboardImageCollectForm
{
    public partial class ToolForm : Form
    {
        private const int WM_CLIPBOARDUPDATE = 0x31D;


        #region ctor / Load
        public ToolForm()
        {
            InitializeComponent();
        }

        private void ToolForm_Load(object sender, EventArgs e)
        {
            // 画面をロードした時に save フォルダを用意しておく。
            string exe = Application.ExecutablePath;
            string dir = Path.Combine( Directory.GetParent( exe ).FullName, "save" );

            this.Log( "execute." );
            this.Log( "  - " + exe );

            this.Log( "save folder." );
            this.Log( "  - " + dir );

            DirectoryInfo di = new DirectoryInfo( dir );
            if ( !di.Exists )
            {
                di.Create();
                this.Log( "  - create." );
            }
            else
            {
                this.Log( "  - exists." );
            }
            this.Log( "load." );
        }
        #endregion


        // Window Proc.
        protected override void WndProc(ref Message m)
        {
            // 参考実装
            if ( WM_CLIPBOARDUPDATE == m.Msg )
            {
                try
                {
                    this.OnClipboardChanged();
                }
                catch ( Exception ex)
                {
                    this.Log( "[ERROR] " + ex.Message );
                    this.Log( ex.StackTrace );
                }
            }

            base.WndProc( ref m );
        }

        #region OnClipboardChanged
        private void OnClipboardChanged()
        {
            if ( Clipboard.ContainsImage() )
            {
                // 画像が入っている場合、保存する。
                this.Log( "WM_CLIPBOARDUPDATE : image." );

                var img = Clipboard.GetImage();

                string path = this.CreateUniquePath();
                this.SaveJpegImage( img, path );
            }
            else
            {
                // skip.
                this.Log( "WM_CLIPBOARDUPDATE" );
            }
        }

        private string CreateUniquePath(int retry = 2)
        {
            string exe = Application.ExecutablePath;
            string dir = Path.Combine( Directory.GetParent( exe ).FullName, "save" );


            // 秒までのタイムスタンプと、GUIDベース（UUIDv4相当）で生成したランダムIDでファイル名にしとく。
            string id = System.Guid.NewGuid().ToString( "N" ).Substring( 0, 6 );
            string filename = DateTime.Now.ToString( "yyyyMMdd_HHmmss_fff" ) + id + ".jpg";

            string path = Path.Combine( dir, filename );

            // 衝突しなければファイル名を返す。
            if ( !File.Exists( path ) )
            {
                this.Log( "save filename." );
                this.Log( $"  - {filename}" );
                return path;
            }
            // 万一衝突したらリトライする。（流石に連発は有り得ないはず）
            else
            {
                this.Log( "retry." );

                if ( retry <= 0 ) throw new Exception( "保存ファイル名が尽く衝突してるので無理無理カタツ無理。" );

                return this.CreateUniquePath( retry - 1 );
            }
        }

        private void SaveJpegImage(Image img, string path)
        {
            // jpeg エンコーダの取得
            var encoder = GetEncoder( ImageFormat.Jpeg );

            long quality = Jpeg.Default.Quality;
            var parameters = new EncoderParameters( 1 );
            parameters.Param[0] = new EncoderParameter( Encoder.Quality, quality );


            
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

            dst.Save( path, encoder, parameters );
            this.Log( "save image." );
            this.Log( $"  - size : {img.Size}" );
            this.Log( $"  - path : {path}" );
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

        private void Log(string msg)
        {
            string timestamp = $"[{DateTime.Now.ToString( "HH:mm:ss" )}] ";
            Console.WriteLine( timestamp + msg );
            this.txtConsole.Text += timestamp + msg + "\r\n";

            this.txtConsole.Select( this.txtConsole.Text.Length, 0 );
            this.txtConsole.ScrollToCaret();
        }
    }
}
