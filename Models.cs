using Microsoft.AspNetCore.Identity;

namespace PMO.API.Models;

// ?? User ??????????????????????????????????????????????????
public class AppUser : IdentityUser<int>
{
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle   { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive      { get; set; } = true;
}

// ?? Lookup tables ?????????????????????????????????????????
public class ProjectStatus
{
    public int    Id     { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Color  { get; set; } = "#718096";
}

public class ProjectPhase
{
    public int    Id     { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int    Order  { get; set; }
}

// ?? Project ???????????????????????????????????????????????
public class Project
{
    public int     Id              { get; set; }
    public string  ProjectCode     { get; set; } = string.Empty;
    public string  ProjectNameAr   { get; set; } = string.Empty;
    public string  ProjectNameEn   { get; set; } = string.Empty;
    public string? Description     { get; set; }
    public int     StatusId        { get; set; }
    public int     PhaseId         { get; set; }
    public int?    ManagerId       { get; set; }
    public decimal Budget          { get; set; }
    public decimal SpentAmount     { get; set; }
    public int     PlannedProgress { get; set; }
    public int     ActualProgress  { get; set; }
    public string? Department      { get; set; }
    public string? Initiative      { get; set; }
    public string? FundingSource   { get; set; }
    public string  ProjectType     { get; set; } = "external";
    public DateTime StartDate      { get; set; }
    public DateTime EndDate        { get; set; }
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt      { get; set; } = DateTime.UtcNow;
    public bool BaselineExists     { get; set; } = false;

    // Navigation
    public ProjectStatus?       Status        { get; set; }
    public ProjectPhase?        Phase         { get; set; }
    public AppUser?             Manager       { get; set; }
    public ICollection<Risk>            Risks          { get; set; } = new List<Risk>();
    public ICollection<Issue>           Issues         { get; set; } = new List<Issue>();
    public ICollection<ChangeRequest>   ChangeRequests { get; set; } = new List<ChangeRequest>();
    public ICollection<Milestone>       Milestones     { get; set; } = new List<Milestone>();
}

// ?? Risk ??????????????????????????????????????????????????
public class Risk
{
    public int     Id           { get; set; }
    public int     ProjectId    { get; set; }
    public string  Title        { get; set; } = string.Empty;
    public string  Description  { get; set; } = string.Empty;
    public string  Level        { get; set; } = "medium";   // low / medium / high
    public string  Probability  { get; set; } = "medium";
    public string  Impact       { get; set; } = "medium";
    public string  Response     { get; set; } = "mitigate"; // accept/avoid/transfer/mitigate
    public string  Status       { get; set; } = "open";     // open/in_progress/closed
    public string? MitigationPlan { get; set; }
    public int?    OwnerId      { get; set; }
    public DateTime DueDate     { get; set; }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    public Project?  Project { get; set; }
    public AppUser?  Owner   { get; set; }
}

// ?? Issue (Challenge) ?????????????????????????????????????
public class Issue
{
    public int     Id          { get; set; }
    public int     ProjectId   { get; set; }
    public string  Title       { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public string  Priority    { get; set; } = "medium";  // low/medium/high
    public string  Type        { get; set; } = "technical";
    public string  Status      { get; set; } = "open";
    public bool    NeedsEscalation { get; set; } = false;
    public string? Resolution  { get; set; }
    public int?    OwnerId     { get; set; }
    public DateTime DueDate    { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    public Project?  Project { get; set; }
    public AppUser?  Owner   { get; set; }
}

// ?? Change Request ????????????????????????????????????????
public class ChangeRequest
{
    public int     Id           { get; set; }
    public int     ProjectId    { get; set; }
    public string  Title        { get; set; } = string.Empty;
    public string  Category     { get; set; } = "scope";
    public string  Impact       { get; set; } = "medium";
    public decimal Cost         { get; set; } = 0;
    public string  Status       { get; set; } = "pending"; // pending/under_review/approved/rejected
    public string  Priority     { get; set; } = "medium";
    public string  Justification { get; set; } = string.Empty;
    public string? Approver     { get; set; }
    public int?    SubmittedById { get; set; }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

    public Project?  Project     { get; set; }
    public AppUser?  SubmittedBy { get; set; }
}

// ?? Milestone ?????????????????????????????????????????????
public class Milestone
{
    public int     Id        { get; set; }
    public int     ProjectId { get; set; }
    public string  Title     { get; set; } = string.Empty;
    public int     Weight    { get; set; } = 10;
    public string  Status    { get; set; } = "upcoming"; // completed/delayed/upcoming
    public DateTime DueDate  { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
}

// ?? AI Insight ????????????????????????????????????????????
public class AiInsight
{
    public int     Id        { get; set; }
    public int?    ProjectId { get; set; }
    public string  Level     { get; set; } = "info"; // danger/warning/info
    public string  Message   { get; set; } = string.Empty;
    public bool    IsRead    { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
}

// ?? Workflow Step ?????????????????????????????????????????
public class WorkflowStep
{
    public int     Id        { get; set; }
    public int     ProjectId { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string  Status    { get; set; } = "pending";
    public int     Order     { get; set; }
    public int?    AssignedToId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project?  Project    { get; set; }
    public AppUser?  AssignedTo { get; set; }
}
