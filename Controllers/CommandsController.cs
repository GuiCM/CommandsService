using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/commands/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly ICommandRepo commandRepository;

        public CommandsController(IMapper mapper, ICommandRepo commandRepository)
        {
            this.mapper = mapper;
            this.commandRepository = commandRepository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetAll([FromRoute] int platformId)
        {
            Console.WriteLine($"--> Getting Commands from CommandsService: {platformId}");

            if (!commandRepository.PlatformExists(platformId))
            {
                return NotFound(new { message = $"Platform with id: {platformId} was not found." });
            }

            return Ok(mapper.Map<IEnumerable<CommandReadDto>>(commandRepository.GetCommandsForPlatform(platformId)));
        }

        [HttpGet("{commandId}", Name = "GetById")]
        public ActionResult<CommandReadDto> GetById([FromRoute] int platformId, [FromRoute] int commandId)
        {
            Console.WriteLine($"--> Getting Command from CommandsService: {platformId} / {commandId}");

            Command command = commandRepository.GetCommand(platformId, commandId);

            return command != null ? Ok(mapper.Map<CommandReadDto>(command)) : NotFound();
        }

        [HttpPost]
        public ActionResult<IEnumerable<PlatformReadDto>> Create([FromRoute] int platformId, [FromBody] CommandCreateDto commandCreateDto)
        {
            Console.WriteLine($"--> Creating Command in CommandsService: {platformId}");

            if (!commandRepository.PlatformExists(platformId))
            {
                return NotFound();
            }

            Command command = mapper.Map<Command>(commandCreateDto);

            commandRepository.CreateCommand(platformId, command);
            commandRepository.SaveChanges();

            return CreatedAtRoute(nameof(GetById), new { platformId = platformId, commandId = command.Id }, mapper.Map<IEnumerable<PlatformReadDto>>(commandRepository.GetAllPlatforms()));
        }
    }
}