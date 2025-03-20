using CqrsWithMediatR.Features.Commands;
using CqrsWithMediatR.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/products")]

public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var productId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = productId }, productId);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductCommand command)
    {
        if (id != command.Id) 
        {
            return BadRequest("Product ID mismatch.");
        }

        try 
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _mediator.Send(new GetAllProductsQuery());
        return Ok(products);
    }

    [HttpGet("filter")]
    public async Task<IActionResult> GetProductsByPrice([FromQuery] decimal price, [FromQuery] string operation)
    {
        var products = await _mediator.Send(new GetProductsByPriceQuery(price, operation));
        return Ok(products);
    }
}
