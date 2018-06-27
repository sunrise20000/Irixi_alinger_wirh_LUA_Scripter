using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irixi_Aligner_Common.Configuration.CameraCfg
{
    public class CameraCfg
    {
        public string Name { get; set; }        //UserName:IP
        public string NameForVision { get; set; }   //Vision use
        public int LightValue { get; set; }
        public string ConnectType { get; set; }
    }
}
