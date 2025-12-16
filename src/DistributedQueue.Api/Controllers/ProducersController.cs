using DistributedQueue.Api.DTOs;
using DistributedQueue.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedQueue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProducersController : ControllerBase
{
    private readonly IProducerManager _producerManager;
    private readonly ILogger<ProducersController> _logger;

    public ProducersController(IProducerManager producerManager, ILogger<ProducersController> logger)
    {
        _producerManager = producerManager;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult CreateProducer([FromBody] CreateProducerRequest request)
    {
        try
        {
            var producer = _producerManager.CreateProducer(request.ProducerId, request.Name);
            _logger.LogInformation("Producer '{ProducerId}' created", request.ProducerId);
            return Ok(new { producer.Id, producer.Name, producer.CreatedAt });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetAllProducers()
    {
        var producers = _producerManager.GetAllProducers()
            .Select(p => new { p.Id, p.Name, p.CreatedAt });
        return Ok(producers);
    }

    [HttpGet("{producerId}")]
    public IActionResult GetProducer(string producerId)
    {
        var producer = _producerManager.GetProducer(producerId);
        if (producer == null)
        {
            return NotFound(new { error = $"Producer '{producerId}' not found" });
        }

        return Ok(new { producer.Id, producer.Name, producer.CreatedAt });
    }

    [HttpDelete("{producerId}")]
    public IActionResult DeleteProducer(string producerId)
    {
        var deleted = _producerManager.DeleteProducer(producerId);
        if (!deleted)
        {
            return NotFound(new { error = $"Producer '{producerId}' not found" });
        }

        _logger.LogInformation("Producer '{ProducerId}' deleted", producerId);
        return Ok(new { message = $"Producer '{producerId}' deleted" });
    }
}
