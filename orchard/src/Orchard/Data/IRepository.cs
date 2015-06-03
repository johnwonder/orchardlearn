using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Orchard.Data {
    /// <summary>
    /// 通过该接口操作数据库
    /// 只需要通过IRepository<T>接口就可以对相应实体T所对应的数据表进行增、删、改、查的操作了
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Copy(T source, T target);
        void Flush();

        T Get(int id);
        T Get(Expression<Func<T, bool>> predicate);

        IQueryable<T> Table { get; }

        int Count(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip, int count);
    }
}