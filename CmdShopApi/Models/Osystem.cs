using System.ComponentModel.DataAnnotations;

namespace CmdShopApi.Models
{
    public class Osystem
    {
        [Key]
        [MaxLength (36)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime RegisterTime { get; set; }

    }
}
