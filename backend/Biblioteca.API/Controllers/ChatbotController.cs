using Biblioteca.API.Extensions;
using Biblioteca.Application.DTOs.Chatbot;
using Biblioteca.Application.Features.Chatbot;
using Biblioteca.Application.Interfaces.Chatbot;
using Biblioteca.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.API.Controllers;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route(ChatbotFeature.Route)]
public sealed class ChatbotController(
    IChatbotProvider chatbotProvider,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthPolicies.Authenticated)]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChatResponse>> PostAsync(
        [FromBody] ChatRequest request,
        CancellationToken ct)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return Unauthorized();

        var response = await chatbotProvider.AskAsync(request.Message, userId, ct);
        return Ok(response);
    }
}
