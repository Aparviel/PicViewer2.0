using System;
using System.Runtime.InteropServices;

namespace PicViewer2._0
{
    class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref System.Drawing.Point pt);

        /// <summary>
        /// Возвращает позицию курсора
        /// </summary>
        public static System.Windows.Point GetMousePosition()
        {
            System.Drawing.Point pt = new System.Drawing.Point();
            GetCursorPos(ref pt);
            return new System.Windows.Point(pt.X, pt.Y);
        }


        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        /// <summary>
        /// Уведомляет Explorer об изменении ассоциаций файлов
        /// </summary>
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
