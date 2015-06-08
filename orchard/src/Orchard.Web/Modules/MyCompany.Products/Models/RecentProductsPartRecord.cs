using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace MyCompany.Products.Models
{
    /// <summary>
    /// 用于存储显示最新多少条数据
    /// </summary>
    public class RecentProductsPartRecord : ContentPartRecord
    {
        public virtual int Count { get; set; }
    }
}