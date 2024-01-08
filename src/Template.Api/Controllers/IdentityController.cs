using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Identity.Commands.ChangePassword;
using Template.Application.Identity.Commands.CreateUser;
using Template.Application.Identity.Commands.DeleteUser;
using Template.Application.Identity.Commands.ForgotPassword;
using Template.Application.Identity.Commands.ResendConfirmationEmail;
using Template.Application.Identity.Commands.ResetPassword;
using Template.Application.Identity.Commands.VerifyEmail;
using Template.Domain.Common.Models;

namespace Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class IdentityController(ILogger<IdentityController> logger, IMediator mediator)
    : ControllerBase
{
    private readonly ILogger<IdentityController> _logger = logger;
    private readonly IMediator _mediator = mediator;

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<object>>> CreateUserAsnyc(
        [FromBody] RegisterUserCommand request
    )
    {
        try
        {
            var result = await _mediator.Send(request);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(CreateUserAsnyc));
            throw;
        }
    }

    [AllowAnonymous]
    [HttpPost("verify")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<object>>> VerifyEmailAsnyc(
        [FromBody] VerifyEmailCommand request
    )
    {
        try
        {
            var result = await _mediator.Send(request);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(VerifyEmailAsnyc));
            throw;
        }
    }

    [AllowAnonymous]
    [HttpPost("verify/resend")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<object>>> ResendVerificationEmailAsnyc(
        [FromBody] ResendConfirmationEmailCommand request
    )
    {
        try
        {
            var result = await _mediator.Send(request);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(ResendVerificationEmailAsnyc));
            throw;
        }
    }

    [AllowAnonymous]
    [HttpPost("password/forgot")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<object>>> ForgotPasswordAsync(
        [FromBody] ForgotPasswordCommand request
    )
    {
        try
        {
            var result = await _mediator.Send(request);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(ForgotPasswordAsync));
            throw;
        }
    }

    [AllowAnonymous]
    [HttpPost("password/reset")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<object>>> ResetPasswordAsync(
        [FromBody] ResetPasswordCommand request
    )
    {
        try
        {
            var result = await _mediator.Send(request);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(ResetPasswordAsync));
            throw;
        }
    }

    [Authorize]
    [HttpPut("password/change")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<object>>> ChangetPasswordAsync(
        [FromBody] ChangePasswordCommand request
    )
    {
        try
        {
            request.Email = User.FindFirst(ClaimTypes.Email)?.Value;

            var result = await _mediator.Send(request);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(ChangetPasswordAsync));
            throw;
        }
    }

    [Authorize]
    [HttpDelete("user/delete")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<object>>> DeleteUserAsync(
        [FromBody] DeleteUserCommand request
    )
    {
        try
        {
            request.Email = User.FindFirst(ClaimTypes.Email)?.Value;

            var result = await _mediator.Send(request);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(DeleteUserAsync));
            throw;
        }
    }
}
