using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.DTOs.Projects;
using project_service.Models;
using project_service.Repositories.Interfaces;
using project_service.Services.Interfaces;

namespace project_service.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly ProjectDbContext _db;
    private readonly IKafkaProducerService _kafkaProducer;

    public ProjectService(IProjectRepository projects, ProjectDbContext db, IKafkaProducerService kafkaProducer)
    {
        _projects = projects;
        _db = db;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<IEnumerable<ProjectResponse>> GetMyProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _projects.GetByUserIdAsync(userId, cancellationToken);
        return list.Select(ProjectResponse.FromEntity);
    }

    public async Task<ProjectResponse?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var entity = await _projects.GetByIdAsync(projectId, cancellationToken);
        return entity == null ? null : ProjectResponse.FromEntity(entity);
    }

    public async Task<ProjectResponse> CreateAsync(Guid userId, CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = new Project
            {
                Name = request.Name,
                Description = request.Description,
                CreatedBy = userId
            };

            _projects.CreateAsync(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Publish event after transaction commit (fire-and-forget for performance)
            _ = _kafkaProducer.PublishProjectCreatedAsync(entity.Id, userId, entity.Name);

            return ProjectResponse.FromEntity(entity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}


