using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMO.API.Data;
using PMO.API.DTOs;
using PMO.API.Models;

namespace PMO.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProjectsController(AppDbContext db) => _db = db;

    // GET /api/projects/
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int? statusId,
        [FromQuery] string? search,
        [FromQuery] string? ordering = "-created_at")
    {
        var q = _db.Projects
            .Include(p => p.Status)
            .Include(p => p.Phase)
            .Include(p => p.Manager)
            .Include(p => p.Risks)
            .Include(p => p.Issues)
            .Include(p => p.ChangeRequests)
            .AsQueryable();

        if (statusId.HasValue)  q = q.Where(p => p.StatusId == statusId);
        if (!string.IsNullOrEmpty(search))
            q = q.Where(p => p.ProjectNameAr.Contains(search) ||
                              p.ProjectNameEn.Contains(search) ||
                              p.ProjectCode.Contains(search));

        q = ordering switch
        {
            "progress"  => q.OrderByDescending(p => p.ActualProgress),
            "-progress" => q.OrderBy(p => p.ActualProgress),
            "budget"    => q.OrderByDescending(p => p.Budget),
            _           => q.OrderByDescending(p => p.CreatedAt),
        };

        var result = await q.Select(p => new ProjectListDto(
            p.Id, p.ProjectCode, p.ProjectNameAr, p.ProjectNameEn,
            p.Status!.NameAr, p.Status.Color, p.Phase!.NameAr,
            p.ActualProgress, p.PlannedProgress,
            p.Budget, p.SpentAmount,
            p.Manager != null ? p.Manager.FullNameAr : null,
            p.Department, p.StartDate, p.EndDate,
            p.Risks.Count, p.Issues.Count, p.ChangeRequests.Count,
            p.BaselineExists)).ToListAsync();

        return Ok(new { count = result.Count, results = result });
    }

    // GET /api/projects/{id}/
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _db.Projects
            .Include(p => p.Status)
            .Include(p => p.Phase)
            .Include(p => p.Manager)
            .Include(p => p.Risks).ThenInclude(r => r.Owner)
            .Include(p => p.Issues).ThenInclude(i => i.Owner)
            .Include(p => p.ChangeRequests).ThenInclude(c => c.SubmittedBy)
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return NotFound();

        return Ok(new ProjectDetailDto(
            p.Id, p.ProjectCode, p.ProjectNameAr, p.ProjectNameEn,
            p.Description, p.Status!.NameAr, p.Status.Color,
            p.Phase!.NameAr, p.StatusId, p.PhaseId,
            p.ActualProgress, p.PlannedProgress,
            p.Budget, p.SpentAmount,
            p.Manager?.FullNameAr, p.ManagerId,
            p.Department, p.Initiative, p.FundingSource,
            p.ProjectType, p.StartDate, p.EndDate,
            p.BaselineExists, p.UpdatedAt,
            p.Risks.Select(r => new RiskDto(r.Id, r.ProjectId, r.Title, r.Description,
                r.Level, r.Probability, r.Impact, r.Response, r.Status,
                r.MitigationPlan, r.Owner?.FullNameAr, r.DueDate, r.CreatedAt)),
            p.Issues.Select(i => new IssueDto(i.Id, i.ProjectId, i.Title, i.Description,
                i.Priority, i.Type, i.Status, i.NeedsEscalation, i.Resolution,
                i.Owner?.FullNameAr, i.DueDate, i.CreatedAt)),
            p.ChangeRequests.Select(c => new ChangeRequestDto(c.Id, c.ProjectId, c.Title,
                c.Category, c.Impact, c.Cost, c.Status, c.Priority,
                c.Justification, c.Approver, c.SubmittedBy?.FullNameAr, c.CreatedAt)),
            p.Milestones.Select(m => new MilestoneDto(m.Id, m.ProjectId, m.Title,
                m.Weight, m.Status, m.DueDate, m.CreatedAt))));
    }

    // GET /api/projects/{id}/dashboard/
    [HttpGet("{id}/dashboard")]
    public async Task<IActionResult> Dashboard(int id)
    {
        var p = await _db.Projects
            .Include(p => p.Status)
            .Include(p => p.Phase)
            .Include(p => p.Risks)
            .Include(p => p.Issues)
            .Include(p => p.ChangeRequests)
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return NotFound();

        var compliance = new List<ComplianceDto>
        {
            new("Project Info",   p.ProjectNameAr.Length > 5 ? 8 : 4, 10, "up"),
            new("Timeline",     p.BaselineExists ? 60 : 0, 60, p.BaselineExists ? "up" : "down"),
            new("Financial KPIs",  p.SpentAmount > 0 ? 3 : 0, 3, p.SpentAmount > 0 ? "up" : "down"),
            new("Project Scope",      p.Description != null ? 2 : 0, 3, "up"),
            new("Risks",           p.Risks.Any() ? 3 : 0, 3, "up"),
            new("Issues",          p.Issues.Any() ? 2 : 0, 3, "up"),
            new("Change Requests",     p.ChangeRequests.Any() ? 0 : 3, 3, "right"),
            new("Stakeholders",     10, 10, "up"),
            new("Documentation",           3, 5, "right"),
        };

        return Ok(new {
            project_name_ar     = p.ProjectNameAr,
            status_name         = p.Status!.NameAr,
            phase_name          = p.Phase!.NameAr,
            actual_progress     = p.ActualProgress,
            planned_progress    = p.PlannedProgress,
            budget              = p.Budget,
            spent_amount        = p.SpentAmount,
            risks_count         = p.Risks.Count,
            high_risks          = p.Risks.Count(r => r.Level == "high"),
            issues_count        = p.Issues.Count,
            escalated_issues    = p.Issues.Count(i => i.NeedsEscalation),
            pending_changes     = p.ChangeRequests.Count(c => c.Status == "pending"),
            milestones_total    = p.Milestones.Count,
            milestones_done     = p.Milestones.Count(m => m.Status == "completed"),
            compliance
        });
    }

    // POST /api/projects/
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req)
    {
        var p = new Project
        {
            ProjectCode   = req.ProjectCode,
            ProjectNameAr = req.ProjectNameAr,
            ProjectNameEn = req.ProjectNameEn,
            Description   = req.Description,
            StatusId      = req.StatusId,
            PhaseId       = req.PhaseId,
            ManagerId     = req.ManagerId,
            Budget        = req.Budget,
            PlannedProgress = req.PlannedProgress,
            Department    = req.Department,
            Initiative    = req.Initiative,
            FundingSource = req.FundingSource,
            ProjectType   = req.ProjectType,
            StartDate     = req.StartDate,
            EndDate       = req.EndDate,
        };
        _db.Projects.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.Id }, new { id = p.Id });
    }

    // PATCH /api/projects/{id}/
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest req)
    {
        var p = await _db.Projects.FindAsync(id);
        if (p == null) return NotFound();

        if (req.ProjectNameAr != null) p.ProjectNameAr   = req.ProjectNameAr;
        if (req.ProjectNameEn != null) p.ProjectNameEn   = req.ProjectNameEn;
        if (req.Description   != null) p.Description     = req.Description;
        if (req.StatusId      != null) p.StatusId        = req.StatusId.Value;
        if (req.PhaseId       != null) p.PhaseId         = req.PhaseId.Value;
        if (req.ManagerId     != null) p.ManagerId       = req.ManagerId;
        if (req.Budget        != null) p.Budget          = req.Budget.Value;
        if (req.SpentAmount   != null) p.SpentAmount     = req.SpentAmount.Value;
        if (req.PlannedProgress != null) p.PlannedProgress = req.PlannedProgress.Value;
        if (req.ActualProgress  != null) p.ActualProgress  = req.ActualProgress.Value;
        if (req.Department    != null) p.Department      = req.Department;
        if (req.BaselineExists != null) p.BaselineExists = req.BaselineExists.Value;
        p.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/projects/{id}/
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Projects.FindAsync(id);
        if (p == null) return NotFound();
        _db.Projects.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
