using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace client.DTOs
{
    public class EditBookDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }

        public int? PressId { get; set; }
        public int? AddressId { get; set; }

        public IEnumerable<SelectListItem>? Presses { get; set; }
        public IEnumerable<SelectListItem>? Addresses { get; set; }
    }
}