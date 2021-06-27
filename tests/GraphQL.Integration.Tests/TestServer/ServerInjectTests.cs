using System.Net.Http;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using IntegrationTestServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GraphQL.Integration.Tests.TestServer
{
    public class ServerInjectTests
    {
        [Fact]
        public async Task
            GIVEN_graphQlClient_WHEN_subscribing_for_TestServer_THEN_can_receive_messages_from_subscription()
        {
             var host =
                new WebHostBuilder()
                    .UseStartup<Startup>()
                    .ConfigureLogging((ctx, logging) => logging.SetMinimumLevel(LogLevel.Debug));


            var testServer = new Microsoft.AspNetCore.TestHost.TestServer(host);
            var httpClient = testServer.CreateClient();
            var webSocketClient = testServer.CreateWebSocketClient();

            var client = new GraphQLHttpClient(new GraphQLHttpClientOptions(),new SystemTextJsonSerializer(),httpClient,)
        }
    }
}
