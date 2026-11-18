using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using Database.Users.DataObjects;
using Database.Users.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Api.Controllers;
using Service.Users;

namespace Database.Users.Tests;

/// <summary>Integration tests for the calendar subscription HTTP API.</summary>
public class CalendarSubscriptionsApiTest : UsersDbIntegrationTestsBase
{
    /// <summary>Exercises authentication, ownership, validation, claiming, and HTTP logging.</summary>
    [Fact]
    public async Task HttpApiEnforcesAuthenticationOwnershipAndRequiredSettings()
    {
        DbContext.Users.AddRange(
            new UserDo { Id = "owner", Username = "owner" },
            new UserDo { Id = "other", Username = "other" });
        await DbContext.SaveChangesAsync();

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var httpLogs = new HttpLogCollector();
        builder.Logging.AddProvider(httpLogs);
        builder.Services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.All);
        builder.Services.AddDbContext<UsersDbContext>(options => options.UseMySQL(DbContext.Database.GetConnectionString()!));
        builder.Services.AddScoped<ICalendarSubscriptionRepository, SqlCalendarSubscriptionRepository>();
        builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
        builder.Services.AddScoped<CalendarSubscriptionService>();
        builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions, TestBearerHandler>("Bearer", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddControllers().AddApplicationPart(typeof(CalendarSubscriptionsController).Assembly);
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpLogging();
        app.MapControllers();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient { BaseAddress = new Uri(address) };

        const string root = "/v3/CalendarSubscriptions";
        var settings = new { includeDoorsTime = true, includeLpStageTime = false };
        var incomplete = await client.PostAsJsonAsync(root, new { includeDoorsTime = true });
        Assert.Equal(HttpStatusCode.BadRequest, incomplete.StatusCode);
        var nullSetting = await client.PostAsJsonAsync(root, new { includeDoorsTime = (bool?)null, includeLpStageTime = false });
        Assert.Equal(HttpStatusCode.BadRequest, nullSetting.StatusCode);
        var invalid = await SendAsync(HttpMethod.Post, root, "invalid", settings);
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
        var noId = await SendAsync(HttpMethod.Post, root, "no-id", settings);
        Assert.Equal(HttpStatusCode.Unauthorized, noId.StatusCode);
        var missingProfile = await SendAsync(HttpMethod.Post, root, "no-profile", settings);
        Assert.Equal(HttpStatusCode.BadRequest, missingProfile.StatusCode);

        var created = await client.PostAsJsonAsync(root, settings);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Contains("no-store", created.Headers.CacheControl!.ToString());
        var anonymous = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = anonymous.GetProperty("id").GetGuid();
        Assert.Equal(JsonValueKind.Null, anonymous.GetProperty("userId").ValueKind);
        var path = $"{root}/{id}";

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(path, new { includeLpStageTime = true })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync(path, settings)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"{root}/mine")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync($"{path}/claim", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await SendAsync(HttpMethod.Post, $"{path}/claim", "no-profile")).StatusCode);

        var claim = await SendAsync(HttpMethod.Post, $"{path}/claim", "owner");
        Assert.Equal(HttpStatusCode.OK, claim.StatusCode);
        Assert.Equal("owner", (await claim.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetString());
        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Post, $"{path}/claim", "owner")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Post, $"{path}/claim", "other")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await SendAsync(HttpMethod.Get, path, "invalid")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Get, path, "other")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Put, path, "other", settings)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Delete, path, "other")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Get, path, "owner")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await SendAsync(HttpMethod.Put, path, "owner", settings)).StatusCode);

        var ownerList = await SendAsync(HttpMethod.Get, $"{root}/mine", "owner");
        Assert.Single((await ownerList.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
        var otherList = await SendAsync(HttpMethod.Get, $"{root}/mine", "other");
        Assert.Empty((await otherList.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
        var owned = await SendAsync(HttpMethod.Post, root, "owner", settings);
        Assert.Equal("owner", (await owned.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetString());
        var unowned = await client.PostAsJsonAsync(root, settings);
        var unownedId = (await unowned.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{root}/{unownedId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await SendAsync(HttpMethod.Delete, path, "owner")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(HttpMethod.Get, path, "owner")).StatusCode);
        await client.GetAsync("/missing-route");
        Assert.NotEmpty(httpLogs.Messages);
        Assert.DoesNotContain(httpLogs.Messages, message => message.Contains(id.ToString(), StringComparison.OrdinalIgnoreCase));

        async Task<HttpResponseMessage> SendAsync(HttpMethod method, string uri, string token, object? body = null)
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
            if (body is not null) request.Content = JsonContent.Create(body);
            return await client.SendAsync(request);
        }
    }

    /// <summary>Collects HTTP middleware log messages for assertions.</summary>
    private sealed class HttpLogCollector : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => new CollectorLogger(
            categoryName == "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware" ? Messages : null);
        public void Dispose() { }

        private sealed class CollectorLogger(ConcurrentQueue<string>? messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => messages is not null;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                messages?.Enqueue(formatter(state, exception));
            }
        }
    }

    /// <summary>Authenticates test requests using their bearer token as the user ID.</summary>
    private sealed class TestBearerHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorization = Request.Headers.Authorization.ToString();
            if (authorization.Length == 0) return Task.FromResult(AuthenticateResult.NoResult());
            if (!authorization.StartsWith("Bearer ") || authorization == "Bearer invalid")
                return Task.FromResult(AuthenticateResult.Fail("Invalid token"));

            var token = authorization["Bearer ".Length..];
            var claims = token == "no-id" ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, token)];
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }
}
