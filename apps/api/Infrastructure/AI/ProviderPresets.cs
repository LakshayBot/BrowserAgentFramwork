namespace BrowserAgent.Api.Infrastructure.AI;

public record ProviderPreset(string Key, string DisplayName, string BaseUrl, List<string> Models);

public static class ProviderPresets
{
    public static readonly List<ProviderPreset> All = new()
    {
        new("DeepSeek", "DeepSeek", "https://api.deepseek.com", new()
        {
            "deepseek-chat",
            "deepseek-reasoner"
        }),
        new("DeepSeek_V4_Flash", "DeepSeek Flash", "https://api.deepseek.com", new()
        {
            "deepseek-flash"
        }),
        new("OpenAI", "OpenAI", "https://api.openai.com/v1", new()
        {
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4-turbo",
            "gpt-4",
            "gpt-3.5-turbo",
            "o1",
            "o1-mini",
            "o3-mini"
        }),
        new("Ollama", "Ollama (Local)", "http://localhost:11434", new()
        {
            "llama3.2",
            "llama3.1",
            "llama3",
            "mistral",
            "codellama",
            "phi3",
            "gemma2",
            "qwen2.5"
        }),
    };
}
