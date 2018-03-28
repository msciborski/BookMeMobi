using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Helpers.Exceptions
{
    public class BookNoFoundException : AppException
    {
        public BookNoFoundException()
            : base() { }
        public BookNoFoundException(string message)
            : base(message) { }
        public BookNoFoundException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args)) { }
        public BookNoFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
