using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Helpers.Exceptions
{
    public class BookNoFoundException : AppException
    {
        public BookNoFoundException(int statusCode = 500)
            : base(statusCode) { }
        public BookNoFoundException(string message, int statusCode = 500)
            : base(message, statusCode) { }
        public BookNoFoundException(string message, int statusCode = 500, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, statusCode, args)) { }
        public BookNoFoundException(string message, Exception innerException, int statusCode = 500)
            : base(message, innerException, statusCode) { }
    }
}
