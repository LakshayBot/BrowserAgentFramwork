using BrowserAgent.Api.Application.DTOs.AI;

namespace BrowserAgent.Api.Application.Interfaces;

public interface IAiClient
{
    Task<ResumeParseResponse> ParseResumeAsync(ResumeParseRequest request, CancellationToken ct = default);
    Task<FieldMapResponse> MapFieldsAsync(FieldMapRequest request, CancellationToken ct = default);
    Task<CompanyAnalyzeResponse> AnalyzeCompanyAsync(CompanyAnalyzeRequest request, CancellationToken ct = default);
    Task<QuestionAnswerResponse> AnswerQuestionAsync(QuestionAnswerRequest request, CancellationToken ct = default);
}
