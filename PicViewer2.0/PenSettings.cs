using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicViewer2._0
{
    static class PenSettings
    {
        public static byte R
        {
            get
            {
                return Properties.Settings.Default.PenRByte;
            }
            set
            {
                Properties.Settings.Default.PenRByte = value;
                Properties.Settings.Default.Save();
            }
        }

        public static byte G
        {
            get
            {
                return Properties.Settings.Default.PenGByte;
            }
            set
            {
                Properties.Settings.Default.PenGByte = value;
                Properties.Settings.Default.Save();
            }
        }

        public static byte B
        {
            get
            {
                return Properties.Settings.Default.PenBByte;
            }
            set
            {
                Properties.Settings.Default.PenBByte = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int PenThickness
        {
            get
            {
                return Properties.Settings.Default.PenThickness;
            }
            set
            {
                Properties.Settings.Default.PenThickness = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
