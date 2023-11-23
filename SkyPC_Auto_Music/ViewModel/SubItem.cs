using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkyPC_Auto_Music.ViewModel
{
    public class SubItem
    {
        public SubItem(string name, UserControl screen = null)
        {
            Name = name;
            Screen = screen;
        }
        public string Name { get; private set; }
        public UserControl Screen { get; private set; }
    }
}
