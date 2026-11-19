using Giny.Protocol.Enums;
using Giny.World.Managers.Experiences;
using Giny.World.Network;
using Giny.World.Records.Characters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProfileController : ControllerBase
    {
        [HttpGet]
        [Route("{name}")]
        public object GetProfile(string name)
        {
            CharacterRecord record = CharacterRecord.GetCharacterRecords()
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (record == null)
            {
                return null;
            }

            bool online = WorldServer.Instance.GetOnlineClients()
                .Any(x => x.Character != null && x.Character.Id == record.Id);

            return new
            {
                Id = record.Id,
                Name = record.Name,
                Level = ExperienceManager.Instance.GetCharacterLevel(record.Experience),
                Breed = (BreedEnum)record.BreedId,
                Sex = record.Sex,
                Online = online,
            };
        }
    }
}
