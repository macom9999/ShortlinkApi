using Microsoft.EntityFrameworkCore;
using ShortlinkApi.Data;
using ShortlinkApi.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Konfig aus Umgebungsvariablen (wir nutzen PG_CONN)
var connString = builder.Configuration["PG_CONN"]
                 ?? Environment.GetEnvironmentVariable("PG_CONN")
                 ?? throw new Exception("DB conn string missing");
var ipSalt = builder.Configuration["IP_SALT"] 
             ?? Environment.GetEnvironmentVariable("IP_SALT") 
             ?? "change-me";
var baseUrl = builder.Configuration["BASE_URL"] 
              ?? Environment.GetEnvironmentVariable("BASE_URL") 
              ?? "http://localhost:5000";

// Services
builder.Services.AddDbContext<AppDb>(o => o.UseNpgsql(connString));
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("chrome-extension://*")
     .AllowAnyHeader().AllowAnyMethod()
     .SetIsOriginAllowed(_ => true)
));

var app = builder.Build();
app.UseCors();

app.MapGet("/", () => Results.Ok(new { ok = true, name = "Shortlink API" }));

string MakeCode()
{
    var bytes = RandomNumberGenerator.GetBytes(5);
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var sb = new StringBuilder();
    foreach (var b in bytes) sb.Append(alphabet[b % alphabet.Length]);
    return sb.ToString();
}

// POST /links -> { code, short_url }
app.MapPost("/links", async (AppDb db, HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    var json = JsonDocument.Parse(body).RootElement;

    var longUrl = json.TryGetProperty("long_url", out var l) ? l.GetString() : null;
    var title = json.TryGetProperty("title", out var t) ? t.GetString() : null;
    if (string.IsNullOrWhiteSpace(longUrl)) return Results.BadRequest(new { error = "long_url required" });

    var code = MakeCode();
    var link = new Link { Code = code, LongUrl = longUrl!, Title = title };
    db.Links.Add(link);
    await db.SaveChangesAsync();

    return Results.Ok(new { code, short_url = $"{baseUrl}/r/{code}" });
});

// GET /r/{code} -> Redirect + Klick loggen
app.MapGet("/r/{code}", async (string code, AppDb db, HttpRequest req) =>
{
    var link = await db.Links.FirstOrDefaultAsync(l => l.Code == code);
    if (link is null) return Results.NotFound("not found");

    var ip = (req.Headers["X-Forwarded-For"].FirstOrDefault()
              ?? req.HttpContext.Connection.RemoteIpAddress?.ToString()
              ?? "");
    var ipHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(ip + ipSalt)));
    var ua = req.Headers.UserAgent.ToString();
    var referer = req.Headers.Referer.ToString();

    db.Clicks.Add(new Click { LinkId = link.Id, IpHash = ipHash, Ua = ua, Referer = referer });
    await db.SaveChangesAsync();

    return Results.Redirect(link.LongUrl, permanent: false);
});

// GET /stats/{code}
app.MapGet("/stats/{code}", async (string code, AppDb db) =>
{
    var link = await db.Links.FirstOrDefaultAsync(l => l.Code == code);
    if (link is null) return Results.NotFound(new { error = "not found" });

    var total = await db.Clicks.CountAsync(c => c.LinkId == link.Id);
    return Results.Ok(new { code, long_url = link.LongUrl, total_clicks = total });
});

app.Run();
