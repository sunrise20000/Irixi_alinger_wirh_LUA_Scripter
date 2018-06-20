using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Irixi_Aligner_Common.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Models
{
    public class TemplateItem
    {
        public string StrName { get; set; }
        public string StrFullName { get; set; }

        public RelayCommand<TemplateItem> OperateAdd
        {
            get
            {
                return new RelayCommand<TemplateItem>(item => {
                    Console.WriteLine(item.StrName);
                });
            }
        }
        public RelayCommand<TemplateItem> OperateEdit
        {
            get
            {
                return new RelayCommand<TemplateItem>(item => {
                    Console.WriteLine(item.StrName);
                });
            }
        }
        public RelayCommand<TemplateItem> OperateDelete
        {
            get
            {
                return new RelayCommand<TemplateItem>(item => {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(FileHelper.GetCurFilePathString());
                    sb.Append("VisionData\\Model\\");
                    sb.Append(item.StrFullName);
                    sb.Append(".shm");
                    if (UserControls.UC_MessageBox.Instance.ShowBox(string.Format("确定要删除{0}吗?", item.StrName)) == System.Windows.MessageBoxResult.Yes)
                    {
                        FileHelper.DeleteFile(sb.ToString());
                        Messenger.Default.Send<string>(item.StrFullName, "UpdateTemplateFiles");
                    }
                });
            }
        }
    }
}
