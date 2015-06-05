using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace MyCompany.Products.Models
{
    public class ProductPart:ContentPart<ProductRecord>
    {
        [Required]
        public double Price
        {
            get { return Record.Price; }
            set { Record.Price = value; }
        }

        [Required]
        public string Brand
        {
            get { return Record.Brand; }
            set { Record.Brand = value; }
        }
    }
}