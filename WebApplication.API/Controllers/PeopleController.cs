using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;


        private readonly ILogger<PeopleController> _logger;
        //private readonly IMetrics _metrics;

        //private readonly CounterOptions _counterOptions = new CounterOptions {
        //    MeasurementUnit = Unit.Calls,
        //    Name = "Home Counter",
        //    ResetOnReporting = true
        //};

        public PeopleController(ApplicationDbContext context,
            ILogger<PeopleController> logger
        //    IMetrics metrics
            ) {
            _context = context;
            _logger = logger;
            //      _metrics = metrics;
        }


        //[HttpGet("increment")]
        //public IActionResult Increment(string tag = null) {
        //    var tags = new MetricTags("userTag", string.IsNullOrEmpty(tag) ? "undefined" : tag);
        //    _metrics.Measure.Counter.Increment(_counterOptions, tags);
        //    return Ok("done");
        //}

        [HttpGet("exception")]
        public IActionResult Exception() {
            throw new ArgumentNullException();
        }

        [HttpGet("regression")]
        public async Task<IActionResult> regression([FromQuery]int sec) {
            await Task.Delay(TimeSpan.FromSeconds(sec));
            return Ok();
        }

        // GET: api/People
        [HttpGet]       
        public async Task<ActionResult<IEnumerable<Person>>> GetPeople() {
            return await _context.People.ToListAsync();
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id) {
            var person = await _context.People.FindAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            return person;
        }

        // PUT: api/People/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, Person person) {
            if (id != person.Id)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/People
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person) {
            _context.People.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerson", new { id = person.Id }, person);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Person>> DeletePerson(int id) {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.People.Remove(person);
            await _context.SaveChangesAsync();

            return person;
        }

        private bool PersonExists(int id) {
            return _context.People.Any(e => e.Id == id);
        }
    }
}
