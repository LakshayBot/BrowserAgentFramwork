using BrowserAgent.Api.Application.DTOs.Workflows;

namespace BrowserAgent.Api.Application.Interfaces;

public interface IWorkflowService
{
    Task<WorkflowDto> CreateAsync(Guid userId, CreateWorkflowRequest request, CancellationToken ct = default);
    Task<WorkflowDto> StartAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<WorkflowDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<List<WorkflowSummaryDto>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<WorkflowDto> PauseAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<WorkflowDto> ResumeAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<WorkflowDto> CancelAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
