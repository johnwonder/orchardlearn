using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClaySharp;

namespace ClayTest
{
    class Program
    {
        /// <summary>
        /// http://www.cnblogs.com/JustRun1983/p/3529157.html
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
          var display =   CreateHelper("ss","ssss");
          display("11",new List<string> {"22" });
        }
        static private readonly DisplayHelperBehavior[] _behaviors = new[] { new DisplayHelperBehavior() };
        public static dynamic CreateHelper(string s1, string s2)
        {
            return ClayActivator.CreateInstance<DisplayHelper>(
                _behaviors,
                s1, s2);
        }

        static void SimpleObject()
        {
            dynamic New = new ClayFactory();
            //var person = New.Person();
            //person.FirstName = "Louis";
            //person.LastName = "Dejardin";

            //使用索引器的方式实现初始化
            //person["FirstName"] = "Louis";
            //person["LastName"] = "Dejardin";

            //使用匿名对象的方式实现初始化
            //var person = New.Person(new
            //{
            //    FirstName = "Louis",
            //    LastName = "Dejardin"
            //});

            //使用命名参数的方式初始化
            //var person = New.Person(
            //    FirstName: "Louis",
            //    LastName: "Dejardin"
            //    );
            //链接方式初始化
            var person = New.Person()
                                        .FirstName("Louis")
                                        .LastName("Dejardin");

            //Console.WriteLine(person.FirstName);
            //Console.WriteLine(person["FirstName"]);
            //Console.WriteLine(person.FirstName);

            //Console.WriteLine(person.FirstName +" "+ person.LastName);



            Console.ReadLine();
        }

        static void SimpleArray()
        {
            dynamic New = new ClayFactory();

            var people = New.Array(
                      New.Person().FirstName("Louis").LastName("Dejardin"),
                      New.Person().FirstName("Bertrand").LastName("Le Roy")
                );

            //Console.WriteLine("Count = {0}", people.Count);

            //Console.WriteLine("people[0].FirstName ={0}", people[0].FirstName);

            //foreach (var person in people)
            //{
            //    Console.WriteLine("{0} {1}", person.FirstName, person.LastName);
            //}

            

        }

        class DisplayHelperBehavior : ClayBehavior
        {
            public override object InvokeMember(Func<object> proceed, object target, string name, INamedEnumerable<object> args)
            {
                return ((DisplayHelper)target).Invoke(name, args);
            }


        }
    }

    public class DisplayHelper
    {
        private readonly string _s1;
        private readonly string _s2;

        public DisplayHelper(
            string s1,
            string s2)
        {
            _s1 = s1;
            _s2 = s2;

        }

        public String S1 { get; set; }
        public String S2 { get; set; }

        public object Invoke(string name, INamedEnumerable<object> parameters)
        {
            return null;
        }
    }
}
