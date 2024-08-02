using System.Linq.Expressions;

namespace TelegramUsernameFinder.Interfaces
{
    public interface IRepository<T>
    {
        List<T> GetAll();

        List<T> GetAllFit(Expression<Func<T, bool>> func);

        T Get(int id);

        void Add(T entity);

        void Update(T entity);

        void Delete(int id);
    }
}
