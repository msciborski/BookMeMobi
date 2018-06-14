using System;
using System.Globalization;

namespace BookMeMobi2.Helpers.Exceptions 
{
  public class DbException : AppException 
  {
    public DbException (int statusCode = 500) : base (statusCode) { }
    public DbException (string message, int statusCode = 500) : base (message, statusCode) { }
    public DbException (string message, int statusCode = 500, params object[] args) : base (String.Format (CultureInfo.CurrentCulture, message, statusCode, args)) { }
    public DbException (string message, Exception innerException, int statusCode = 500) : base (message, innerException, statusCode) { }
  }
}