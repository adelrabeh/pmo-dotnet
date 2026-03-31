using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMO.API.Models;
namespace PMO.API.Data;
public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Risk> Risks { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<ChangeRequest> ChangeRequests { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<ProjectStatus> ProjectStatuses { get; set; }
    public DbSet<ProjectPhase> ProjectPhases { get; set; }
    public DbSet<AiInsight> AiInsights { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.UseSerialColumns();
        b.Entity<Project>().HasOne(p => p.Manager).WithMany().HasForeignKey(p => p.ManagerId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Risk>().HasOne(r => r.Project).WithMany(p => p.Risks).HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Issue>().HasOne(i => i.Project).WithMany(p => p.Issues).HasForeignKey(i => i.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ChangeRequest>().HasOne(c => c.Project).WithMany(p => p.ChangeRequests).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Milestone>().HasOne(m => m.Project).WithMany(p => p.Milestones).HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProjectStatus>().HasData(
            new ProjectStatus { Id=1,NameAr="On Track",NameEn="On Track",Color="#38a169" },
            new ProjectStatus { Id=2,NameAr="Delayed",NameEn="Delayed",Color="#ed8936" },
            new ProjectStatus { Id=3,NameAr="Critical",NameEn="Critical",Color="#e53e3e" },
            new ProjectStatus { Id=4,NameAr="Closed",NameEn="Closed",Color="#718096" },
            new ProjectStatus { Id=5,NameAr="On Hold",NameEn="On Hold",Color="#805ad5" });
        b.Entity<ProjectPhase>().HasData(
            new ProjectPhase { Id=1,NameAr="Initiation",NameEn="Initiation",Order=1 },
            new ProjectPhase { Id=2,NameAr="Planning",NameEn="Planning",Order=2 },
            new ProjectPhase { Id=3,NameAr="Execution",NameEn="Execution",Order=3 },
            new ProjectPhase { Id=4,NameAr="Monitoring",NameEn="Monitoring",Order=4 },
            new ProjectPhase { Id=5,NameAr="Closure",NameEn="Closure",Order=5 });
    }
}
