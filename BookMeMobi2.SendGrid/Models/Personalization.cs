using System;
using System.Collections.Generic;
using System.Text;

namespace BookMeMobi2.SendGrid.Models
{
    public class Personalization
    {
        public List<EmailAddress> To { get; set; }
        public string Subject { get; set; }

        internal Personalization() { }
    }
}
