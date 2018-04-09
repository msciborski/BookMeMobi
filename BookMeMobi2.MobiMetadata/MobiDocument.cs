using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BookMeMobi2.MobiMetadata.Headers;

namespace BookMeMobi2.MobiMetadata
{
    public class MobiDocument
    {
        #region Properties

        public PdbHeader PdbHeader { get; set; }
        public MobiHeader MobiHeader { get; set; }
        //public string FilePath { get; private set; }
        public string Title => GetTitle();
        public string Author => GetAuthor();
        public DateTime? PublishingDate => GetPublishingDate();
        public string Asin => GetAsin();

        #endregion


        internal MobiDocument()
        {
        }

        private string GetAsin()
        {
            if (MobiHeader == null)
                return "";

            var firstAsin = MobiHeader.GetExthRecordValue(113);
            var secondAsin = MobiHeader.GetExthRecordValue(504);

            return firstAsin ?? secondAsin;
        }

        private string GetAuthor()
        {
            if (MobiHeader == null)
                return "";

            return MobiHeader.GetExthRecordValue(100);
        }

        private DateTime? GetPublishingDate()
        {
            if (MobiHeader == null)
                return null;

            var date = MobiHeader.GetExthRecordValue(106);
            return Convert.ToDateTime(date);
        }

        private string GetTitle()
        {

            if (MobiHeader == null)
                return PdbHeader.Name;

            return MobiHeader.FullName;
        }

    }
}
