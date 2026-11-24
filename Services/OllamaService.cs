using System.Net.Http.Json;
using System.Text.Json;

namespace TrafficLaw.API.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OllamaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AskAsync(string question, CancellationToken cancellationToken = default)
        {
            var model = _configuration["Ollama:Model"] ?? "llama3.2";

            var payload = new
            {
                model,
                messages = new[]
                {
                    new { role = "user", content = question }
                },
                stream = false
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "api/chat", payload, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Ollama trả về HTTP {(int)response.StatusCode}: {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var content))
            {
                var answer = content.GetString();
                if (!string.IsNullOrWhiteSpace(answer))
                    return answer;
            }

            throw new InvalidOperationException("Ollama không trả về nội dung câu trả lời hợp lệ.");
        }
    }
}