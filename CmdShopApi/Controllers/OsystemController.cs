using CmdShopApi.Models;
using CmdShopApi.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public object UpdateOsystem([FromQuery] Guid id, [FromBody] UpdateOsystem updateOsystem)
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

                return StatusCode(204, new { message = "Sikeres törlés" });

            }
            else
            {
                return StatusCode(404, new { message = "Sikertelen törlés" });
            }
        }

        // 1. Kérdezze le az összes Osystem - et a hozzá tartozó számítógépekkel.
        [HttpGet("GetAllWithComputers")]
        public object GetAllOsystemWithComputers()
        {
            var computers = context.Osystems.Include(o => o.Computers).ToList();
            if (computers.Count > 0)
            {

                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computers });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 5. Op-rendszerek a hozzájuk tartozó gépek számával (GET api/query/os-computer-count)
        [HttpGet("OsComputerCount")]
        public object GetOsComputerCount()
        {
            var osCounts = context.Osystems.Join(context.Computers, o => o.Id, c => c.OsysId, (o, c) => new { o.Id, o.Name, o.Version, ComputerCount = context.Computers.Count(cc => cc.OsysId == o.Id) });

            if (osCounts != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = osCounts });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        //6. Adott márkájú gépek op-rendszerrel (GET api/query/brand/{brand})
        [HttpGet("BrandWithOs")]
        public object GetBrandWithOs([FromQuery] string brand)
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

        // 9. Op-rendszerenkénti statisztika (os-stats)
        [HttpGet("OsStats")]
        public object GetOsStats()
        {
            var osStats = context.Osystems.Join(context.Computers, o => o.Id, c => c.OsysId, (o, c) => new { o.Id, o.Name, o.Version, ComputerCount = context.Computers.Count(cc => cc.OsysId == o.Id) })
                .GroupBy(o => new { o.Id, o.Name, o.Version })
                .Select(g => new { g.Key.Id, g.Key.Name, g.Key.Version, ComputerCount = g.Sum(x => x.ComputerCount) });
            if (osStats != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = osStats });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 10. Gépek op-rendszer neve szerint (by-os)
        [HttpGet("ByOs")]
        public object GetComputersByOs([FromQuery] string osName)
        {
            var computers = context.Computers.Join(context.Osystems, c => c.OsysId, o => o.Id, (c, o) => new { c.Brand, c.Type, c.Display, c.Memory, o.Name })
                .Where(x => x.Name == osName)
                .ToList();
            if (computers.Count > 0)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computers });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 11. Gép nélküli op-rendszerek
        [HttpGet("OsWithoutComputers")]
        public object GetOsWithoutComputers()
        {
            var osWithoutComputers = context.Osystems
                .Where(o => !context.Computers.Any(c => c.OsysId == o.Id))
                .ToList();
            if (osWithoutComputers.Count > 0)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = osWithoutComputers });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 12. Legnagyobb memóriájú gép op-rendszerenként
        [HttpGet("MaxMemoryByOs")]
        public object GetMaxMemoryByOs()
        {
            var maxMemoryByOs = context.Osystems.Join(context.Computers, o => o.Id, c => c.OsysId, (o, c) => new { o.Name, o.Version, c.Memory })
                .GroupBy(x => new { x.Name, x.Version })
                .Select(g => new { g.Key.Name, g.Key.Version, MaxMemory = g.Max(x => x.Memory) });
            if (maxMemoryByOs != null)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = maxMemoryByOs });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }

        // 13. Memória és OS-verzió szerinti szűrők
        [HttpGet("FilterByMemoryAndOsVersion")]
        public object GetComputersByMemoryAndOsVersion([FromQuery] int memory, [FromQuery] string osVersion)
        {
            var computers = context.Computers.Join(context.Osystems, c => c.OsysId, o => o.Id, (c, o) => new { c.Brand, c.Type, c.Memory, o.Version })
                .Where(x => x.Memory >= memory && x.Version == osVersion)
                .ToList();
            if (computers.Count > 0)
            {
                return StatusCode(200, new { message = "Sikeres lekérdezés", result = computers });
            }
            else
            {
                return StatusCode(404, new { message = "Nincs elérhető adat" });
            }
        }
    }
}