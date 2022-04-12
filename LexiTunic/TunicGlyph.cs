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
        #region Segment attached prop
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Attached property for tracking what segment our Line and Ellipse children correspond to
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
        #endregion

        #region Bitfield Dep Prop
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Bitfield dependency property
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
        #endregion

        // Canvas we'll present
        private Canvas m_canvas = new Canvas();
        // Child objects for drawing the glyph
        private Line[] m_lines = new Line[13];
        private Line m_midline;
        private Ellipse m_circle;

        // Brushes to use
        Brush m_vowelBrush = Brushes.DarkGreen;
        Brush m_consonantBrush = Brushes.LawnGreen;
        Brush m_derivedVowelBrush = Brushes.DarkOliveGreen;
        Brush m_derivedConsonentBrush = Brushes.DarkSeaGreen;
        Brush m_inactiveBrush = Brushes.LightGray;

        // Tracking mouse state
        bool m_mouseDown = false;
        bool m_mouseOp = true;

        // Basic numbers for the glyph lines
        static double THICKNESS = 20;

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

        // Create the actual segments from those static positions
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

        static TunicGlyph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TunicGlyph), new FrameworkPropertyMetadata(typeof(TunicGlyph)));
        }

        public TunicGlyph()
        {
            for(int i=Segments.Count()-1; i >= 0; i--)
            {
                var line = new Line();
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;
                line.Stroke = GetDefaultStrokeForSegment(i);
                line.StrokeThickness = THICKNESS;
                TunicGlyph.SetSegment(line, i);
                m_canvas.Children.Add(line);
                m_lines[i] = line;
            }

            m_midline = new Line();
            m_midline.StrokeStartLineCap = PenLineCap.Round;
            m_midline.StrokeEndLineCap = PenLineCap.Round;
            m_midline.Stroke = m_derivedVowelBrush;
            m_midline.StrokeThickness = THICKNESS;
            m_canvas.Children.Add(m_midline);

            m_circle = new Ellipse();
            m_circle.Stroke = m_vowelBrush;
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

        // Do the current correct thing to whatever segment the mouse is on
        private void ApplyOpToSegment(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);

            HitTestResult result = VisualTreeHelper.HitTest(m_canvas, pt);

            if (result != null)
            {
                var segHit = result.VisualHit;

                int segment = GetSegment(segHit);
                if (IsInteractiveeSegment(segment))
                    if (m_mouseOp)
                        SetSegment(segment);
                    else
                        ClearSegment(segment);
                else
                    if (m_mouseOp)
                        SetDerivedSegment(segment);
                    else
                        ClearDerivedSegment(segment);


                UpdateDerivedSegments();
            }
        }

        private void UpdateDerivedSegments()
        {
            // Consonants
            if (IsSegmentActive(6) || IsSegmentActive(7) || IsSegmentActive(8))
                SetSegment(9);
            else
                ClearSegment(9);

            // Vowels
            if (IsSegmentActive(2))
                SetSegment(3);
            else
                ClearSegment(3);
        }

        private void SetDerivedSegment(int segment)
        {
            if (segment == 3)
            {
                SetSegment(2);
            }
            else if (segment == 9)
            {
                // No way to know which to do here, just assume middle
                SetSegment(7);
            }
        }

        private void ClearDerivedSegment(int segment)
        {
            if (segment == 3)
            {
                ClearSegment(2);
            }
            else if (segment == 9)
            {
                ClearSegment(6);
                ClearSegment(7);
                ClearSegment(8);
            }
        }

        private static bool IsDerivedSegment(int segment) => !IsInteractiveeSegment(segment);

        private static bool IsInteractiveeSegment(int segment)
        {
            return segment != 9 && segment != 3;
        }

        private Brush GetDefaultStrokeForSegment(int i)
        {
            bool isVowel = i < 6;
            bool isInteractive = IsInteractiveeSegment(i);
            return isVowel ? isInteractive ? m_vowelBrush : m_derivedVowelBrush : isInteractive ? m_consonantBrush : m_derivedConsonentBrush;
        }

        // Mouse is moving, check if it's still down and do what should be done
        private void TunicGlyph_MouseMove(object sender, MouseEventArgs e)
        {
            if (System.Windows.Input.Mouse.LeftButton != MouseButtonState.Pressed)
                m_mouseDown = false;

            if (m_mouseDown)
            {
                ApplyOpToSegment(sender, e);
            }

        }

        // Mouse went down, set the correct operation to do to other segments
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

        // Mouse went up, hit the current segment if there is one
        private void TunicGlyph_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ApplyOpToSegment(sender, e);
            m_mouseDown = false;
        }

        // Called all the time when stuff changes
        private void RedrawLines()
        {
            double scale = Math.Min(ActualWidth, ActualHeight);

            for (int i=0; i < m_lines.Length; i++)
            {
                // Rescale the positions of the lines given our current size
                var line = m_lines[i];
                var pts = Segments[i];
                line.X1 = pts.Item1.X * scale;
                line.Y1 = pts.Item1.Y * scale;
                line.X2 = pts.Item2.X * scale;
                line.Y2 = pts.Item2.Y * scale;

                // Make the vowels draw on top of the consonants by default
                int baseZ = i + (i < 6 ? 10 : 0);
                if (IsDerivedSegment(i))
                    baseZ -= 3;

                // Color the segments correctly and draw the active segments on top
                if(IsSegmentActive(i))
                {
                    line.Stroke = GetDefaultStrokeForSegment(i);
                    Canvas.SetZIndex(line, baseZ + 100);
                }
                else
                {
                    line.Stroke = m_inactiveBrush;
                    Canvas.SetZIndex(line, baseZ);
                }
            }

            // Rescale the grid line and draw it on top
            m_midline.X1 = 0 * scale;
            m_midline.Y1 = G_MIDLINE * scale;
            m_midline.X2 = 1 * scale;
            m_midline.Y2 = G_MIDLINE * scale;
            Canvas.SetZIndex(m_midline, 1000);

            // Scale and color the circle
            if (IsSegmentActive(13))
            {
                m_circle.Stroke = m_vowelBrush;
                Canvas.SetZIndex(m_circle, 113);
            }
            else
            {
                m_circle.Stroke = m_inactiveBrush;
                Canvas.SetZIndex(m_circle, 13);
            }
            
            // Position the circle
            Canvas.SetLeft(m_circle, G_CENTER * scale - THICKNESS);
            Canvas.SetTop(m_circle, G_BOT_CIRCLE * scale - THICKNESS);
        }

        // Control size changed, redraw
        private void TunicGlyph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawLines();
        }

        #region Segment bitfield math functions
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
        #endregion

    }
}
