using System.ComponentModel.DataAnnotations;
using FleetFlow.Api.Authentication;
using FleetFlow.Domain;
using FleetFlow.Infrastructure.Authentication;
using FleetFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    FleetFlowDbContext dbContext,
    AccessTokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>> Register(
        RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var tenant = new Tenant(request.CompanyName);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = email,
            Email = email, TenantId = tenant.Id
        };

        // Keep tenant creation and account creation atomic, including validation failures.
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);
        dbContext.Tenants.Add(tenant);

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        await transaction.CommitAsync(cancellationToken);

        // Signup never grants the platform Administrator role.
        return StatusCode(StatusCodes.Status201Created,
            new RegistrationResponse(user.Id, tenant.Id, user.Email!));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return InvalidCredentials();
        }

        var result = await signInManager.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return InvalidCredentials();
        }

        // Derive tenant and roles from the stored account, never from login input.
        var roles = await userManager.GetRolesAsync(user);
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";
        return Ok(tokens.Create(user, roles));
    }

    private ObjectResult InvalidCredentials() => Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Invalid email or password.");
}

public sealed record RegisterRequest(
    [Required, StringLength(200)] string CompanyName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128, MinimumLength = 12)] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password);

public sealed record RegistrationResponse(Guid UserId, Guid TenantId, string Email);
