using System;
using Eto.Drawing;

namespace ANTLRStudio.Editor.Utils
{
    [Serializable]
    public class ServiceColors
    {
        public Color CollapseMarkerForeColor { get; set; }
        public Color CollapseMarkerBackColor { get; set; }
        public Color CollapseMarkerBorderColor { get; set; }
        public Color ExpandMarkerForeColor { get; set; }
        public Color ExpandMarkerBackColor { get; set; }
        public Color ExpandMarkerBorderColor { get; set; }

        public ServiceColors()
        {
            CollapseMarkerForeColor = Colors.Silver;
            CollapseMarkerBackColor = Colors.White;
            CollapseMarkerBorderColor = Colors.Silver;
            ExpandMarkerForeColor = Colors.Red;
            ExpandMarkerBackColor = Colors.White;
            ExpandMarkerBorderColor = Colors.Silver;
        }
    }
}
