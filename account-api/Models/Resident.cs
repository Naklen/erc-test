﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace account_api.Models
{
    public class Resident
    {
        public int Id { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string Surname { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        public List<Account> Accounts { get; set; }
    }
}