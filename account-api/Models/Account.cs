using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace account_api.Models
{
    public class Account
    {
        public int Id { get; set; }
        
        public string AccountNumber { get; set; }
        [Required]
        public DateTime OpenDate { get; set; }
        [Required]
        public DateTime CloseDate { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public double SpaceArea { get; set; }
        public List<Resident> Residents { get; set; }
    }
}
