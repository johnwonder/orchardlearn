using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Orchard.Data {
    /// <summary>
    /// ͨ���ýӿڲ������ݿ�
    /// ֻ��Ҫͨ��IRepository<T>�ӿھͿ��Զ���Ӧʵ��T����Ӧ�����ݱ��������ɾ���ġ���Ĳ�����
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