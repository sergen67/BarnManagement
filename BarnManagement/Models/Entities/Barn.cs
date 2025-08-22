using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarnManagement.Models.Entities
{
    public class Barn
    {
        public int Id { get; set; }
        public int TotalCapacity { get; set; }
        public int CurrentAnimalCount { get; set; }
        public  DateTime CreatedAt { get; set; } = DateTime.Now;
       
}
}
