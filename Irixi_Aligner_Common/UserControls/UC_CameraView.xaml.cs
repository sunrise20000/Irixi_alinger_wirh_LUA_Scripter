﻿using GalaSoft.MvvmLight.Messaging;
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
        private Window_TemplateRoiSetting WindowTemplateRoiSetting = new Window_TemplateRoiSetting();
        List<HTuple> HwindowList = new List<HTuple>();
        private CancellationTokenSource cts = null;
        private Task task = null;
        private AutoResetEvent grabEvent = new AutoResetEvent(false);
        private object _lock = new object();
        private bool bFirstLoaded = true;
        public UC_CameraView()
        {
            InitializeComponent();
            Messenger.Default.Register<string>(this, "WindowSizeChanged", str => { lock (_lock) { grabEvent.Set(); } });
            Messenger.Default.Register<string>(this, "SetCamState", strState => {
                lock (_lock)
                {
                    switch (strState.ToLower())
                    {
                        case "snapcontinues":
                            StartContinusGrab();
                            break;
                        case "stopsnap":
                            if (cts != null)
                                cts.Cancel();
                            break;
                        case "snaponce":
                            Vision.Vision.Instance.GrabImage(0);
                            break;
                        default:
                            throw new Exception("Unknow cmd for camera!");
                    }
                }
            });
        }
        ~UC_CameraView()
        {
            WindowTemplateRoiSetting.SetCloseFlag(true);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowTemplateRoiSetting.DataContext =(DataContext as ViewModelLocator).Service;
            WindowTemplateRoiSetting.ShowDialog();
        }
        private void StartContinusGrab()
        {
            if (task == null || task.IsCompleted || task.IsCanceled)
            {
                cts = new CancellationTokenSource();
                task = new Task(() => ThreadFunc(), cts.Token);
                task.Start();
            }
        }
        private void ThreadFunc()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                lock (_lock)
                {
                    bool ret = grabEvent.WaitOne(50);
                    if (ret)
                        continue;
                    Vision.Vision.Instance.GrabImage(0);
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
                Vision.Vision.Instance.AttachCamWIndow(0, "ViewCam2", Cam2.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "ViewCam3", Cam3.HalconWindow);
                Vision.Vision.Instance.AttachCamWIndow(0, "ViewCam4", Cam4.HalconWindow);
            }
            else
            {
                Vision.Vision.Instance.DetachCamWindow(0, "ViewCam1");
                Vision.Vision.Instance.DetachCamWindow(0, "ViewCam2");
                Vision.Vision.Instance.DetachCamWindow(0, "ViewCam3");
                Vision.Vision.Instance.DetachCamWindow(0, "ViewCam4");
            }
        }
        private async void LoadDelay()
        {
            await Task.Run(() => {
                if (bFirstLoaded)
                {
                    Task.Delay(2000).Wait();
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
    }
}