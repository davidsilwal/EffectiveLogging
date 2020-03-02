using BlazorApp1.Data;
using Microsoft.AspNetCore.Blazor;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp1
{
    public class PersonService : IPersonService
    {
        private readonly HttpClient httpClient;

        public PersonService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<Person>> FindAll()
        {
            return await httpClient.GetJsonAsync<List<Person>>("/api/people");

        }

        public async Task Edit(Person person)
        {
            await httpClient.SendJsonAsync(HttpMethod.Put, $"/api/people/{person.Id}", person);
        }

        public async Task Add(Person person)
        {
            await httpClient.SendJsonAsync(HttpMethod.Post, "/api/people", person);
        }

        public async Task Delete(int Id)
        {
            await httpClient.SendJsonAsync(HttpMethod.Delete, "/api/people", Id);
        }

        public async Task<Person> Get(int Id)
        {
            return await httpClient.GetJsonAsync<Person>($"/api/{Id}");
        }
    }
}
