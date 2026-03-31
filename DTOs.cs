namespace PMO.API.DTOs;

// ?? Auth ??????????????????????????????????????????????????
public record LoginRequest(string Username, string Password);
public record RegisterRequest(
    string Username, string Password, string Email,
    string FullNameAr, string FullNameEn,
    string? Department, string? JobTitle);
public record TokenResponse(string Access, string Refresh, UserDto User);
public record RefreshRequest(string Refresh);

public record UserDto(
    int Id, string Username, string Email,
    string FullNameAr, string FullNameEn,
    string? Department, string? JobTitle);

// ?? Project ???????????????????????????????????????????????
public record ProjectListDto(
    int Id, string ProjectCode, string ProjectNameAr, string ProjectNameEn,
    string StatusName, string StatusColor, string PhaseName,
    int ActualProgress, int PlannedProgress,
    decimal Budget, decimal SpentAmount,
    string? ManagerName, string? Department,
    DateTime StartDate, DateTime EndDate,
    int RisksCount, int IssuesCount, int ChangeRequestsCount,
    bool BaselineExists);

public record ProjectDetailDto(
    int Id, string ProjectCode, string ProjectNameAr, string ProjectNameEn,
    string? Description, string StatusName, string StatusColor,
    string PhaseName, int StatusId, int PhaseId,
    int ActualProgress, int PlannedProgress,
    decimal Budget, decimal SpentAmount,
    string? ManagerName, int? ManagerId,
    string? Department, string? Initiative, string? FundingSource,
    string ProjectType, DateTime StartDate, DateTime EndDate,
    bool BaselineExists, DateTime UpdatedAt,
    IEnumerable<RiskDto> Risks,
    IEnumerable<IssueDto> Issues,
    IEnumerable<ChangeRequestDto> ChangeRequests,
    IEnumerable<MilestoneDto> Milestones);

public record CreateProjectRequest(
    string ProjectCode, string ProjectNameAr, string ProjectNameEn,
    string? Description, int StatusId, int PhaseId, int? ManagerId,
    decimal Budget, int PlannedProgress,
    string? Department, string? Initiative, string? FundingSource,
    string ProjectType, DateTime StartDate, DateTime EndDate);

public record UpdateProjectRequest(
    string? ProjectNameAr, string? ProjectNameEn, string? Description,
    int? StatusId, int? PhaseId, int? ManagerId,
    decimal? Budget, decimal? SpentAmount,
    int? PlannedProgress, int? ActualProgress,
    string? Department, bool? BaselineExists);

// ?? Risk ??????????????????????????????????????????????????
public record RiskDto(
    int Id, int ProjectId, string Title, string Description,
    string Level, string Probability, string Impact,
    string Response, string Status, string? MitigationPlan,
    string? OwnerName, DateTime DueDate, DateTime CreatedAt);

public record CreateRiskRequest(
    int ProjectId, string Title, string Description,
    string Level, string Probability, string Impact,
    string Response, string? MitigationPlan,
    int? OwnerId, DateTime DueDate);

public record UpdateRiskRequest(
    string? Title, string? Status, string? MitigationPlan,
    string? Level, int? OwnerId);

// ?? Issue ?????????????????????????????????????????????????
public record IssueDto(
    int Id, int ProjectId, string Title, string Description,
    string Priority, string Type, string Status,
    bool NeedsEscalation, string? Resolution,
    string? OwnerName, DateTime DueDate, DateTime CreatedAt);

public record CreateIssueRequest(
    int ProjectId, string Title, string Description,
    string Priority, string Type, bool NeedsEscalation,
    int? OwnerId, DateTime DueDate);

public record UpdateIssueRequest(
    string? Title, string? Status, string? Resolution,
    bool? NeedsEscalation, int? OwnerId);

// ?? Change Request ????????????????????????????????????????
public record ChangeRequestDto(
    int Id, int ProjectId, string Title, string Category,
    string Impact, decimal Cost, string Status, string Priority,
    string Justification, string? Approver,
    string? SubmittedByName, DateTime CreatedAt);

public record CreateChangeRequestRequest(
    int ProjectId, string Title, string Category,
    string Impact, decimal Cost, string Priority,
    string Justification, string? Approver);

public record UpdateChangeRequestRequest(string Status, string? Approver);

// ?? Milestone ?????????????????????????????????????????????
public record MilestoneDto(
    int Id, int ProjectId, string Title, int Weight,
    string Status, DateTime DueDate, DateTime CreatedAt);

public record CreateMilestoneRequest(
    int ProjectId, string Title, int Weight, DateTime DueDate);

public record UpdateMilestoneRequest(string Status);

// ?? AI Insight ????????????????????????????????????????????
public record AiInsightDto(
    int Id, int? ProjectId, string? ProjectName,
    string Level, string Message, bool IsRead, DateTime CreatedAt);

// ?? Dashboard ?????????????????????????????????????????????
public record DashboardStatsDto(
    int TotalProjects, int OpenRisks, int HighRisks,
    int OpenIssues, int EscalatedIssues,
    int PendingChanges, decimal TotalBudget, decimal TotalSpent,
    IEnumerable<AiInsightDto> Insights,
    IEnumerable<StatusDistributionDto> StatusDistribution);

public record StatusDistributionDto(string Status, string Color, int Count);

// ?? Compliance ????????????????????????????????????????????
public record ComplianceDto(
    string Label, int Score, int MaxScore, string Trend);
