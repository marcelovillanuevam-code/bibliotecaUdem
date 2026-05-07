using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Interfaces.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/ejemplares")]
public sealed class BookCopiesController(IBookCopyService bookCopyService) : ControllerBase
{
    [HttpGet("~/api/libros/{libroId:guid}/ejemplares")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(IReadOnlyCollection<BookCopyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<BookCopyDto>>> GetByBookIdAsync(
        Guid libroId,
        CancellationToken cancellationToken)
    {
        var copies = await bookCopyService.GetByBookIdAsync(libroId, cancellationToken);
        return Ok(copies);
    }

    [HttpGet("{id:guid}", Name = "GetBookCopyById")]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(BookCopyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookCopyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var copy = await bookCopyService.GetByIdAsync(id, cancellationToken);
        if (copy is null)
            return NotFound();
        return Ok(copy);
    }

    [HttpPost("~/api/libros/{libroId:guid}/ejemplares")]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(BookCopyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookCopyDto>> CreateAsync(
        Guid libroId,
        [FromBody] CreateBookCopyRequest request,
        CancellationToken cancellationToken)
    {
        var copy = await bookCopyService.CreateAsync(libroId, request, cancellationToken);
        return CreatedAtRoute("GetBookCopyById", new { id = copy.Id }, copy);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(typeof(BookCopyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookCopyDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateBookCopyRequest request,
        CancellationToken cancellationToken)
    {
        var copy = await bookCopyService.UpdateAsync(id, request, cancellationToken);
        return Ok(copy);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthPolicies.AdminOrLibrarian)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await bookCopyService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
