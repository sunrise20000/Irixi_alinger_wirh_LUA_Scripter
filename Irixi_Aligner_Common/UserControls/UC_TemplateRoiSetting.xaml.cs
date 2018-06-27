using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class UC_TemplateRoiSetting : UserControl,INotifyPropertyChanged
    {

        public UC_TemplateRoiSetting()
        {
            InitializeComponent();
        }
  
        private bool bFirstLoaded = true;
        private int _currentSelectRoiTemplate=0;
        public event PropertyChangedEventHandler PropertyChanged;
        public int CurrentSelectRoiTemplate
        {
            get { return _currentSelectRoiTemplate; }
            set {
                if (_currentSelectRoiTemplate != value)
                {
                    _currentSelectRoiTemplate = value;
                    PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("CurrentSelectRoiTemplate"));
                }
            }
        }
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
        private void Cb_Cameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as SystemService).UpdateRoiTemplate.Execute((sender as ComboBox).SelectedIndex);
            if (!bFirstLoaded)
                SetAttachCamWindow(Cb_Cameras.SelectedIndex, true);

        }
        private void Storyboard2RoiCompleted(object sender, EventArgs e)
        {
            ListBoxRoiTemplate.ItemsSource = (DataContext as SystemService).RoiCollection;
        }
        private void SetAttachCamWindow(int nCamID, bool bAttach = true)
        {
            if (bAttach)
                Vision.Vision.Instance.AttachCamWIndow(nCamID, "CameraViewCam", CamDebug.HalconWindow);
            else
                Vision.Vision.Instance.DetachCamWindow(nCamID, "CameraViewCam");
        }
        private async void LoadDelay()
        {
            await Task.Run(() => {
                if (bFirstLoaded)
                {
                    Task.Delay(3000).Wait();
                    bFirstLoaded = false;
                }
                Application.Current.Dispatcher.Invoke(()=>SetAttachCamWindow(Cb_Cameras.SelectedIndex,true));
            });
        }
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && !bFirstLoaded)
                SetAttachCamWindow(Cb_Cameras.SelectedIndex,true);
            else
            {
                SetAttachCamWindow(Cb_Cameras.SelectedIndex,false);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDelay();
        }
  
    }
}
