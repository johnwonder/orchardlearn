using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace MyCompany.Products.Models
{
    /// <summary>
    /// 在Orchard中如果需要实现内容部件的功能，相应的数据模型需要继承于ContentPartRecord类，
    /// 这样Orchard框架就可以根据Content Id 来将相关的内容组织在一起。
    /// 另外在数据模型中的所有属性都必须加入virtual关键字，这应该是数据访问和OR映射的需要。
    /// </summary>
    public class ProductRecord:ContentPartRecord
    {
        public virtual double Price { get; set; }
        public virtual string Brand{get;set;}
    }
}