using System;

namespace BarnManagement.WinForms.Models.Entities
{
    public class Sale
    {
        public int Id { get; set; }

        public int ProductId { get; set; } // unique
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public DateTime SoldAt { get; set; } = DateTime.Now;

        public decimal Revenue => UnitPrice * Quantity; // hesaplanır
        public virtual Product Product { get; set; }
    }
}
