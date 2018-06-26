using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2
{
    public class JWTSettings
    {
        public string Secret { get; set; }
        public int AccessTokenExpiry { get; set; }
        public int RefreshTokenExpiry { get; set; }
    }
}
