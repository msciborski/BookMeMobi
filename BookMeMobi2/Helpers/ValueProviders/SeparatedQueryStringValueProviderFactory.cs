using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookMeMobi2.Helpers.ValueProviders
{
    public class SeparatedQueryStringValueProviderFactory : IValueProviderFactory
    {
        private readonly string _separator;
        private readonly string _key;

        public SeparatedQueryStringValueProviderFactory(string separator)
          : this(null, separator)
          {
          }
        public SeparatedQueryStringValueProviderFactory(string key, string separator)
        {
          _key = key;
          _separator = separator;
        }

        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
          context.ValueProviders.Insert(0,
                        new SeparatedQueryStringValueProvider(_key, context.ActionContext.HttpContext.Request.Query, _separator));
          return Task.CompletedTask;
        }
    }
}