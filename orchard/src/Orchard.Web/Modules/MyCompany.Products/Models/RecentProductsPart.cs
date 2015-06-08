using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace MyCompany.Products.Models
{
    public class RecentProductsPart : ContentPart<RecentProductsPartRecord>
    {
        [Required]
        public int Count
        {
            get { return Record.Count; }
            set { Record.Count = value; }
        }
    }
}