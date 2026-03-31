using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMO.API.Data;
using PMO.API.DTOs;
using PMO.API.Models;

namespace PMO.API.Controllers;

// ?? Risks ?????????????????????????????????????????????????
[ApiController]
[Route("api/projects/risks")]
[Authorize]
public class RisksController : ControllerBase
{
    private readonly AppDbContext _db;
    public RisksController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? projectId, [FromQuery] string? status)
    {
        var q = _db.Risks.Include(r => r.Owner).AsQueryable();
        if (projectId.HasValue) q = q.Where(r => r.ProjectId == projectId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);

        var result = await q.Select(r => new RiskDto(
            r.Id, r.ProjectId, r.Title, r.Description,
            r.Level, r.Probability, r.Impact, r.Response, r.Status,
            r.MitigationPlan, r.Owner != null ? r.Owner.FullNameAr : null,
            r.DueDate, r.CreatedAt)).ToListAsync();

        return Ok(new { count = result.Count, results = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var r = await _db.Risks.Include(r => r.Owner).FirstOrDefaultAsync(r => r.Id == id);
        if (r == null) return NotFound();
        return Ok(new RiskDto(r.Id, r.ProjectId, r.Title, r.Description,
            r.Level, r.Probability, r.Impact, r.Response, r.Status,
            r.MitigationPlan, r.Owner?.FullNameAr, r.DueDate, r.CreatedAt));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRiskRequest req)
    {
        var risk = new Risk
        {
            ProjectId      = req.ProjectId,
            Title          = req.Title,
            Description    = req.Description,
            Level          = req.Level,
            Probability    = req.Probability,
            Impact         = req.Impact,
            Response       = req.Response,
            MitigationPlan = req.MitigationPlan,
            OwnerId        = req.OwnerId,
            DueDate        = req.DueDate,
        };
        _db.Risks.Add(risk);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = risk.Id }, new { id = risk.Id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRiskRequest req)
    {
        var r = await _db.Risks.FindAsync(id);
        if (r == null) return NotFound();
        if (req.Title          != null) r.Title          = req.Title;
        if (req.Status         != null) r.Status         = req.Status;
        if (req.MitigationPlan != null) r.MitigationPlan = req.MitigationPlan;
        if (req.Level          != null) r.Level          = req.Level;
        if (req.OwnerId        != null) r.OwnerId        = req.OwnerId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.Risks.FindAsync(id);
        if (r == null) return NotFound();
        _db.Risks.Remove(r);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ?? Issues ????????????????????????????????????????????????
[ApiController]
[Route("api/projects/issues")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly AppDbContext _db;
    public IssuesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? projectId, [FromQuery] string? status)
    {
        var q = _db.Issues.Include(i => i.Owner).AsQueryable();
        if (projectId.HasValue) q = q.Where(i => i.ProjectId == projectId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(i => i.Status == status);

        var result = await q.Select(i => new IssueDto(
            i.Id, i.ProjectId, i.Title, i.Description,
            i.Priority, i.Type, i.Status, i.NeedsEscalation, i.Resolution,
            i.Owner != null ? i.Owner.FullNameAr : null,
            i.DueDate, i.CreatedAt)).ToListAsync();

        return Ok(new { count = result.Count, results = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var i = await _db.Issues.Include(i => i.Owner).FirstOrDefaultAsync(i => i.Id == id);
        if (i == null) return NotFound();
        return Ok(new IssueDto(i.Id, i.ProjectId, i.Title, i.Description,
            i.Priority, i.Type, i.Status, i.NeedsEscalation, i.Resolution,
            i.Owner?.FullNameAr, i.DueDate, i.CreatedAt));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIssueRequest req)
    {
        var issue = new Issue
        {
            ProjectId       = req.ProjectId,
            Title           = req.Title,
            Description     = req.Description,
            Priority        = req.Priority,
            Type            = req.Type,
            NeedsEscalation = req.NeedsEscalation,
            OwnerId         = req.OwnerId,
            DueDate         = req.DueDate,
        };
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = issue.Id }, new { id = issue.Id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIssueRequest req)
    {
        var i = await _db.Issues.FindAsync(id);
        if (i == null) return NotFound();
        if (req.Title           != null) i.Title           = req.Title;
        if (req.Status          != null) i.Status          = req.Status;
        if (req.Resolution      != null) i.Resolution      = req.Resolution;
        if (req.NeedsEscalation != null) i.NeedsEscalation = req.NeedsEscalation.Value;
        if (req.OwnerId         != null) i.OwnerId         = req.OwnerId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var i = await _db.Issues.FindAsync(id);
        if (i == null) return NotFound();
        _db.Issues.Remove(i);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ?? Change Requests ???????????????????????????????????????
[ApiController]
[Route("api/workflows/change-requests")]
[Authorize]
public class ChangeRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ChangeRequestsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? projectId, [FromQuery] string? status)
    {
        var q = _db.ChangeRequests.Include(c => c.SubmittedBy).AsQueryable();
        if (projectId.HasValue) q = q.Where(c => c.ProjectId == projectId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(c => c.Status == status);

        var result = await q.Select(c => new ChangeRequestDto(
            c.Id, c.ProjectId, c.Title, c.Category, c.Impact,
            c.Cost, c.Status, c.Priority, c.Justification, c.Approver,
            c.SubmittedBy != null ? c.SubmittedBy.FullNameAr : null,
            c.CreatedAt)).ToListAsync();

        return Ok(new { count = result.Count, results = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await _db.ChangeRequests.Include(c => c.SubmittedBy)
                         .FirstOrDefaultAsync(c => c.Id == id);
        if (c == null) return NotFound();
        return Ok(new ChangeRequestDto(c.Id, c.ProjectId, c.Title, c.Category,
            c.Impact, c.Cost, c.Status, c.Priority, c.Justification, c.Approver,
            c.SubmittedBy?.FullNameAr, c.CreatedAt));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChangeRequestRequest req)
    {
        var cr = new ChangeRequest
        {
            ProjectId     = req.ProjectId,
            Title         = req.Title,
            Category      = req.Category,
            Impact        = req.Impact,
            Cost          = req.Cost,
            Priority      = req.Priority,
            Justification = req.Justification,
            Approver      = req.Approver,
        };
        _db.ChangeRequests.Add(cr);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = cr.Id }, new { id = cr.Id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChangeRequestRequest req)
    {
        var c = await _db.ChangeRequests.FindAsync(id);
        if (c == null) return NotFound();
        c.Status   = req.Status;
        if (req.Approver != null) c.Approver = req.Approver;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.ChangeRequests.FindAsync(id);
        if (c == null) return NotFound();
        _db.ChangeRequests.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ?? Milestones ????????????????????????????????????????????
[ApiController]
[Route("api/projects/milestones")]
[Authorize]
public class MilestonesController : ControllerBase
{
    private readonly AppDbContext _db;
    public MilestonesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? projectId)
    {
        var q = _db.Milestones.AsQueryable();
        if (projectId.HasValue) q = q.Where(m => m.ProjectId == projectId);

        var result = await q.Select(m => new MilestoneDto(
            m.Id, m.ProjectId, m.Title, m.Weight,
            m.Status, m.DueDate, m.CreatedAt)).ToListAsync();

        return Ok(new { count = result.Count, results = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMilestoneRequest req)
    {
        var m = new Milestone
        {
            ProjectId = req.ProjectId,
            Title     = req.Title,
            Weight    = req.Weight,
            DueDate   = req.DueDate,
        };
        _db.Milestones.Add(m);
        await _db.SaveChangesAsync();
        return Created("", new { id = m.Id });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMilestoneRequest req)
    {
        var m = await _db.Milestones.FindAsync(id);
        if (m == null) return NotFound();
        m.Status = req.Status;
        if (req.Status == "completed") m.DueDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
