using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Service;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IActorService _service;

        public ValuesController(IActorService service)
        {
            _service = service;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var res = await _service.Get(4, 5, "FOO");

            var foo = res.Result.ToString();

            return new string[] { "value1", "value2", foo };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task PostAsync([FromBody] Input input)
        {
            var res = await _service.Get(int.Parse(input.Value1), int.Parse(input.Value2), input.Operation);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class Input
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Operation { get; set; }
    }
}