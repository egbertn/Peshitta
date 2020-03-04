﻿using System.Collections.Generic;

namespace Peshitta.Infrastructure.Models
{
    public class SearchArguments
    {
        
        public string FindString { get; set; }
        public bool ExactMatch { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
