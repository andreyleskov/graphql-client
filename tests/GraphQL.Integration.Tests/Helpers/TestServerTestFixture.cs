using System;
using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Serializer.SystemTextJson;
using IntegrationTestServer;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GraphQL.Integration.Tests.Helpers
{
    public abstract class TestServerTestFixture : IntegrationServerTestFixture
    {
        private HttpClient _testHttpClient;
        private WebSocketClient _testWebSocketClient;
        public ITestOutputHelper Output { get; set; }
        public override async Task CreateServer()
        {
            var host =
                new WebHostBuilder()
                    .UseStartup<Startup>()
                    .ConfigureLogging((ctx, logging) =>
                    {
                        logging.AddProvider(new XUnitLoggerProvider(Output, new XUnitLoggerOptions()));
                        logging.SetMinimumLevel(LogLevel.Trace);
                    });

            var testServer = new TestServer(host);
            _testHttpClient = testServer.CreateClient();
            _testWebSocketClient = testServer.CreateWebSocketClient();
           // _testWebSocketClient.SubProtocols.Add("graphql-ws");
            _testWebSocketClient.ConfigureRequest = r =>
            {
                r.Headers["Sec-WebSocket-Protocol"] = "graphql-ws";
            };

            Server = testServer.Host;
            await testServer.Host.StartAsync();
        }

        protected override GraphQLHttpClient GetGraphQLClient(string endpoint, bool requestsViaWebsocket = false)
        {
            if (Serializer == null)
                throw new InvalidOperationException("JSON serializer not configured");

            return new GraphQLHttpClient(new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri($"http://localhost:{Port}{endpoint}"),
                    UseWebSocketForQueriesAndMutations = requestsViaWebsocket
                },
                Serializer,_testHttpClient,(uri,token)=>
                {
                    return _testWebSocketClient.ConnectAsync(uri, token);
                });
        }
    }

    public class TestServerTestSystemTextFixture : TestServerTestFixture
    {
        public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new SystemTextJsonSerializer();
    }

    public class TestServerTestNewtonsoftFixture : TestServerTestFixture
    {
        public override IGraphQLWebsocketJsonSerializer Serializer { get; } = new NewtonsoftJsonSerializer();
    }
}
