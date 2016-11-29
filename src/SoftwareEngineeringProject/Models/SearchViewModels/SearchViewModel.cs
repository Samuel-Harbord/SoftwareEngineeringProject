﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SoftwareEngineeringProject.Models.SearchViewModels
{
    public class SearchViewModel
    {
        [Display(Name = "Query")]
        public string Query { get; set; }
    }
}
