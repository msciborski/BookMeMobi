using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Helpers.Exceptions
{
    public class UserNoFoundException : AppException
    {
        public UserNoFoundException()
            : base() { }
        public UserNoFoundException(string message)
            : base(message) { }
        public UserNoFoundException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args)) { }
        public UserNoFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
