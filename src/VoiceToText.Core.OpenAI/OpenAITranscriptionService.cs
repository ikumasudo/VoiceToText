using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace VoiceToText.Core.OpenAI;

/// <summary>
/// OpenAI transcription service using the Audio Transcriptions API.
/// Implements exponential backoff retry for transient errors.
/// </summary>
public sealed class OpenAITranscriptionService : ITranscriptionService, IDisposable
{
    private const string ApiEndpoint = "https://api.openai.com/v1/audio/transcriptions";
    private const int MaxRetries = 3;
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)];

    private readonly HttpClient _httpClient;
    private bool _isDisposed;

    public string? ApiKey { get; set; }

    public OpenAITranscriptionService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
    }

    public OpenAITranscriptionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TranscriptionResult> TranscribeAsync(byte[] wavData, TranscriptionOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            return new TranscriptionResult
            {
                Success = false,
                Error = "API key is not configured",
                ErrorType = TranscriptionErrorType.Authentication
            };
        }

        if (wavData == null || wavData.Length == 0)
        {
            return new TranscriptionResult
            {
                Success = false,
                Error = "No audio data provided",
                ErrorType = TranscriptionErrorType.InvalidRequest
            };
        }

        options ??= new TranscriptionOptions();

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var result = await SendRequestAsync(wavData, options, cancellationToken);

                // Don't retry on success or non-transient errors
                if (result.Success ||
                    result.ErrorType == TranscriptionErrorType.Authentication ||
                    result.ErrorType == TranscriptionErrorType.InvalidRequest)
                {
                    return result;
                }

                // Retry on transient errors (rate limit, network, server error)
                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelays[attempt], cancellationToken);
                }
                else
                {
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                return new TranscriptionResult
                {
                    Success = false,
                    Error = "Request was cancelled",
                    ErrorType = TranscriptionErrorType.Unknown
                };
            }
            catch (Exception ex)
            {
                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelays[attempt], cancellationToken);
                }
                else
                {
                    return new TranscriptionResult
                    {
                        Success = false,
                        Error = $"Network error: {ex.Message}",
                        ErrorType = TranscriptionErrorType.Network
                    };
                }
            }
        }

        return new TranscriptionResult
        {
            Success = false,
            Error = "Max retries exceeded",
            ErrorType = TranscriptionErrorType.Unknown
        };
    }

    private async Task<TranscriptionResult> SendRequestAsync(byte[] wavData, TranscriptionOptions options, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(wavData);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(fileContent, "file", "audio.wav");

        content.Add(new StringContent(options.Model), "model");

        if (!string.IsNullOrEmpty(options.Language))
        {
            content.Add(new StringContent(options.Language), "language");
        }

        content.Add(new StringContent("json"), "response_format");

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement.GetProperty("text").GetString() ?? "";

                // Apply text formatting
                text = FormatText(text, options);

                return new TranscriptionResult
                {
                    Success = true,
                    Text = text
                };
            }
            catch (JsonException ex)
            {
                return new TranscriptionResult
                {
                    Success = false,
                    Error = $"Failed to parse response: {ex.Message}",
                    ErrorType = TranscriptionErrorType.Unknown
                };
            }
        }

        return ParseErrorResponse(response.StatusCode, responseBody);
    }

    private static string FormatText(string text, TranscriptionOptions options)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // Trim whitespace
        text = text.Trim();

        // Handle punctuation
        if (!options.AddPunctuation)
        {
            // Remove common punctuation
            text = text.Replace(".", "").Replace(",", "").Replace("!", "").Replace("?", "").Replace(";", "").Replace(":", "");
        }

        // Handle newlines
        if (!options.PreserveNewlines)
        {
            text = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            // Remove multiple spaces
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }
        }

        return text;
    }

    private static TranscriptionResult ParseErrorResponse(HttpStatusCode statusCode, string responseBody)
    {
        var errorType = statusCode switch
        {
            HttpStatusCode.Unauthorized => TranscriptionErrorType.Authentication,
            HttpStatusCode.TooManyRequests => TranscriptionErrorType.RateLimit,
            HttpStatusCode.BadRequest => TranscriptionErrorType.InvalidRequest,
            HttpStatusCode.InternalServerError => TranscriptionErrorType.ServerError,
            HttpStatusCode.ServiceUnavailable => TranscriptionErrorType.ServerError,
            HttpStatusCode.BadGateway => TranscriptionErrorType.ServerError,
            HttpStatusCode.GatewayTimeout => TranscriptionErrorType.ServerError,
            _ => TranscriptionErrorType.Unknown
        };

        string errorMessage;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("error", out var errorElement) &&
                errorElement.TryGetProperty("message", out var messageElement))
            {
                errorMessage = messageElement.GetString() ?? $"HTTP {(int)statusCode}";
            }
            else
            {
                errorMessage = $"HTTP {(int)statusCode}: {responseBody}";
            }
        }
        catch
        {
            errorMessage = $"HTTP {(int)statusCode}: {responseBody}";
        }

        return new TranscriptionResult
        {
            Success = false,
            Error = errorMessage,
            ErrorType = errorType
        };
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _httpClient.Dispose();
        _isDisposed = true;
    }
}
