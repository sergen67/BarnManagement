// Entities/Animal.cs  (dosya adını da Animal.cs yap)
using System;

namespace BarnManagement.WinForms.Models.Entities   // kendi gerçek namespace’in
{
    public class Animal
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int AgeDays { get; set; }
        public string Gender { get; set; }
        public int LifetimeDays { get; set; }
        public bool IsAlive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
