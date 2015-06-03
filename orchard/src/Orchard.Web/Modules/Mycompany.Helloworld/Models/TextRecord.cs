using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mycompany.Helloworld.Models
{
    public class TextRecord
    {
        public virtual int Id { get; set; }
        public virtual string Content { get; set; }
    }
}