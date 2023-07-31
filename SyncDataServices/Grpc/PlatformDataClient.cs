using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;
using PlatformService;

namespace CommandsService.SyncDataServices.Grpc
{
    public class PlatformDataClient : IPlatformDataClient
    {
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;

        public PlatformDataClient(IConfiguration configuration, IMapper mapper)
        {
            this.configuration = configuration;
            this.mapper = mapper;
        }

        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            Console.WriteLine($"--> Calling GRPC Service {configuration["GrpcPlatform"]}");

            HttpClientHandler handler = new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };
            GrpcChannel grpcChannel = GrpcChannel.ForAddress(configuration["GrpcPlatform"], new GrpcChannelOptions() { HttpHandler = handler });
            GrpcPlatform.GrpcPlatformClient client = new GrpcPlatform.GrpcPlatformClient(grpcChannel);
            GetAllRequest request = new GetAllRequest();

            try
            {
                PlatformResponse response = client.GetAllPlatforms(request);
                return mapper.Map<IEnumerable<Platform>>(response.Platform);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not call GRPC Server {ex.Message}");
                return null;
            }
        }
    }
}