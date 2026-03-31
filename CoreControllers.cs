using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMO.API.Data;
using PMO.API.DTOs;
using PMO.API.Models;

namespace PMO.API.Controllers;

// ?? Portfolio Dashboard ???????????????????????????????????
[ApiController]
[Route("api/core")]
[Authorize]
public class CoreController : ControllerBase
{
    private readonly AppDbContext _db;
    public CoreController(AppDbContext db) => _db = db;

    // GET /api/core/dashboard/
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var projects = await _db.Projects
            .Include(p => p.Status)
            .Include(p => p.Risks)
            .Include(p => p.Issues)
            .Include(p => p.ChangeRequests)
            .ToListAsync();

        var insights = await _db.AiInsights
            .Where(a => !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Include(a => a.Project)
            .Select(a => new AiInsightDto(
                a.Id, a.ProjectId,
                a.Project != null ? a.Project.ProjectNameAr : null,
                a.Level, a.Message, a.IsRead, a.CreatedAt))
            .ToListAsync();

        var statusDist = projects
            .GroupBy(p => p.Status!.NameAr)
            .Select(g => new StatusDistributionDto(
                g.Key,
                g.First().Status!.Color,
                g.Count()))
            .ToList();

        return Ok(new DashboardStatsDto(
            TotalProjects:    projects.Count,
            OpenRisks:        projects.Sum(p => p.Risks.Count(r => r.Status != "closed")),
            HighRisks:        projects.Sum(p => p.Risks.Count(r => r.Level == "high")),
            OpenIssues:       projects.Sum(p => p.Issues.Count(i => i.Status != "closed")),
            EscalatedIssues:  projects.Sum(p => p.Issues.Count(i => i.NeedsEscalation)),
            PendingChanges:   projects.Sum(p => p.ChangeRequests.Count(c => c.Status == "pending")),
            TotalBudget:      projects.Sum(p => p.Budget),
            TotalSpent:       projects.Sum(p => p.SpentAmount),
            Insights:         insights,
            StatusDistribution: statusDist));
    }

    // GET /api/core/statuses/
    [HttpGet("statuses")]
    public async Task<IActionResult> Statuses()
        => Ok(await _db.ProjectStatuses.ToListAsync());

    // GET /api/core/phases/
    [HttpGet("phases")]
    public async Task<IActionResult> Phases()
        => Ok(await _db.ProjectPhases.ToListAsync());
}

// ?? AI Insights ???????????????????????????????????????????
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiInsightsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AiInsightsController(AppDbContext db) => _db = db;

    // GET /api/ai/insights/
    [HttpGet("insights")]
    public async Task<IActionResult> List([FromQuery] bool? unreadOnly)
    {
        var q = _db.AiInsights.Include(a => a.Project).AsQueryable();
        if (unreadOnly == true) q = q.Where(a => !a.IsRead);

        var result = await q
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AiInsightDto(
                a.Id, a.ProjectId,
                a.Project != null ? a.Project.ProjectNameAr : null,
                a.Level, a.Message, a.IsRead, a.CreatedAt))
            .ToListAsync();

        return Ok(new { count = result.Count, results = result });
    }

    // POST /api/ai/insights/{id}/read/
    [HttpPost("insights/{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var insight = await _db.AiInsights.FindAsync(id);
        if (insight == null) return NotFound();
        insight.IsRead = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/ai/generate/{projectId}/
    [HttpPost("generate/{projectId}")]
    public async Task<IActionResult> Generate(int projectId)
    {
        var p = await _db.Projects
            .Include(p => p.Risks)
            .Include(p => p.Issues)
            .Include(p => p.ChangeRequests)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (p == null) return NotFound();

        var messages = new List<(string msg, string level)>();

        if (p.Issues.Count(i => i.Status == "open") >= 3)
            messages.Add(($"{p.ProjectNameAr}: {p.Issues.Count(i => i.Status == "open")} ?????? open? ?????? ????????.", "danger"));

        if (p.Risks.Count(r => r.Level == "high") > 0)
            messages.Add(($"{p.ProjectNameAr}: {p.Risks.Count(r => r.Level == "high")} ????? ????? open?.", "danger"));

        if (p.ActualProgress < p.PlannedProgress - 15)
            messages.Add(($"{p.ProjectNameAr}: Actual progress ({p.ActualProgress}%) below planned ({p.PlannedProgress}%).", "warning"));

        if (p.ChangeRequests.Count(c => c.Status == "pending") >= 2)
            messages.Add(($"{p.ProjectNameAr}: {p.ChangeRequests.Count(c => c.Status == "pending")} ????? ????? pending.", "warning"));

        if (p.ActualProgress == 100)
            messages.Add(($"{p.ProjectNameAr}: Ready for closure ? document lessons learned.", "info"));

        foreach (var (msg, level) in messages)
        {
            _db.AiInsights.Add(new AiInsight
            {
                ProjectId = projectId,
                Message   = msg,
                Level     = level,
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { generated = messages.Count });
    }
}

// ?? Users ?????????????????????????????????????????????????
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    // GET /api/users/me/
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user   = await _db.Users.FindAsync(int.Parse(userId!));
        if (user == null) return NotFound();
        return Ok(new UserDto(user.Id, user.UserName!, user.Email!,
            user.FullNameAr, user.FullNameEn, user.Department, user.JobTitle));
    }

    // GET /api/users/
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var users = await _db.Users
            .Where(u => u.IsActive)
            .Select(u => new UserDto(u.Id, u.UserName!, u.Email!,
                u.FullNameAr, u.FullNameEn, u.Department, u.JobTitle))
            .ToListAsync();
        return Ok(users);
    }
}
