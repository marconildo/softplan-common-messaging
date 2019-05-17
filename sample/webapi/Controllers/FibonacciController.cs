using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Softplan.Common.Messaging.Abstractions;
using rpcExample;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FibonacciController : ControllerBase
    {
        private readonly IPublisher _publisher;
        public FibonacciController(IPublisher publisher)
        {
            this._publisher = publisher;
        }

        // GET api/values5
        [HttpGet("{number}")]
        public async Task<ActionResult<int>> Get(int number)
        {
            var response = await _publisher.PublishAndWait<FibMessage>(new FibMessage { Number = number }, "test.fibonacci");
            return Ok(response.Number);

        }
    }
}
