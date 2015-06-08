using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using MyCompany.Products.Models;

namespace MyCompany.Products.Handlers
{
    public class RecentProductsPartHandler : ContentHandler
    {
        public RecentProductsPartHandler(IRepository<RecentProductsPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
