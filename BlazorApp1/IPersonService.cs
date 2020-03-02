using BlazorApp1.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1
{
    public interface IPersonService
    {
        Task Add(Person person);
        Task Delete(int Id);
        Task Edit(Person person);
        Task<Person> Get(int Id);
        Task<List<Person>> FindAll();
    }
}