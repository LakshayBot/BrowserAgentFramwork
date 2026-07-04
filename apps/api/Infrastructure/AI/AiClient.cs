using System.Net.Http.Json;
using System.Text.Json;
using BrowserAgent.Api.Application.DTOs.AI;
using BrowserAgent.Api.Application.Interfaces;

namespace BrowserAgent.Api.Infrastructure.AI;

public class AiClient : IAiClient
{
    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<AiClient> _logger;

    public AiClient(HttpClient httpClient, ILogger<AiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResumeParseResponse> ParseResumeAsync(ResumeParseRequest request, CancellationToken ct = default)
    {
        return await PostAsync<ResumeParseRequest, ResumeParseResponse>("/resume/parse", request, ct);
    }

    public async Task<FieldMapResponse> MapFieldsAsync(FieldMapRequest request, CancellationToken ct = default)
    {
        return await PostAsync<FieldMapRequest, FieldMapResponse>("/field-map", request, ct);
    }

    public async Task<CompanyAnalyzeResponse> AnalyzeCompanyAsync(CompanyAnalyzeRequest request, CancellationToken ct = default)
    {
        return await PostAsync<CompanyAnalyzeRequest, CompanyAnalyzeResponse>("/company/analyze", request, ct);
    }

    public async Task<QuestionAnswerResponse> AnswerQuestionAsync(QuestionAnswerRequest request, CancellationToken ct = default)
    {
        return await PostAsync<QuestionAnswerRequest, QuestionAnswerResponse>("/answer-question", request, ct);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest request, CancellationToken ct)
        where TRequest : class
        where TResponse : class
    {
        var response = await _httpClient.PostAsJsonAsync(path, request, SnakeCaseOptions, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("AI service {Path} returned {StatusCode}: {Body}", path, (int)response.StatusCode, body);
            throw new HttpRequestException($"AI service error ({path}): {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(SnakeCaseOptions, ct);
        return result ?? throw new InvalidOperationException($"Empty response from AI service: {path}");
    }
}
