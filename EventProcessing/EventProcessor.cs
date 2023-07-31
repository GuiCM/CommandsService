using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IMapper mapper;

        public EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            EventType eventType = DetermineEventType(message);

            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;
                default:
                    Console.WriteLine("--> Event type undetermined, ignoring event.");
                    break;
            }
        }

        private EventType DetermineEventType(string notificationMessage)
        {
            Console.WriteLine("--> Determining Event");

            GenericEventDto genericEventDto = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (genericEventDto.Event)
            {
                case "Platform_Published":
                    Console.WriteLine("--> Platform Published Event Detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> Could not determine the event type");
                    return EventType.Undetermined;
            }
        }

        private void AddPlatform(string platformMessage)
        {
            using (IServiceScope scope = serviceScopeFactory.CreateScope())
            {
                ICommandRepo commandRepo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                PlatformPublishedDto platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformMessage);

                try
                {
                    Platform platform = mapper.Map<Platform>(platformPublishedDto);

                    if (!commandRepo.PlatformExistsByExternalId(platform.ExternalId))
                    {
                        commandRepo.CreatePlatform(platform);
                        commandRepo.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("--> Platform already exists");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not add Platform to DB: {ex.Message}");
                }
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}