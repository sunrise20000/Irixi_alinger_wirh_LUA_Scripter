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
using System.Windows.Shapes;

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// Window_TemplateRoiSetting.xaml 的交互逻辑
    /// </summary>

    public partial class Window_TemplateRoiSetting : Window
    {
        private bool bClose = false;
        public Window_TemplateRoiSetting()
        {
            InitializeComponent();
        }
        public void ShowDlg()
        {
            ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = !bClose;
        }
        public void CLoseDlg()
        {
            bClose = true;
            Close();
        }
            
    }
}
