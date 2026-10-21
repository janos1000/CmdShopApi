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
        [HttpGet("GetAllWithOs")]
        public object GetAllComputersWithOs()
        {
            var computers = context.Osystems.Include(o => o.Computers).Select(o => new { o.Name, o.Version, o.Computers });

            var cmp = context.Osystems.Join(context.Computers, o => o.Id, c => c.OsysId, (o, c) => new { o.Name, o.Version, c.Brand, c.Type });
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
        [HttpGet("OneOsystemAndComputers")]
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

        // 4. Egy gép az op-rendszere nevével (GET api/query/computer/{id}), 404, ha nincs ilyen id
        [HttpGet("OneComputerAndOsystem")]
        public object GetOneComputerAndOsystem([FromQuery] Guid id)
        {
            var computer = context.Computers.Join(context.Osystems, c => c.OsysId, o => o.Id, (c, o) => new { c.Id, c.Brand, c.Type, o.Name, o.Version });

            if (computer != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computer });
            }
            else
            {
                return StatusCode(404, new { message = "A megadott számú számítógép nem található" });
            }
        }

        // 6. Adott márkájú gépek op-rendszerrel (GET api/query/brand/{brand})
        [HttpGet("BrandAndOsystem")]
        public object GetBrandAndOsystem([FromQuery] string brand)
        {
            var computers = context.Computers.Where(c => c.Brand == brand).Join(context.Osystems, c => c.OsysId, o => o.Id, (c, o) => new { c.Brand, c.Type, o.Name, o.Version });
            if (computers != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computers });
            }
            else
            {
                return StatusCode(404, new { message = "A megadott márkájú számítógép nem található" });
            }
        }

        // 7. Minden gép op-rendszerrel, márka majd típus szerint rendezve (GET api/query/computers-sorted)
        [HttpGet("ComputersSorted")]
        public object GetComputersSorted()
        {
            var computers = context.Computers.Join(context.Osystems, c => c.OsysId, o => o.Id, (c, o) => new { c.Brand, c.Type, o.Name, o.Version }).OrderBy(c => c.Brand).ThenBy(c => c.Type);
            if (computers != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computers });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 8. Adott képernyőméretnél nagyobb gépek op-rendszerrel (GET api/query/min-display/{size})
        [HttpGet("MinDisplay")]
        public object GetMinDisplay([FromQuery] int size)
        {
            var computers = context.Computers.Where(c => c.Display > size).Join(context.Osystems, c => c.OsysId, o => o.Id, (c, o) => new { c.Brand, c.Type, c.Display, o.Name, o.Version });
            if (computers != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computers });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 14. Legutóbb felvett gépek (latest)
        [HttpGet("Latest")]
        public object GetLatestComputers()
        {
            var computers = context.Computers.OrderByDescending(c => c.CreatedTime).Take(5).Join(context.Osystems, c => c.OsysId, o => o.Id, (c, o) => new { c.Brand, c.Type, c.Display, c.Memory, o.Name, o.Version });
            if (computers != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computers });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 15. Márkánkénti összesítés (brand-summary)
        [HttpGet("BrandSummary")]
        public object GetBrandSummary()
        {
            var brandSummary = context.Computers.GroupBy(c => c.Brand)
                .Select(g => new { Brand = g.Key, ComputerCount = g.Count() });
            if (brandSummary != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = brandSummary });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }
    }
}