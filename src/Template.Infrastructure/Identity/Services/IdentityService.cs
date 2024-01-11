using System.Security.Claims;
using System.Text;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Template.Application.Identity.Commands.ChangePassword;
using Template.Application.Identity.Commands.CreateUser;
using Template.Application.Identity.Commands.ResetPassword;
using Template.Application.Identity.Commands.VerifyEmail;
using Template.Application.Identity.Common;
using Template.Application.Identity.Interfaces;
using Template.Domain.Common.Models;
using Template.Domain.Identity.Constants.Errors;
using Template.Domain.Identity.Entites;

namespace Template.Infrastructure.Identity.Services;

public sealed class IdentityService(
    ILogger<IdentityService> logger,
    UserManager<User> userManager,
    IPasswordHasher<User> passwordHasher
) : IIdentityService
{
    private readonly ILogger<IdentityService> _logger = logger;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    public async Task<Result<User>> FindUserAsync(FindUserDto request)
    {
        try
        {
            var result = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (result is null)
                return Result<User>.Failed(ErrorCode.ERR_USER, ErrorMessage.USER_DOES_NOT_EXIST);

            return Result<User>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(FindUserAsync));
            throw;
        }
    }

    public async Task<Result<object>> CreateUserAsync(CreateUserDto request)
    {
        try
        {
            var searchResult = await FindUserAsync(new(request.Email));
            if (searchResult.Succeeded)
                return Result<object>.Failed(ErrorCode.ERR_USER, ErrorMessage.USER_ALREADY_EXISTS);

            var user = CreateUserDto.ToEntity(request);
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            var result = await _userManager.CreateAsync(user).ConfigureAwait(false);
            if (!result.Succeeded)
                return Result<object>.Failed(result.Errors.ToArray());

            return Result<object>.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(CreateUserAsync));
            throw;
        }
    }

    public async Task<Result<string>> GenerateEmailVerificationTokenAsync(string email)
    {
        try
        {
            var searchResult = await FindUserAsync(new(email));
            if (!searchResult.Succeeded)
                return Result<string>.Failed(searchResult.Errors.ToArray());

            var token = await _userManager
                .GenerateEmailConfirmationTokenAsync(searchResult.Body)
                .ConfigureAwait(false);

            var result = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(FindUserAsync));
            throw;
        }
    }

    public async Task<Result<object>> VerifyEmailAsync(VerifyEmailDto request)
    {
        try
        {
            var searchResult = await FindUserAsync(new(request.Email));
            if (!searchResult.Succeeded)
                return Result<object>.Failed(searchResult.Errors.ToArray());

            var token = Encoding.UTF8.GetString(Convert.FromBase64String(request.Token));

            var result = await _userManager
                .ConfirmEmailAsync(searchResult.Body, token)
                .ConfigureAwait(false);

            if (!result.Succeeded)
                return Result<object>.Failed(result.Errors.ToArray());

            var claimsResult = await _userManager
                .AddClaimsAsync(
                    searchResult.Body,
                    [
                        new Claim(JwtClaimTypes.Email, searchResult.Body.Email),
                        new Claim(JwtClaimTypes.GivenName, searchResult.Body.FirstName),
                        new Claim(JwtClaimTypes.FamilyName, searchResult.Body.LastName)
                    ]
                )
                .ConfigureAwait(false);

            if (!claimsResult.Succeeded)
                return Result<object>.Failed(claimsResult.Errors.ToArray());

            return Result<object>.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(VerifyEmailAsync));
            throw;
        }
    }

    public async Task<Result<string>> GenerateResetPasswordTokenAsync(string email)
    {
        try
        {
            var searchResult = await FindUserAsync(new(email));
            if (!searchResult.Succeeded)
                return Result<string>.Failed(searchResult.Errors.ToArray());

            var token = await _userManager
                .GeneratePasswordResetTokenAsync(searchResult.Body)
                .ConfigureAwait(false);

            var result = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(GenerateResetPasswordTokenAsync));
            throw;
        }
    }

    public async Task<Result<object>> ResetPasswordAsync(ResetPasswordDto request)
    {
        try
        {
            var searchResult = await FindUserAsync(new(request.Email));
            if (!searchResult.Succeeded)
                return Result<object>.Failed(searchResult.Errors.ToArray());

            var token = Encoding.UTF8.GetString(Convert.FromBase64String(request.Token));

            var result = await _userManager
                .ResetPasswordAsync(searchResult.Body, token, request.Password)
                .ConfigureAwait(false);

            if (!result.Succeeded)
                return Result<object>.Failed(result.Errors.ToArray());

            return Result<object>.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(ResetPasswordAsync));
            throw;
        }
    }

    public async Task<Result<object>> ChangePasswordAsync(ChangePasswordDto request)
    {
        try
        {
            var searchResult = await FindUserAsync(new(request.Email));
            if (!searchResult.Succeeded)
                return Result<object>.Failed(searchResult.Errors.ToArray());

            var isOldPasswordCorrect = await _userManager
                .CheckPasswordAsync(searchResult.Body, request.OldPassword)
                .ConfigureAwait(false);

            if (!isOldPasswordCorrect)
                return Result<object>.Failed(
                    ErrorCode.ERR_PASSWORD,
                    ErrorMessage.PASSWORD_NOT_MATCHING
                );

            var user = searchResult.Body;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!result.Succeeded)
                return Result<object>.Failed(result.Errors.ToArray());

            return Result<object>.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(ChangePasswordAsync));
            throw;
        }
    }

    public async Task<Result<object>> DeleteUserAsync(FindUserDto request)
    {
        try
        {
            var userSearchResult = await FindUserAsync(new(request.Email));
            if (!userSearchResult.Succeeded)
                return Result<object>.Failed(userSearchResult.Errors.ToArray());

            var user = userSearchResult.Body;

            var result = await _userManager.DeleteAsync(user).ConfigureAwait(false);
            if (!result.Succeeded)
                return Result<object>.Failed(result.Errors.ToArray());

            return Result<object>.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, nameof(DeleteUserAsync));
            throw;
        }
    }
}
