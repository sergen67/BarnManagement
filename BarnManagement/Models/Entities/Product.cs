using System;

namespace BarnManagement.WinForms.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string ProductType { get; set; } // Milk/Egg/Wool...
        public decimal Quantity { get; set; }
        public DateTime ProducedAt { get; set; } = DateTime.Now;
        public bool IsSold { get; set; } = false;

        public virtual Animal Animal { get; set; }
    }
}
