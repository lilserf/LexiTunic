using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LexiTunic
{
    public class AddPanelVm : DependencyObject, INotifyPropertyChanged
    {
        public uint Bitfield
        {
            get { return (uint)GetValue(BitfieldProperty); }
            set { SetValue(BitfieldProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bitfield.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BitfieldProperty =
            DependencyProperty.Register("Bitfield", typeof(uint), typeof(AddPanelVm), new PropertyMetadata((uint)0, OnBitfieldChanged));

        private static void OnBitfieldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is AddPanelVm vm)
            {
                vm.BitfieldChanged();
            }
        }

        private uint VOWELS = 0b111111;
        private uint CONSONANTS = 0b1111111000000;
        private uint CIRCLE = 1 << 13;

        private void BitfieldChanged()
        {
            var vowelMask = Bitfield & VOWELS;
            var consMask = Bitfield & CONSONANTS;
            var reversed = (Bitfield & CIRCLE) > 0;

            List<string> parts = new List<string>();

            string vowel = null;
            string cons = null;

            if(GlyphMap.ContainsKey(consMask))
            {
                cons = GlyphMap[consMask];
            }

            if(GlyphMap.ContainsKey(vowelMask))
            {
                vowel = GlyphMap[vowelMask];
            }

            if (vowel != null)
            {
                VowelPart = vowel;
            }
            else if(vowelMask == 0)
            {
                VowelPart = "";
            }
            else
            {
                VowelPart = "??";
            }
            OnPropertyChanged(nameof(VowelPart));

            if (cons != null)
            {
                ConsonantPart = cons;
            }
            else if(consMask == 0)
            {
                ConsonantPart = "";
            }
            else
            {
                ConsonantPart = "??";
            }
            OnPropertyChanged(nameof(ConsonantPart));

            IsReversed = reversed;
            OnPropertyChanged(nameof(IsReversed));
        }

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool IsReversed { get; set; }
        public string VowelPart { get; set; }
        public string ConsonantPart { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public string GlyphDesc { get; set; } = "";

        public Dictionary<uint, string> GlyphMap => m_glyphs;
        Dictionary<uint, string> m_glyphs = new Dictionary<uint, string>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddPanelVm()
        {
            AddCommand = new DelegateCommand(AddCurrentGlyph);
            ClearCommand = new DelegateCommand(ClearGlyph);
        }

        private void ClearGlyph(object obj)
        {
            GlyphDesc = "";
            Bitfield = 0;
            var cb = PropertyChanged;
            if (cb != null)
            {
                cb.Invoke(this, new PropertyChangedEventArgs(nameof(GlyphDesc)));
                cb.Invoke(this, new PropertyChangedEventArgs(nameof(Bitfield)));
            }
        }

        private void AddCurrentGlyph(object obj)
        {
            if (!m_glyphs.ContainsKey(Bitfield))
            {
                m_glyphs.Add(Bitfield, GlyphDesc);
            }
            ClearGlyph(obj);
        }
    }
}
