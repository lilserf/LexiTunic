using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    public class TunicGlyph : ContentControl
    {
        // Attached property
        public static readonly DependencyProperty SegmentProperty =
                    DependencyProperty.RegisterAttached("Segment", typeof(int), typeof(TunicGlyph), new PropertyMetadata(-1));

        public static int GetSegment(DependencyObject d)
        {
            return (int)d.GetValue(SegmentProperty);
        }

        public static void SetSegment(DependencyObject d, int value)
        {
            d.SetValue(SegmentProperty, value);
        }

        public uint Bitfield
        {
            get { return (uint)GetValue(BitfieldProperty); }
            set { SetValue(BitfieldProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bitfield.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BitfieldProperty =
            DependencyProperty.Register("Bitfield", typeof(uint), typeof(TunicGlyph), new PropertyMetadata((uint)0, OnBitfieldChangedStatic));

        private static void OnBitfieldChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TunicGlyph me)
            {
                me.OnBitfieldChanged();
            }
        }

        private void OnBitfieldChanged()
        {
            RedrawLines();
        }

        Brush m_activeBrush = Brushes.DarkGreen;
        Brush m_inactiveBrush = Brushes.LightGray;

        static TunicGlyph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TunicGlyph), new FrameworkPropertyMetadata(typeof(TunicGlyph)));
        }

        private Canvas m_canvas = new Canvas();

        Line[] m_lines = new Line[13];
        Line m_midline;
        Ellipse m_circle;

        static double THICKNESS = 20;

        public TunicGlyph()
        {
            for(int i=Segments.Count()-1; i >= 0; i--)
            {
                var line = new Line();
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;
                line.Stroke = m_activeBrush;
                line.StrokeThickness = THICKNESS;
                TunicGlyph.SetSegment(line, i);
                m_lines[i] = line;
            }
            // Overlap very specifically
            m_canvas.Children.Add(m_lines[8]);
            m_canvas.Children.Add(m_lines[6]);
            m_canvas.Children.Add(m_lines[7]);
            m_canvas.Children.Add(m_lines[9]);
            m_canvas.Children.Add(m_lines[12]);
            m_canvas.Children.Add(m_lines[10]);
            m_canvas.Children.Add(m_lines[11]);
            m_canvas.Children.Add(m_lines[3]);
            m_canvas.Children.Add(m_lines[2]);
            m_canvas.Children.Add(m_lines[1]);
            m_canvas.Children.Add(m_lines[0]);
            m_canvas.Children.Add(m_lines[4]);
            m_canvas.Children.Add(m_lines[5]);


            m_midline = new Line();
            m_midline.StrokeStartLineCap = PenLineCap.Round;
            m_midline.StrokeEndLineCap = PenLineCap.Round;
            m_midline.Stroke = m_activeBrush;
            m_midline.StrokeThickness = THICKNESS;
            m_canvas.Children.Add(m_midline);

            m_circle = new Ellipse();
            m_circle.Stroke = m_activeBrush;
            m_circle.StrokeThickness = 0.8 * THICKNESS;
            m_circle.Width = 2 * THICKNESS;
            m_circle.Height = 2 * THICKNESS;
            TunicGlyph.SetSegment(m_circle, 13);
            m_canvas.Children.Add(m_circle);

            this.Content = m_canvas;

            SizeChanged += TunicGlyph_SizeChanged;
            MouseUp += TunicGlyph_MouseUp;
            MouseDown += TunicGlyph_MouseDown;
            MouseMove += TunicGlyph_MouseMove;
            RedrawLines();
        }

        private void ApplyOpToSegment(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);

            HitTestResult result = VisualTreeHelper.HitTest(m_canvas, pt);

            if (result != null)
            {
                var segHit = result.VisualHit;

                int segment = GetSegment(segHit);
                if (m_mouseOp)
                    SetSegment(segment);
                else
                    ClearSegment(segment);
            }

        }

        private void TunicGlyph_MouseMove(object sender, MouseEventArgs e)
        {
            if (System.Windows.Input.Mouse.LeftButton != MouseButtonState.Pressed)
                m_mouseDown = false;

            if (m_mouseDown)
            {
                ApplyOpToSegment(sender, e);
            }

        }

        bool m_mouseDown = false;
        bool m_mouseOp = true;

        private void TunicGlyph_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);

            HitTestResult result = VisualTreeHelper.HitTest(m_canvas, pt);

            if (result != null)
            {
                var segHit = result.VisualHit;

                int segment = GetSegment(segHit);
                m_mouseDown = true;
                m_mouseOp = !IsSegmentActive(segment);
            }
        }

        private void TunicGlyph_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ApplyOpToSegment(sender, e);
            m_mouseDown = false;
        }

        private void RedrawLines()
        {
            double scale = Math.Min(ActualWidth, ActualHeight);

            for (int i=0; i < m_lines.Length; i++)
            {
                var line = m_lines[i];
                var pts = Segments[i];
                line.X1 = pts.Item1.X * scale;
                line.Y1 = pts.Item1.Y * scale;
                line.X2 = pts.Item2.X * scale;
                line.Y2 = pts.Item2.Y * scale;
                if(IsSegmentActive(i))
                {
                    line.Stroke = m_activeBrush;
                    Canvas.SetZIndex(line, i + 100);
                }
                else
                {
                    line.Stroke = m_inactiveBrush;
                    Canvas.SetZIndex(line, i);
                }
            }

            m_midline.X1 = 0 * scale;
            m_midline.Y1 = G_MIDLINE * scale;
            m_midline.X2 = 1 * scale;
            m_midline.Y2 = G_MIDLINE * scale;
            Canvas.SetZIndex(m_midline, 1000);

            if (IsSegmentActive(13))
            {
                m_circle.Stroke = m_activeBrush;
                Canvas.SetZIndex(m_circle, 113);
            }
            else
            {
                m_circle.Stroke = m_inactiveBrush;
                Canvas.SetZIndex(m_circle, 13);
            }
            
            Canvas.SetLeft(m_circle, G_CENTER * scale - THICKNESS);
            Canvas.SetTop(m_circle, G_BOT_CIRCLE * scale - THICKNESS);
        }

        private void TunicGlyph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawLines();
        }

        private bool IsSegmentActive(int seg)
        {
            if (seg < 0) return false;

            int mask = 1 << seg;
            return (Bitfield & mask) > 0;          
        }

        private void ToggleSegment(int seg)
        {
            if (seg < 0) return;
            int mask = 1 << seg;
            Bitfield ^= (uint)mask;
        }

        private void SetSegment(int seg)
        {
            if (seg < 0) return;
            int mask = 1 << seg;
            Bitfield |= (uint)mask;
        }

        private void ClearSegment(int seg)
        {
            if (seg < 0) return;
            int mask = 1 << seg;
            Bitfield &= ~(uint)mask;
        }

        static double G_TOP = 0.1;
        static double G_TOP_MID = 0.25;
        static double G_TOP_CENTER_MID = 0.4;
        static double G_MIDLINE = 0.5;
        static double G_BELOW_MIDLINE = 0.58;
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

    }
}
