using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Messaging;
using HalconDotNet;

namespace Irixi_Aligner_Common.Vision
{
    public enum Enum_REGION_OPERATOR { ADD, SUB }
    public enum Enum_REGION_TYPE { RECTANGLE, CIRCLE }
    public enum EnumCamSnapState
    {
        IDLE,
        BUSY,
        DISCONNECTED
    }

    public class Vision
    {
        #region constructor
        private Vision()
        {
            for (int i = 0; i < 10; i++)
            {
                HoImageList.Add(new HObject());
                AcqHandleList.Add(new HTuple());
                _lockList.Add(new object());
            }
            HOperatorSet.GenEmptyObj(out Region);
        }

    


        private static readonly Lazy<Vision> _instance = new Lazy<Vision>(() => new Vision());
        public static Vision Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region  var
        public List<object> _lockList = new List<object>();
        public enum IMAGEPROCESS_STEP
        {
            T1,
            T2,
            T3,
            T4
        }
        private List<HObject> HoImageList = new List<HObject>(10);    //Image
        private List<HTuple> AcqHandleList = new List<HTuple>(10);    //Aqu
        private Dictionary<int, Dictionary<string, HTuple>> HwindowDic = new Dictionary<int, Dictionary<string, HTuple>>();    //Hwindow
        private Dictionary<int, Dictionary<string, System.Windows.Controls.Image>> ImageWindowDic = new Dictionary<int, Dictionary<string, System.Windows.Controls.Image>>();   //ImageWindow in WPF
        private Dictionary<int, Tuple<HTuple, HTuple>> ActiveCamDic = new Dictionary<int, Tuple<HTuple, HTuple>>();
        private HObject Region = null;
        public Enum_REGION_OPERATOR RegionOperator = Enum_REGION_OPERATOR.ADD;
        public Enum_REGION_TYPE RegionType = Enum_REGION_TYPE.CIRCLE;
        private HObject ImageTemp = null;
        #endregion

        #region public method 
        public bool AttachCamWIndow(int nCamID, string Name, HTuple hWindow)
        {
            lock (_lockList[nCamID])
            {
                //关联当前窗口
                if (HwindowDic.Keys.Contains(nCamID))
                {
                    var its = from hd in HwindowDic[nCamID] where hd.Key == Name select hd;
                    if (its.Count() == 0)
                        HwindowDic[nCamID].Add(Name, hWindow);
                    else
                        HwindowDic[nCamID][Name] = hWindow;
                }
                else
                    HwindowDic.Add(nCamID, new Dictionary<string, HTuple>() { { Name, hWindow } });
                if (ActiveCamDic.Keys.Contains(nCamID))
                    HOperatorSet.SetPart(HwindowDic[nCamID][Name], 0, 0, ActiveCamDic[nCamID].Item2, ActiveCamDic[nCamID].Item1);

                //需要解除此窗口与其他相机的关联
                foreach (var kps in HwindowDic)
                {
                    if (kps.Key == nCamID)
                        continue;
                    foreach (var kp in kps.Value)
                    {
                        if (kp.Key == Name)
                        {
                            kps.Value.Remove(Name);
                            break;
                        }
                    }
                }
                return true;
            }

        }
        public bool AttachCamWIndow(int nCamID, string Name, System.Windows.Controls.Image imageWindow) //同一个相机最多只能与一个窗口绑定
        {
            lock (_lockList[nCamID])
            {
                //关联当前窗口
                if (ImageWindowDic.Keys.Contains(nCamID))
                {
                    var its = from hd in ImageWindowDic[nCamID] where hd.Key == Name select hd;
                    if (its.Count() == 0)
                        ImageWindowDic[nCamID].Add(Name, imageWindow);
                    else
                        ImageWindowDic[nCamID][Name] = imageWindow;
                }
                else
                    ImageWindowDic.Add(nCamID, new Dictionary<string, System.Windows.Controls.Image>() { { Name, imageWindow } });
               
                //需要解除此窗口与其他相机的关联
                foreach (var kps in ImageWindowDic)
                {
                    if (kps.Key == nCamID)
                        continue;
                    foreach (var kp in kps.Value)
                    {
                        if (kp.Key == Name)
                        {
                            kps.Value.Remove(Name);
                            break;
                        }
                    }
                }
                return true;
            }

        }
        public bool DetachCamWindow(int nCamID, string Name)
        {
            lock (_lockList[nCamID])
            {
                if (HwindowDic.Keys.Contains(nCamID))
                {
                    if (HwindowDic[nCamID].Keys.Contains(Name))
                        HwindowDic[nCamID].Remove(Name);
                }
                if(ImageWindowDic.Keys.Contains(nCamID))
                {
                    if (ImageWindowDic[nCamID].Keys.Contains(Name))
                        ImageWindowDic[nCamID].Remove(Name);
                }
                return true;
            }
        }
   


        public bool OpenCam(int nCamID)
        {
            HObject image = null;
            HTuple hv_AcqHandle = null;
            HTuple width = 0, height = 0;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!IsCamOpen(nCamID))
                    {
                        HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                                                    -1, "false", "default", "Integrated Camera", 0, -1, out hv_AcqHandle);
                        HOperatorSet.GrabImage(out image, hv_AcqHandle);
                        HOperatorSet.GetImageSize(image, out width, out height);
                        ActiveCamDic.Add(nCamID, new Tuple<HTuple, HTuple>(width, height));
                        AcqHandleList[nCamID] = hv_AcqHandle;
                    }
                    if (IsCamOpen(nCamID))
                    {
                        if (HwindowDic.Keys.Contains(nCamID))
                        {
                            foreach (var it in HwindowDic[nCamID])
                            {
                                HOperatorSet.SetPart(it.Value, 0, 0, ActiveCamDic[nCamID].Item2, ActiveCamDic[nCamID].Item1);
                                HOperatorSet.DispObj(image, it.Value);
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
                return false;
            }
            finally
            {
                if (image != null)
                    image.Dispose();
            }
        }
        public bool CloseCam(int nCamID)
        {
            try
            {
                lock (_lockList[nCamID])
                {
                    if (ActiveCamDic.Keys.Contains(nCamID))
                    {
                        HOperatorSet.CloseFramegrabber(AcqHandleList[nCamID]);
                        ActiveCamDic.Remove(nCamID);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
                return false;
            }
        }
        public bool IsCamOpen(int nCamID)
        {
            lock (_lockList[nCamID])
            {
                return ActiveCamDic.Keys.Contains(nCamID);
            }
        }
        public void GrabImage(int nCamID, bool bDispose = true)
        {
            HObject image = null;
            Bitmap bitmap = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    if (!HwindowDic.Keys.Contains(nCamID))
                    {
                        Messenger.Default.Send<string>(string.Format("请先给相机{0}绑定视觉窗口", nCamID), "ShowError");
                        return;
                    }
                    if (!IsCamOpen(nCamID))
                        OpenCam(nCamID);
                    if (!IsCamOpen(nCamID))
                    {
                        Messenger.Default.Send<string>(string.Format("打开相机{0}失败", nCamID), "ShowError");
                        return;
                    }
                    if (ImageTemp != null)
                    {
                        ImageTemp.Dispose();
                        ImageTemp = null;
                        
                    }
                    HOperatorSet.GrabImage(out image, AcqHandleList[nCamID]);
                    HOperatorSet.GenEmptyObj(out ImageTemp);
                    HOperatorSet.ConcatObj(ImageTemp, image, out ImageTemp);
                    foreach (var it in HwindowDic[nCamID])
                       if(it.Value!=-1)
                            HOperatorSet.DispObj(image, it.Value);

                    //显示图片
                    VisionDataHelper.GenertateRGBBitmap(image, out bitmap);
                    if (ImageWindowDic.Keys.Contains(nCamID))
                    {
                        foreach (var it in ImageWindowDic[nCamID])
                        {
                            it.Value.Source = VisionDataHelper.ChangeBitmapToImageSource(bitmap);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<string>(ex.Message, "ShowError");
            }
            finally
            {
                if (bDispose && image != null)
                {
                    image.Dispose();
                    image = null;
                }
                if (bDispose && bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
        }
        public bool ProcessImage(IMAGEPROCESS_STEP nStep, int nCamID, object para, out object result)
        {
            HObject image = null;
            try
            {
                lock (_lockList[nCamID])
                {
                    switch (nStep)
                    {
                        case IMAGEPROCESS_STEP.T1:

                            break;
                        case IMAGEPROCESS_STEP.T2:
                            break;
                        default:
                            break;
                    }
                    result = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                result = null;
                Messenger.Default.Send<string>(ex.Message, "Error");
                return false;
            }
            finally
            {
                image.Dispose();
            }
        }
        public bool DrawRoi(int nCamID)
        {
            try
            {
                lock (_lockList[nCamID])
                {
                    if (HwindowDic.Keys.Contains(nCamID) && HwindowDic[nCamID].Keys.Contains("CamDebug"))
                    {
                        HTuple window = HwindowDic[nCamID]["CamDebug"];
                        HTuple row, column, phi, length1, length2, radius;
                        HObject newRegion = null;
                        HOperatorSet.SetColor(window, "green");
                        switch (RegionType)
                        {
                            case Enum_REGION_TYPE.RECTANGLE:
                                HOperatorSet.DrawRectangle2(window, out row, out column, out phi, out length1, out length2);
                                HOperatorSet.GenRectangle2(out newRegion, row, column, phi, length1, length2);
                                break;
                            case Enum_REGION_TYPE.CIRCLE:
                                HOperatorSet.DrawCircle(window, out row, out column, out radius);
                                HOperatorSet.GenCircle(out newRegion, row, column, radius);
                                break;
                            default:
                                break;
                        }
                        if (RegionOperator == Enum_REGION_OPERATOR.ADD)
                        {
                            HOperatorSet.Union2(Region, newRegion, out Region);
                        }
                        else
                        {
                            HOperatorSet.Difference(Region, newRegion, out Region);
                        }

                        HOperatorSet.SetDraw(window, "fill");
                        HOperatorSet.SetColor(window, "red");
                        HOperatorSet.ClearWindow(window);
                        HOperatorSet.DispObj(ImageTemp, window);
                        HOperatorSet.DispObj(Region, window);
                        return true;
                    }
                    Messenger.Default.Send<String>("绘制模板窗口没有打开,或者该相机未关联绘制模板窗口", "ShowError");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Messenger.Default.Send<String>(string.Format("DrawRectangle出错:{0}", ex.Message), "ShowError");
                return false;
            }
        }
        #endregion
    }


    public class VisionDataHelper
    {
        [DllImport("kernel32")]
        public static extern int CopyMemory(int pSource, int pDes, Int32 nSize);
        public static List<string> GetRoiListForSpecCamera(int nCamID, List<string> fileListInDataDirection)
        {
            var list = new List<string>();
            foreach (var it in fileListInDataDirection)
            {
                if (it.Contains(string.Format("Cam{0}_", nCamID)))
                    list.Add(it);
            }
            return list;
        }
        public static List<string> GetTemplateListForSpecCamera(int nCamID, List<string> fileListInDataDirection)
        {
            var list = new List<string>();
            foreach (var it in fileListInDataDirection)
            {
                if (it.Contains(string.Format("Cam{0}_", nCamID)))
                    list.Add(it);
            }
            return list;
        }
        public static void GenertateGrayBitmap(HObject image, out Bitmap res)
        {
            HTuple hpoint, type, width, height;

            const int Alpha = 255;
            int[] ptr = new int[2];
            HOperatorSet.GetImagePointer1(image, out hpoint, out type, out width, out height);

            res = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = res.Palette;
            for (int i = 0; i <= 255; i++)
            {
                pal.Entries[i] = Color.FromArgb(Alpha, i, i, i);
            }
            res.Palette = pal;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = res.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            int PixelSize = Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            ptr[0] = bitmapData.Scan0.ToInt32();
            ptr[1] = hpoint.I;
            if (width % 4 == 0)
                CopyMemory(ptr[0], ptr[1], width * height * PixelSize);
            else
            {
                for (int i = 0; i < height - 1; i++)
                {
                    ptr[1] += width;
                    CopyMemory(ptr[0], ptr[1], width * PixelSize);
                    ptr[0] += bitmapData.Stride;
                }
            }
            res.UnlockBits(bitmapData);
        }
        unsafe public static void GenertateRGBBitmap(HObject image, out Bitmap res)
        {
            HTuple hred, hgreen, hblue, type, width, height,pImage;

            HOperatorSet.GetImagePointer3(image, out hred, out hgreen, out hblue, out type, out width, out height);
            HOperatorSet.GetImagePointer1(image,out pImage, out type, out width, out height);
            byte* ppp = (byte*)pImage.I;
            res = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            Rectangle rect = new Rectangle(0, 0, width.I, height.I);
            BitmapData bitmapData = res.LockBits(rect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            {
                byte* bptr = (byte*)bitmapData.Scan0;
                byte* r = ((byte*)hred.I);
                byte* g = ((byte*)hgreen.I);
                byte* b = ((byte*)hblue.I);
                for (int i = 0; i < width * height; i++)
                {
                    bptr[i * 4] = (b)[i];
                    bptr[i * 4 + 1] = (g)[i];
                    bptr[i * 4 + 2] = (r)[i];
                    bptr[i * 4 + 3] = 255;
                }
            }
            res.UnlockBits(bitmapData);
        }
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public static System.Windows.Media.ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            System.Windows.Media.ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }
    }

}
