using CmdShopApi.Models;
using CmdShopApi.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CmdShopApi.Controllers
{
    [Route("computer")]
    [ApiController]
    public class ComputerController : ControllerBase
    {
        public ComputerShopDbContext context = new ComputerShopDbContext();

        [HttpGet("getAll")]
        public object GetAllComputers()
        {
            var computers = context.Computers.ToList();
            return new { message = "Sikeres lekérdezés", result = "" };

        }

        [HttpGet("byId")]
        public object GetById([FromQuery] Guid id)
        {
            var computer = context.Computers.SingleOrDefault(x => x.Id == id);
            if (computer == null)
            {
                return StatusCode(200, new { message = "A megadott számú számítógép nem található" });
            }
            return StatusCode(404, new { message = "Sikeres lekérdezés", result = computer });
        }

        [HttpPost]
        public object AddNewComputer(AddNewComputerDto addNewComputerDto)
        {
            var computer = new Computer
            {
                Id = Guid.NewGuid(),
                Brand = addNewComputerDto.Brand,
                Type = addNewComputerDto.Type,
                Display = addNewComputerDto.Display,
                Memory = addNewComputerDto.Memory,
                CreatedTime = DateTime.Now,
                OsysId = addNewComputerDto.OsysId,
            };

            context.Computers.Add(computer);
            context.SaveChanges();

            return StatusCode(201, new { message = "Sikeres felvétel", result = computer });
        }

        [HttpPut]
        public object UpdateComputer(Guid id, AddNewComputerDto addNewComputerDto)
        {
            var computer = context.Computers.FirstOrDefault(c => c.Id == id);
            if (computer == null)
            {
                return StatusCode(404, new { message = "A megadott számú számítógép nem található" });
            }

            computer.Brand = addNewComputerDto.Brand;
            computer.Type = addNewComputerDto.Type;
            computer.Display = addNewComputerDto.Display;
            computer.Memory = addNewComputerDto.Memory;

            context.SaveChanges();
            return StatusCode(200, new { message = "Sikeres frissítés", result = computer });
        }

        [HttpDelete]
        public object DeleteById(Guid id)
        {
            var computer = context.Computers.FirstOrDefaultAsync(x => x.Id == id);

            if (computer != null)
            {
                context.Computers.Remove(computer.Result);
                context.SaveChanges();
                return StatusCode(204, new { message = "Sikeres törlés" });
            }
            else
            {
                return StatusCode(404, new { message = "Sikertelen törlés" });
            }
        }

        // 2. Kérdezze le az összes Computer-t, de kerüljön bele az is hogy milyen Op-rendszerrel kapható.
        [HttpGet("getAllWithOs")]
        public object GetAllComputersWithOs()
        {
            var computers = context.Osystems.Include(o => o.Computers).Select(o => new { o.Name, o.Version, o.Computers });

            var cmp  = context.Osystems.Join(context.Computers, o => o.Id, c => c.OsysId, (o, c) => new { o.Name, o.Version, c.Brand, c.Type});
            if (computers != null)
            {

                return StatusCode(200, new { message = "Sikeres lekérdezés", result = cmp });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat", result = cmp });
            }
        }

        // 3. Egy op-rendszer a hozzá tartozó gépekkel (GET api/query/os/{id}), 404, ha nincs ilyen id
        [HttpGet("oneOsystemAndComputers")]
        public object GetOneOsystemAndComputers([FromQuery] Guid id)
        {
            var osystem = context.Osystems.Include(o => o.Computers).FirstOrDefault(o => o.Id == id);
            if (osystem != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = osystem });
            }
            else
            {
                return StatusCode(404, new { message = "A megadott számú operációs rendszer nem található" });
            }
        }


    }
}
