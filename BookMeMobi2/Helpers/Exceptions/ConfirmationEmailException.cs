using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BookMeMobi2.Helpers.Exceptions
{
    public class ConfirmationEmailException : AppException
    {
        public ConfirmationEmailException(int statusCode = 500)
            : base(statusCode) { }
        public ConfirmationEmailException(string message, int statusCode = 500)
            : base(message, statusCode) { }
        public ConfirmationEmailException(string message, int statusCode = 500, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, statusCode, args)) { }
        public ConfirmationEmailException(string message, Exception innerException, int statusCode = 500)
            : base(message, innerException, statusCode) { }
    }
}
