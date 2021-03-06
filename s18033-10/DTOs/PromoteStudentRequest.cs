﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace s18033_10.DTOs
{
    public class PromoteStudentRequest
    {
        [Required]
        public string Studies { get; set; }

        [Required]
        [Range(0, 10)]
        public int Semester { get; set; }
    }
}
