using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Helpers.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; set; }

        public AppException(int statusCode = 500)
            : base()
        {
            StatusCode = statusCode;
        }

        public AppException(string message, int statusCode = 500)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public AppException(string message, int statusCode = 500, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
            StatusCode = statusCode;
        }

        public AppException(string message, Exception innerException, int statusCode = 500)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
