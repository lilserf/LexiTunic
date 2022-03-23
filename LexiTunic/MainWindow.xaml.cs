using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    public class MainWindowVm
    {
        public AddPanelVm AddPanelVm { get; set; } = new AddPanelVm();

        public MainWindowVm()
        {
            using(StreamReader f = new StreamReader("glyphs.txt"))
            {
                while(!f.EndOfStream)
                {
                    string line = f.ReadLine();
                    var split = line.Split("|");
                    AddPanelVm.GlyphMap.Add(uint.Parse(split[0]), split[1]);
                }
            }
        }

        internal void OnExit()
        {
            using(StreamWriter f = new StreamWriter("glyphs.txt"))
            {
                foreach(var g in AddPanelVm.GlyphMap)
                {
                    f.WriteLine($"{g.Key}|{g.Value}");
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowVm m_vm = new MainWindowVm();
        public MainWindow()
        {
            DataContext = m_vm;
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            m_vm.OnExit();
        }
    }
}
