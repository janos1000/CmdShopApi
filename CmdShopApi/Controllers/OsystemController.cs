using CmdShopApi.DTOs;
using CmdShopApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CmdShopApi.Controllers
{
    [Route("osystem")]
    [ApiController]
    public class OsystemController : ControllerBase
    {
        public ComputerShopDbContext context = new ComputerShopDbContext();

        [HttpGet("getAll")]
        public object GetAllOsystem() 
        { 
            var osystems = context.Osystems.ToList();
            return new { message = "Sikeres lekérdezés", result = osystems };
        }

        [HttpPost]
        public object AddNewOsystem(AddNewOsystemDto addNewOsystemDto)
        {
            var osystem = new Osystem
            {
                Id = Guid.NewGuid(),
                Name = addNewOsystemDto.Name,
                Version = addNewOsystemDto.Version,
                RegisterTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            context.Osystems.Add(osystem);
            context.SaveChanges();

            return StatusCode(201, new { message = "Sikeres felvétel", result = osystem });
        }

        [HttpPut]
        public object UpdateOsystem([FromQuery]Guid id, [FromBody]UpdateOsystem updateOsystem)
        {
            var osystem = context.Osystems.FirstOrDefault(osystem => osystem.Id == id);

            if (osystem != null)
            {
                osystem.Name = updateOsystem.Name;
                osystem.Version = updateOsystem.Version;
                osystem.UpdateTime = DateTime.Now;

                context.Osystems.Update(osystem);
                context.SaveChanges();
            }

                return (404, new { message = "Sikeres frissítés", result = osystem });

            }

        [HttpDelete]
        public object DeleteOsystem([FromQuery] Guid id)
        {
            var osystem = context.Osystems.Find(id);

            if (osystem != null)
            {
                context.Osystems.Remove(osystem);
                context.SaveChanges();

                return StatusCode(204, new { message = "Sikeres törlés"});

            }
            else
            {
                return StatusCode(404, new { message = "Sikertelen törlés"});
            }
        }
    }
}
