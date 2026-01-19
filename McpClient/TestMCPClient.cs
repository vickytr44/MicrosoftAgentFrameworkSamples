using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace McpClientSamples;

public class TestMCPClient
{
    public static async Task RunTestMcpClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:5001")
        };

        // Accept SSE or anything
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("text/event-stream"));
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("*/*"));


        // ---------- 1️⃣ Initialize MCP session ----------
        Console.WriteLine("Initializing MCP session...");

        var initPayload = new
        {
            jsonrpc = "2.0",
            id = "init",
            method = "initialize",
            @params = new { }
        };

        var initRequest = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(initPayload),
                Encoding.UTF8,
                "application/json")
        };

        var initResponse = await http.SendAsync(
            initRequest,
            HttpCompletionOption.ResponseHeadersRead);

        initResponse.EnsureSuccessStatusCode();

        var sessionId = GetSessionId(initResponse);
        Console.WriteLine($"Session created: {sessionId}");


        // ---------- 2️⃣ List tools ----------
        Console.WriteLine("\nListing tools...");
        await SendMcpAsync(http, sessionId, new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "tools/list"
        });


        // ---------- 3️⃣ Call TestAgent ----------
        Console.WriteLine("\nCalling TestAgent...");
        await SendMcpAsync(http, sessionId, new
        {
            jsonrpc = "2.0",
            id = 2,
            method = "tools/call",
            @params = new
            {
                name = "TestAgent",
                arguments = new
                {
                    query = "Say hello in one sentence"
                }
            }
        });


        // ---------------- helpers ----------------

        static string GetSessionId(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Mcp-Session-Id", out var values))
                return values.First();

            throw new InvalidOperationException("Mcp-Session-Id header not found.");
        }

        static async Task SendMcpAsync(
            HttpClient http,
            string sessionId,
            object payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "/mcp")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json")
            };

            req.Headers.Add("Mcp-Session-Id", sessionId);

            var res = await http.SendAsync(
                req,
                HttpCompletionOption.ResponseHeadersRead);

            res.EnsureSuccessStatusCode();

            using var stream = await res.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    Console.WriteLine(line);
            }
        }
    }
}
