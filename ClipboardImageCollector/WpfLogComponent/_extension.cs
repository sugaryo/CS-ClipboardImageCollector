using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfLogComponent
{
    public static class _extension
    {
        public static void information(this LogContainer container, string message)
        {
            container.Push( LogType.Info, message );
        }
        public static void warning(this LogContainer container, string message)
        {
            container.Push( LogType.Warn, message );
        }
        public static void error(this LogContainer container, string message)
        {
            container.Push( LogType.Error, message );
        }
    }
}
