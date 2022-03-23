using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LexiTunic
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:LexiTunic"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:LexiTunic;assembly=LexiTunic"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:TunicGlyph/>
    ///
    /// </summary>
    public class TunicGlyph : FrameworkElement
    {
        public uint Bitfield
        {
            get { return (uint)GetValue(BitfieldProperty); }
            set { SetValue(BitfieldProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bitfield.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BitfieldProperty =
            DependencyProperty.Register("Bitfield", typeof(uint), typeof(TunicGlyph), new PropertyMetadata((uint)0));


        Pen m_activePen = new Pen(Brushes.DarkGreen, 10);

        static TunicGlyph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TunicGlyph), new FrameworkPropertyMetadata(typeof(TunicGlyph)));
        }

        static double G_TOP = 0.1;
        static double G_TOP_MID = 0.25;
        static double G_TOP_CENTER_MID = 0.4;
        static double G_MIDLINE = 0.5;
        static double G_BELOW_MIDLINE = 0.55;
        static double G_BOT_CENTER_MID = 0.6;
        static double G_BOT_MID = 0.75;
        static double G_BOT = 0.9;
        static double G_BOT_CIRCLE = 0.95;
        
        static double G_LEFT = 0.15;
        static double G_CENTER = 0.5;
        static double G_RIGHT = 0.85;

        (Point, Point)[] Segments = new[]
        {
            (new Point(G_RIGHT, G_TOP_MID), new Point(G_CENTER, G_TOP)),
            (new Point(G_CENTER, G_TOP), new Point(G_LEFT, G_TOP_MID)),
            (new Point(G_LEFT, G_TOP_MID), new Point(G_LEFT, G_MIDLINE)),
            (new Point(G_LEFT, G_BELOW_MIDLINE), new Point(G_LEFT, G_BOT_MID)),
            (new Point(G_LEFT, G_BOT_MID), new Point(G_CENTER, G_BOT)),
            (new Point(G_CENTER, G_BOT), new Point(G_RIGHT, G_BOT_MID)),
            (new Point(G_CENTER, G_TOP_CENTER_MID), new Point(G_RIGHT, G_TOP_MID)),
            (new Point(G_CENTER, G_TOP_CENTER_MID), new Point(G_CENTER, G_TOP)),
            (new Point(G_CENTER, G_TOP_CENTER_MID), new Point(G_LEFT, G_TOP_MID)),
            (new Point(G_CENTER, G_TOP_CENTER_MID), new Point(G_CENTER, G_MIDLINE)),
            (new Point(G_CENTER, G_BOT_CENTER_MID), new Point(G_LEFT, G_BOT_MID)),
            (new Point(G_CENTER, G_BOT_CENTER_MID), new Point(G_CENTER, G_BOT)),
            (new Point(G_CENTER, G_BOT_CENTER_MID), new Point(G_RIGHT, G_BOT_MID))
        };


        protected void DrawSegment(DrawingContext dc, int seg)
        {
            if (seg < Segments.Count())
            {
                var pts = Segments[seg];
                var start = new Point(pts.Item1.X * m_scale, pts.Item1.Y * m_scale);
                var end = new Point(pts.Item2.X * m_scale, pts.Item2.Y * m_scale);
                dc.DrawLine(m_activePen, start, end);
            }
            else if(seg == 13)
            {
                dc.DrawEllipse(null, m_activePen, new Point(G_CENTER * m_scale, G_BOT_CIRCLE * m_scale), 10, 10);
            }
        }

        protected void DrawMidline(DrawingContext dc)
        {
            var start = new Point(0 * m_scale, G_MIDLINE * m_scale);
            var end = new Point(1 * m_scale, G_MIDLINE * m_scale);
            dc.DrawLine(m_activePen, start, end);
        }

        protected double m_scale;

        protected override void OnRender(DrawingContext dc)
        {
            m_scale = Math.Min(ActualWidth, ActualHeight);

            DrawMidline(dc);
            for(int i = 0; i < 14; i++)
            {
                DrawSegment(dc, i);
            }
        }
    }
}
