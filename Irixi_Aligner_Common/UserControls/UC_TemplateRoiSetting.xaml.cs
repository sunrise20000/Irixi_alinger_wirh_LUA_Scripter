using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// UC_TemplateRoiSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UC_TemplateRoiSetting : UserControl
    {
        public UC_TemplateRoiSetting()
        {
            InitializeComponent();
        }
        private bool bFirstLoaded = true;
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Storyboard RoiSb = FindResource("RoiSb") as Storyboard;
            Storyboard TemplateSb = FindResource("TemplateSb") as Storyboard;
            CurrentSelectRoiTemplate = CurrentSelectRoiTemplate == 0 ? 1 : 0;
            if (CurrentSelectRoiTemplate == 0)
                TemplateSb.Begin();
            else
                RoiSb.Begin();
        }
        private void Storyboard2TemplateCompleted(object sender, EventArgs e)
        {
            ListBoxRoiTemplate.ItemsSource = (DataContext as SystemService).TemplateCollection;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as SystemService).UpdateRoiTemplate.Execute((sender as ComboBox).SelectedIndex);
        }
        private void Storyboard2RoiCompleted(object sender, EventArgs e)
        {
            ListBoxRoiTemplate.ItemsSource = (DataContext as SystemService).RoiCollection;
        }
        public int CurrentSelectRoiTemplate { get { return Convert.ToInt16(GetValue(CurrentSelectRoiTemplateProperty)); } set { SetValue(CurrentSelectRoiTemplateProperty, value); } }
        public DependencyProperty CurrentSelectRoiTemplateProperty = DependencyProperty.Register("CurrentSelectRoiTemplate", typeof(int), typeof(UC_TemplateRoiSetting));
        private void SetAttachCamWindow(bool bAttach = true)
        {
            if (bAttach)
                Vision.Vision.Instance.AttachCamWIndow(0, "CamDebug", CamDebug.HalconWindow);
            else
                Vision.Vision.Instance.DetachCamWindow(0, "CamDebug");
        }
        private async void LoadDelay()
        {
            await Task.Run(() => {
                if (bFirstLoaded)
                {
                    Task.Delay(1500).Wait();
                    bFirstLoaded = false;
                }
                SetAttachCamWindow(true);
            });
        }
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                LoadDelay();
            else
            {
                SetAttachCamWindow(false);
            }
        }
    }
}
