using System.Security.Claims;
using backend.Features.Feedbacks.Application.DTOs;
using backend.Features.Feedbacks.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Feedbacks.Presentation.Controllers;

public class FeedbacksController(IFeedbackService feedbackService) : ApiControllerBase
{
    [HttpGet]
    [SkipStatusCodePages] // Exemplo: se quiser permitir acesso público a listagem de feedbacks
    public async Task<ActionResult<IEnumerable<FeedbackResponseDTO>>> GetFeedbacks()
    {
        var results = await feedbackService.GetFeedbacksAsync();
        return Ok(results);
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackResponseDTO>> SubmitFeedback(FeedbackCreateRequestDTO dto)
    {
        var userId = GetUserId();
        var result = await feedbackService.SubmitFeedbackAsync(userId, dto);
        return CreatedAtAction(nameof(GetFeedbacks), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<FeedbackResponseDTO>> UpdateFeedback(Guid id, FeedbackUpdateRequestDTO dto)
    {
        var userId = GetUserId();
        var result = await feedbackService.UpdateFeedbackAsync(id, userId, dto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFeedback(Guid id)
    {
        var userId = GetUserId();
        await feedbackService.DeleteFeedbackAsync(id, userId);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(id!);
    }
}