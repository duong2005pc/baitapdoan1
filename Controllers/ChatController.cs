using Microsoft.AspNetCore.Mvc;
using TrafficLaw.API.Models;
using TrafficLaw.API.Services;

namespace TrafficLaw.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly OllamaService _ollamaService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(OllamaService ollamaService, ILogger<ChatController> logger)
        {
            _ollamaService = ollamaService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> Ask(
            [FromBody] ChatRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var answer = await _ollamaService.AskAsync(request.Question, cancellationToken);

                return Ok(new ChatResponseDto
                {
                    Status = "answered",
                    Answer = answer
                });
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Yêu cầu tới Ollama bị quá thời gian chờ.");
                return StatusCode(StatusCodes.Status504GatewayTimeout, new
                {
                    status = "timeout",
                    message = "Ollama phản hồi quá lâu. Vui lòng thử lại."
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Không thể kết nối hoặc nhận phản hồi từ Ollama.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "unavailable",
                    message = "Không thể kết nối tới Ollama. Hãy kiểm tra Ollama đang chạy và địa chỉ cấu hình."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý câu hỏi bằng Ollama.");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = "error",
                    message = "Đã xảy ra lỗi khi xử lý câu hỏi."
                });
            }
        }
    }
}