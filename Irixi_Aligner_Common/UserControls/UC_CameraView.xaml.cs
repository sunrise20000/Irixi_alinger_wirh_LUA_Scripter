using GalaSoft.MvvmLight.Messaging;
using HalconDotNet;
using Irixi_Aligner_Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// UC_CameraView.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class UC_CameraView : UserControl
    {
        List<HTuple> HwindowList = new List<HTuple>();
        private CancellationTokenSource cts = null;
        private Task task = null;
        private AutoResetEvent grabEvent = new AutoResetEvent(false);
        private object _lock = new object();
        private bool bFirstLoaded = true;
        private Window_TemplateRoiSetting DlgTemplateRoiSetting = new Window_TemplateRoiSetting();
        public UC_CameraView()
        {
            InitializeComponent();
            Messenger.Default.Register<string>(this, "WindowSizeChanged", str => { lock (_lock) { grabEvent.Set(); } });
            Messenger.Default.Register<Tuple<string, int>>(this, "SetCamState", tuple => {
                lock (_lock)
                {
                    switch (tuple.Item1.ToLower())
                    {
                        case "snapcontinuous":
                            StartContinusGrab(tuple.Item2);
                            break;
                        case "stopsnap":
                            if (cts != null)
                                cts.Cancel();
                            break;
                        case "snaponce":
                            Vision.Vision.Instance.GrabImage(tuple.Item2);
                            break;
                        default:
                            throw new Exception("Unknow cmd for camera!");
                    }
                }
            });
        }
        ~UC_CameraView()
        {
            Messenger.Default.Unregister("WindowSizeChanged");
            Messenger.Default.Unregister("SetCamState");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DlgTemplateRoiSetting.ShowDlg();
        }
        private void StartContinusGrab(int nCamID)
        {
            if (task == null || task.IsCompleted || task.IsCanceled)
            {
                cts = new CancellationTokenSource();
                task = new Task(() => ThreadFunc(nCamID), cts.Token);
                task.Start();
            }
        }
        private void ThreadFunc(int nCamID)
        {
            while (!cts.Token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    bool ret = grabEvent.WaitOne(50);
                    if (ret)
                        continue;
                    Vision.Vision.Instance.GrabImage(nCamID);
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lock (_lock)
            {
                grabEvent.Set();
            }
        }
        private void SetAttachWindow(bool bAttach)
        {
            if (bAttach)
            {
                Vision.Vision.Instance.AttachCamWIndow(0, "ViewCam1", Cam1.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(1, "ViewCam2", Cam2.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(2, "ViewCam3", Cam3.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(3, "ViewCam4", Cam4.HalconWindow);

            }
            else
            {
                Vision.Vision.Instance.DetachCamWindow(0, "ViewCam1");
                Vision.Vision.Instance.DetachCamWindow(1, "ViewCam2");
                Vision.Vision.Instance.DetachCamWindow(2, "ViewCam3");
                Vision.Vision.Instance.DetachCamWindow(3, "ViewCam4");

            }
        }
        private async void LoadDelay()
        {
            await Task.Run(() => {
                if (bFirstLoaded)
                {
                    Task.Delay(5000).Wait();
                    bFirstLoaded = false;
                }
                SetAttachWindow(true);
            });
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                LoadDelay();
            else
            {
                SetAttachWindow(false);
            }
        }
        public void UC_CameraView_Closing()
        {
            DlgTemplateRoiSetting.CLoseDlg() ;
        }
    }
}
