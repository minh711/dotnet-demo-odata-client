using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace client.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string ISBN { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public decimal Price { get; set; }

        [ForeignKey("AddressId")]
        public int LocationId { get; set; }

        [ForeignKey("PressId")]
        public int PressId { get; set; }

        public Address? Location { get; set; }

        public Press? Press { get; set; }
    }
}