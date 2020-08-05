using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardImageCollectForm
{
    static class Program
    {
        [DllImport( "user32.dll", SetLastError = true )]
        private extern static bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport( "user32.dll", SetLastError = true )]
        private extern static bool RemoveClipboardFormatListener(IntPtr hwnd);



        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            using ( var form = new ToolForm() )
            {
                var hwnd = form.Handle;

                // AddはすぐやってもいいけどRemoveと対称性を持たせる意味でLoadに仕込んでおく。
                form.Load += (f, _) =>
                    {
                        bool add = AddClipboardFormatListener( hwnd );
                        Console.WriteLine( $"★AddClipboardFormatListener[{hwnd}] : ({add})" );
                    };
                // Disposeされる前にやらないと意味がないので、仕方なくこれで。
                form.FormClosing += (f, _) =>
                    {
                        bool remove = RemoveClipboardFormatListener( hwnd );
                        Console.WriteLine( $"★RemoveClipboardFormatListener[{hwnd}] : ({remove})" );
                    };
                // ついでなので集約例外でもRemoveするように仕込んでおく。
                Application.ThreadException += (x, _) =>
                    {
                        bool remove = RemoveClipboardFormatListener( hwnd );
                        Console.WriteLine( $"★RemoveClipboardFormatListener[{hwnd}]! : ({remove})" );
                    };


                Application.Run( form );
                Console.WriteLine( "exit." );
            }
        }
    }
}
