namespace AngularAuthApi.Models.Repository
{
    public interface IGenericRepo<T>  where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);
        Task Create(T entity);
        Task Update(int id, T entity);
        Task Delete(int id);
    }
}
