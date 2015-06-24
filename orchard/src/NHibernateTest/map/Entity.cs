using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernateTest.map
{
    public abstract  class Entity
    {
        virtual public int ID { get; set; }
    }

    public  class Project : Entity
    {
        public Project()
        {

        }

        public virtual string Name { get; set; }

        //public virtual User User { get; set; }

        //public virtual IList<Product> Product { get; set; }

        //public virtual IList<Task> Task { get; set; }
    }
}
