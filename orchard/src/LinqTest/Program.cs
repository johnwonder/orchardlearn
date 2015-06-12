using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace LinqTest
{
    class Program
    {
        static void Main(string[] args)
        {
            
            SimpleExpressions();

            ParameteExpressions();

            Console.ReadLine();
        }
        /// <summary>
        /// http://www.cnblogs.com/Terrylee/archive/2008/08/01/custom-linq-provider-part-1-expression-tree.html
        /// </summary>
        static void ExpressionDelegate()
        {
            //无返回类型用Action
            //有返回类型用Func
            //http://www.cnblogs.com/Gyoung/archive/2013/04/04/2997050.html
            Expression<Action<int, int>> expression = (a, b) => Console.WriteLine((a * b) + 2);

            expression.Compile()(2, 3);
        }

        static void SimpleExpressions()
        {
            // simple expression : 2+3
            Expression e = Expression.Add(Expression.Constant(2), Expression.Constant(3));

            //lambda express : () => 2+3;
            LambdaExpression l = Expression.Lambda(e, null);

            // compile to delegate
            Delegate d = l.Compile();

            //convert to 
            Func<int> f = (Func<int>)d;

            Console.WriteLine(e);
            Console.WriteLine(l);
            Console.WriteLine(d);
            Console.WriteLine(f());
        }

        static void ParameteExpressions()
        {
            ParameterExpression p = Expression.Parameter(typeof(int), "x");
            //expression x+3
            Expression e2 = Expression.Add(p, Expression.Constant(3));
            //Lambda : x => x+3 加个 ParameterExpression
            LambdaExpression l2 = Expression.Lambda(e2, new ParameterExpression[] { p });

            Delegate d2 = l2.Compile();
            Func<int,int> f2 = (Func<int,int>)d2;

            Console.WriteLine(e2);
            Console.WriteLine(l2);
            Console.WriteLine(d2);
            Console.WriteLine(f2(3));
        }
    }
}
