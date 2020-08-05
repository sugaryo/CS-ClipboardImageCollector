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

namespace ClipboardImageCollectForm
{
    public partial class ToolForm : Form
    {

        #region ctor / Load
        public ToolForm()
        {
            InitializeComponent();
        }

        private void ToolForm_Load(object sender, EventArgs e)
        {

        }
        #endregion


        // Window Proc.
        protected override void WndProc(ref Message m)
        {
            // 参考実装
            if ( 0x31D == m.Msg )
            {
                string message = $"hwnd{m.HWnd} - msg:{m.Msg}";
                Console.WriteLine( message );
                this.txtConsole.Text += message + "\r\n";

                // 参考実装：テキストデータだったらクリップボードに突っ込まれた内容を画面にも出す
                string text = Clipboard.GetText();
                if ( !string.IsNullOrEmpty( text ) )
                {
                    StringBuilder sb = new StringBuilder();
                    var lines = text.Split( new string[] { "\r\n" }, StringSplitOptions.None );

                    sb.AppendLine( $"# Data[{lines.Length}]" );
                    foreach ( var line in lines )
                    {
                        string x = "  - " + line;
                        sb.AppendLine( x );
                        Console.WriteLine( x );
                    }
                    string data = sb.ToString();
                    this.txtConsole.Text += data;
                }

                // set curret last.
                this.txtConsole.Select( this.txtConsole.Text.Length, 0 );
                this.txtConsole.ScrollToCaret();
            }

            base.WndProc( ref m );
        }
    }
}
