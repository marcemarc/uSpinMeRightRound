using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace tooorangey.uSpinMeRightRound.Models
{
    public class MemoryStreamPostedFile : HttpPostedFileBase
    {
    
        public MemoryStreamPostedFile(MemoryStream ms, string fileName)
        {
            this.ContentLength = (int)ms.Length;
            this.FileName = fileName;
            this.InputStream = ms;
        }
        public override int ContentLength {
            get; }

        public override string FileName { get; }

        public override Stream InputStream { get; }

 
    }
}
