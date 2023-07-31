using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandsService.Dtos
{
    public class PlatformPublishedDto : GenericEventDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}