using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;

namespace CommandsService.Data
{
    public class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                IPlatformDataClient platformDataClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                IEnumerable<Platform> platforms = platformDataClient.ReturnAllPlatforms();

                SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
            }
        }

        private static void SeedData(ICommandRepo commandRepo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--> Seeding new platforms...");

            foreach (Platform platform in platforms)
            {
                if (!commandRepo.PlatformExistsByExternalId(platform.ExternalId))
                {
                    commandRepo.CreatePlatform(platform);
                }

                commandRepo.SaveChanges();
            }
        }
    }
}