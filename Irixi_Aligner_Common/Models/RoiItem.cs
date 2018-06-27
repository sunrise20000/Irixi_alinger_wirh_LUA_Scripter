using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes;
using Irixi_Aligner_Common.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Models
{
    public class RoiItem
    {
        public string StrName { get; set; }
        public string StrFullName { get; set; }
        public RelayCommand<RoiItem> OperateAdd
        {
            get
            {
                return new RelayCommand<RoiItem>(item =>
                {
                    Vision.Vision.Instance.DrawRoi(Convert.ToInt16(item.StrFullName.Substring(3, 1)));
                });
            }
        }
        public RelayCommand<RoiItem> OperateEdit
        {
            get
            {
                return new RelayCommand<RoiItem>(item =>
                {
                    Console.WriteLine(item.StrName);
                });
            }
        }
        public RelayCommand<RoiItem> OperateDelete => new RelayCommand<RoiItem>(item =>
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FileHelper.GetCurFilePathString());
            sb.Append("VisionData\\Roi\\");
            if (!Directory.Exists(sb.ToString()))
                throw new Exception(string.Format("{0} is not exist", sb.ToString()));
            sb.Append(item.StrFullName);
            sb.Append(".reg");
            if(File.Exists(sb.ToString()))
                throw new Exception(string.Format("{0} is not exist", sb.ToString()));
            FileHelper.DeleteFile(sb.ToString());
            Messenger.Default.Send<string>(item.StrFullName, "UpdateRoiFiles");
        });

    }

}
