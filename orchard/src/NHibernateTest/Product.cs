using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernateTest.Entity
{
    public  class Product
    {
        public virtual int ID { get; set; }

        public virtual string  Name { get; set; }

        public virtual decimal Price { get; set; }

        public virtual DateTime CreateTime { get; set; }

    }

    public class Order
    {
        public virtual int   ID { get; set; }

        public virtual decimal Price { get; set; }

        public virtual string State { get; set; }

        public virtual DateTime CreateTime { get; set; }
    }

    //public enum OrderState
    //{
    //    Created,
    //    Paied,
    //    Consignment,
    //    Complete
    //}
}
