using System;

namespace BarnManagement.Models.Entities
{
    public class Barn
    {
        public int Id { get; set; }
        public int TotalCapacity { get; set; }
        public int CurrentAnimalCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
