using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Configs;
using Application;
using Application.Contacts;
using Application.Models;
using Domain.Enums;
using Identity;
using Persistance;
using Persistance.DbContext;
using Persistance.Static;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters
        .Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddJwtSecurity(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddIdentityServices();
builder.Services.AddSwaggerConfig(); 
builder.Services.AddCorsConfig();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.InitializeDbObjectsAsync(db);
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

var auth = app.MapGroup("/api/auth").WithTags("Auth");

auth.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
{
    try
    {
        var result = await authService.RegisterAsync(request);
        return Results.Created("/api/auth/register", result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
})
.WithName("Register")
.WithOpenApi()
.AllowAnonymous();

auth.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    try
    {
        var result = await authService.LoginAsync(request);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("Login")
.WithOpenApi()
.AllowAnonymous();

static Guid GetUserId(HttpContext ctx) =>
    Guid.Parse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

static bool IsAdmin(HttpContext ctx) =>
    ctx.User.IsInRole(nameof(UserRole.Admin));

var categories = app.MapGroup("/api/categories")
    .WithTags("Categories")
    .RequireAuthorization();

categories.MapGet("/", async (ICategoryService svc) =>
    Results.Ok(await svc.GetAllAsync()));

categories.MapPost("/", async (CreateCategoryRequest request, ICategoryService svc) =>
{
    try
    {
        var result = await svc.CreateAsync(request);
        return Results.Created($"/api/categories/{result.Id}", result);
    }
    catch (ArgumentException e) { return Results.BadRequest(new { e.Message }); }
})
.RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

categories.MapDelete("/{id:guid}", async (Guid id, ICategoryService svc) =>
{
    try
    {
        await svc.DeleteAsync(id);
        return Results.NoContent();
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (InvalidOperationException e) { return Results.BadRequest(new { e.Message }); }
})
.RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

var users = app.MapGroup("/api/users")
    .WithTags("Users")
    .RequireAuthorization();

users.MapGet("/me", async (HttpContext ctx, IUserService svc) =>
{
    try
    {
        var result = await svc.GetProfileAsync(GetUserId(ctx));
        return Results.Ok(result);
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
});

users.MapPut("/me", async (UpdateProfileRequest request, HttpContext ctx, IUserService svc) =>
{
    try
    {
        await svc.UpdateProfileAsync(GetUserId(ctx), request);
        return Results.NoContent();
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (ArgumentException e) { return Results.BadRequest(new { e.Message }); }
});

var adminUsers = app.MapGroup("/api/admin/users")
    .WithTags("Admin - Users")
    .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

adminUsers.MapGet("/", async (IUserService svc) =>
    Results.Ok(await svc.GetAllUsersAsync()));

adminUsers.MapPatch("/{id:guid}/role", async (Guid id, ChangeRoleRequest request, IUserService svc) =>
{
    try
    {
        await svc.ChangeUserRoleAsync(id, request.Role);
        return Results.NoContent();
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (ArgumentException e) { return Results.BadRequest(new { e.Message }); }
});

adminUsers.MapDelete("/{id:guid}", async (Guid id, HttpContext ctx, IUserService svc) =>
{
    try
    {
        if (GetUserId(ctx) == id)
            return Results.BadRequest(new { Message = "You cannot delete your own account." });

        await svc.DeleteUserAsync(id);
        return Results.NoContent();
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
});


var tests = app.MapGroup("/api/tests")
    .WithTags("Tests")
    .RequireAuthorization();

tests.MapGet("/", async (ITestService svc) =>
    Results.Ok(await svc.GetAllTestsAsync()));

tests.MapGet("/my", async (HttpContext ctx, ITestService svc) =>
    Results.Ok(await svc.GetTestsByUserAsync(GetUserId(ctx))));

tests.MapGet("/category/{categoryId:guid}", async (Guid categoryId, ITestService svc) =>
    Results.Ok(await svc.GetByCategoryIdAsync(categoryId)));

tests.MapGet("/{id:guid}", async (Guid id, HttpContext ctx, ITestService svc) =>
{
    var preview = await svc.GetTestByIdAsync(id, isOwnerOrAdmin: false);
    if (preview is null) return Results.NotFound();

    var isOwnerOrAdmin = IsAdmin(ctx) || preview.CreatedByUserId == GetUserId(ctx);

    var result = isOwnerOrAdmin
        ? await svc.GetTestByIdAsync(id, isOwnerOrAdmin: true)
        : preview;

    return Results.Ok(result);
});
tests.MapPost("/", async (CreateTestRequest request, HttpContext ctx, ITestService svc) =>
{
    try
    {
        var result = await svc.CreateTestAsync(GetUserId(ctx), request);
        return Results.Created($"/api/tests/{result.Id}", result);
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (ArgumentException e) { return Results.BadRequest(new { e.Message }); }
});

tests.MapPut("/{id:guid}", async (Guid id, CreateTestRequest request, HttpContext ctx, ITestService svc) =>
{
    try
    {
        await svc.UpdateTestAsync(id, GetUserId(ctx), request);
        return Results.NoContent();
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (UnauthorizedAccessException) { return Results.Forbid(); }
    catch (ArgumentException e) { return Results.BadRequest(new { e.Message }); }
});

tests.MapDelete("/{id:guid}", async (Guid id, HttpContext ctx, ITestService svc) =>
{
    try
    {
        await svc.DeleteTestAsync(id, GetUserId(ctx));
        return Results.NoContent();
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (UnauthorizedAccessException) { return Results.Forbid(); }
});

var attempts = app.MapGroup("/api/attempts")
    .WithTags("Attempts")
    .RequireAuthorization();

attempts.MapGet("/my", async (HttpContext ctx, ITestAttemptService svc) =>
    Results.Ok(await svc.GetUserAttemptsAsync(GetUserId(ctx))));

attempts.MapGet("/{attemptId:guid}/result", async (Guid attemptId, HttpContext ctx, ITestAttemptService svc) =>
{
    try
    {
        var result = await svc.GetAttemptResultAsync(GetUserId(ctx), attemptId);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
});

attempts.MapPost("/start/{testId:guid}", async (Guid testId, HttpContext ctx, ITestAttemptService svc) =>
{
    try
    {
        var result = await svc.StartAttemptAsync(GetUserId(ctx), testId);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (InvalidOperationException e) { return Results.BadRequest(new { e.Message }); }
});

attempts.MapPost("/{attemptId:guid}/submit", async (
    Guid attemptId,
    SubmitAttemptRequest request,
    HttpContext ctx,
    ITestAttemptService svc) =>
{
    try
    {
        var result = await svc.SubmitAttemptAsync(GetUserId(ctx), attemptId, request);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    catch (InvalidOperationException e) { return Results.BadRequest(new { e.Message }); }
    catch (UnauthorizedAccessException) { return Results.Forbid(); }
});

attempts.MapGet("/test/{testId:guid}", async (Guid testId, HttpContext ctx, ITestAttemptService svc) =>
    Results.Ok(await svc.GetAttemptsByTestIdAsync(testId)))
    .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

var reports = app.MapGroup("/api/reports")
    .WithTags("Reports")
    .RequireAuthorization(p => p.RequireRole(nameof(UserRole.Admin)));

reports.MapGet("/users/{userId:guid}/passrate",
    async (Guid userId, IReportService svc) =>
    {
        try
        {
            return Results.Ok(await svc.GetUserPassRateAsync(userId));
        }
        catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    });

reports.MapGet("/tests/underperforming",
    async (IReportService svc) =>
        Results.Ok(await svc.GetUnderperformingTestsAsync()));

reports.MapGet("/tests/{testId:guid}/leaderboard",
    async (Guid testId, IReportService svc) =>
    {
        try
        {
            return Results.Ok(await svc.GetTestLeaderboardAsync(testId));
        }
        catch (KeyNotFoundException e) { return Results.NotFound(new { e.Message }); }
    });

app.MapFallbackToFile("index.html");
app.Run();