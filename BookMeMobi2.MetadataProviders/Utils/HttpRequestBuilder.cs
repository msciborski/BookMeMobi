using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookMeMobi2.MetadataProviders.Utils
{
    internal class HttpRequestBuilder
    {
        private HttpMethod _method = null;
        private string _requestUri = String.Empty;
        private HttpContent _content = null;
        private string _acceptHeader = "application/json";
        private TimeSpan _timeout = new TimeSpan(0, 0, 15);
        private bool _allowAutorRedirect = false;

        public HttpRequestBuilder() { }

        public HttpRequestBuilder AddMethod(HttpMethod method)
        {
            _method = method;
            return this;
        }
        public HttpRequestBuilder AddRequestUri(string requestUri)
        {
            _requestUri = requestUri;
            return this;
        }
        public HttpRequestBuilder AddContent(HttpContent content)
        {
            _content = content;
            return this;
        }
        public HttpRequestBuilder AddAcceptHeader(string acceptHeader)
        {
            _acceptHeader = acceptHeader;
            return this;
        }
        public HttpRequestBuilder AddTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }
        public HttpRequestBuilder AllowAutoRedirect(bool allowAutoRedirect)
        {
          _allowAutorRedirect = allowAutoRedirect;
          return this;
        }
        public async Task<HttpResponseMessage> SendAsync()
        {
          // Check if required arguments (method, requestUri) are setted
          CheckArguments();

          HttpRequestMessage request = new HttpRequestMessage(_method, _requestUri);

          // Setting up content
          if (_content != null)
          {
            request.Content = _content;
          }

          // Setting up accept header
          request.Headers.Accept.Clear();
          if (String.IsNullOrEmpty(_acceptHeader))
          {
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(_acceptHeader));
          }

          var handler = new HttpClientHandler();
          handler.AllowAutoRedirect = _allowAutorRedirect;

          HttpClient client = new HttpClient(handler);
          client.Timeout = _timeout;

          return await client.SendAsync(request);
        }

        private void CheckArguments()
        {
            if (_method == null)
            {
                throw new ArgumentNullException("Method");
            }
            if (String.IsNullOrEmpty(_requestUri))
            {
                throw new ArgumentNullException("RequestUri");
            }
        }
    }
}