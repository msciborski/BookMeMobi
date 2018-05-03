using System;
using System.Collections.Generic;
using System.Text;

namespace BookMeMobi2.SendGrid.Models
{
    public class Attachment
    {
        public string Content { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
        public string Disposition { get; set; }
        public string ContentId { get; set; }

        internal Attachment() { }
    }

}
