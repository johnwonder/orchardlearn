using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate.Automapping;

namespace NHibernateTest
{
    class Program
    {
        static void Main(string[] args)
        {
           ISessionFactory session =  AutoMapTest.CreateSessionFactory();
           ISession Session = session.OpenSession();
           //get user from database  
           //User user1 = Session.Load<User>(2);  

           //save the User data  
           Session.Transaction.Begin();

           ////User usreUser = Session.Get<User>(2);  
           //User user = new User()
           //{
           //    Name = "Jack",
           //    No = "12321"
           //};
           //UserDetail userDetails = new UserDetail()
           //{
           //    Age = 12,
           //    BirthDate = DateTime.Now.Date,
           //    Height = 240,
           //    Sex = 1
           //};
           //user.UserDetails = userDetails;
           //userDetails.User = user;
           //Session.Save(user);
           //Session.Save(userDetails);
           Session.Transaction.Commit();  
        }

    }
}
