using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LexiTunic
{
    public class AddPanelVm : INotifyPropertyChanged
    {
        public uint Bitfield { get; set; }
        public ICommand AddCommand { get; set; }
        public string GlyphDesc { get; set; } = "";

        public Dictionary<uint, string> GlyphMap => m_glyphs;
        Dictionary<uint, string> m_glyphs = new Dictionary<uint, string>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddPanelVm()
        {
            AddCommand = new DelegateCommand(AddCurrentGlyph);
        }

        private void AddCurrentGlyph(object obj)
        {
            if (!m_glyphs.ContainsKey(Bitfield))
            {
                m_glyphs.Add(Bitfield, GlyphDesc);
            }
            GlyphDesc = "";
            Bitfield = 0;
            var cb = PropertyChanged;
            if (cb != null)
            {
                cb.Invoke(this, new PropertyChangedEventArgs(nameof(GlyphDesc)));
                cb.Invoke(this, new PropertyChangedEventArgs(nameof(Bitfield)));
            }
        }
    }
}
