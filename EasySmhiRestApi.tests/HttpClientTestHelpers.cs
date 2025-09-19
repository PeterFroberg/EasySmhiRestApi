namespace EasySmhiRestApi.tests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;

    public static class HttpClientTestHelpers
    {
        public static HttpClient CreateFakeHttpClient(Func<HttpRequestMessage, string> responseSelector)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync",
                       ItExpr.IsAny<HttpRequestMessage>(),
                       ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
                   {
                       var body = responseSelector(req);
                       return new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(body)
                       };
                   });
            return new HttpClient(handler.Object);
        }
    }
}
