using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRestService.Models;

namespace MovieRestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RootsController : ControllerBase
    {
        private readonly MovieContext _context;
        private readonly IConfigurationRoot _config;

        string apiKey = string.Empty;
        JsonSerializerOptions options;

        public RootsController(MovieContext context, IConfigurationRoot config)
        {
            _context = context;
            _config = config;

            apiKey = _config["ApiKey"];

            options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new JsonStringEnumConverter());
        }

        // GET: api/Roots
        [HttpGet]

        //http://www.omdbapi.com/?t=[userData]&apikey=e63e029e

        public async Task<ActionResult<IEnumerable<Root>>> GetRoot()
        {
            if (_context.Root == null)
            {
                return NotFound();
            }

            return await _context.Root.ToListAsync();
        }

        // GET: api/Roots/5
        [HttpGet("{data}")]
        public async Task<ActionResult<List<Root>>> GetRoot(string data = "")
        {
            if (_context.Root == null)
            {
                return NotFound();
            }
            string url = "http://www.omdbapi.com/?t=" + data + "&apikey=" + apiKey;
            var root = _context.Root.Where(m => m.Title.Contains(data)).FirstOrDefault();

            if (root == null)
            {
                var root_list = _context.Root.Where(m => m.Director.Contains(data)).ToList();
                if (root_list.Count != 0)
                {
                    return root_list;
                }
                root_list = _context.Root.Where(m => m.Actors.Contains(data)).ToList();
                if (root_list.Count != 0)
                {
                    return root_list;
                }

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        root = JsonSerializer.Deserialize<Root>(result, options);
                        _context.Root.Add(root);
                        _context.SaveChanges();
                        var recent = new Recent()
                        {
                            RootId = root.Id,
                            CreatedAt = DateTime.Now.ToString()
                        };
                        _context.Recent.Add(recent);
                        _context.SaveChanges();
                    }
                }
            }
            return new List<Root>() { root };
        }

        // PUT: api/Roots/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoot(int id, Root root)
        {
            if (id != root.Id)
            {
                return BadRequest();
            }

            _context.Entry(root).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RootExists(id))
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

        // POST: api/Roots
        [HttpPost]
        public async Task<ActionResult<Root>> PostRoot(Root root)
        {
            if (_context.Root == null)
            {
                return Problem("Entity set 'MovieContext.Root'  is null.");
            }
            _context.Root.Add(root);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoot", new { id = root.Id }, root);
        }

        // DELETE: api/Roots/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoot(int id)
        {
            if (_context.Root == null)
            {
                return NotFound();
            }
            var root = await _context.Root.FindAsync(id);
            if (root == null)
            {
                return NotFound();
            }

            _context.Root.Remove(root);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("img/{id}")]
        public ActionResult GetImage(int id)
        {
            var rootFromDb = _context.Root.Find(id);
            if (rootFromDb != null)
            {
                var image = System.IO.File.OpenRead(rootFromDb.Poster);
                return File(image, "image/jpeg");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("new/{id}")]
        public async Task<ActionResult<IEnumerable<Root>>> GetRecent(int id)
        {
            if (_context.Root == null)
            {
                return NotFound();
            }
            var list = _context.Recent.OrderByDescending(r => r.CreatedAt).Take(id);
            var roots = new List<Root>();

            foreach(Recent item in list)
            {
                roots.Add(_context.Root.Find(item.RootId));
            }

            return roots;
        }


        private bool RootExists(int id)
        {
            return (_context.Root?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}