using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using rpcExample;
using Softplan.Common.Messaging.RabbitMq.Abstractions;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FibonacciController : ControllerBase
    {
        private readonly IPublisher _publisher;
        
        private const string TestFibonacci = "test.fibonacci";
        
        public FibonacciController(IPublisher publisher)
        {
            _publisher = publisher;
        }

        // GET api/values5
        [HttpGet("{number}")]
        public async Task<ActionResult<int>> Get(int number)
        {
            try
            {
                var response =
                    await _publisher.PublishAndWait<FibMessage>(new FibMessage {Number = number}, TestFibonacci);
                return Ok(response.Number);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
