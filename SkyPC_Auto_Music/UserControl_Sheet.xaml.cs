using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkyPC_Auto_Music
{
    /// <summary>
    /// UserControl_Sheet.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Sheet : System.Windows.Controls.UserControl
    {
        public UserControl_Sheet()
        {
            InitializeComponent();
        }

        private void BrowsePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择乐谱";
            dialog.Filter = "文本文件(*.txt)|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath.Text = dialog.FileName;
            }
        }
    }
}
