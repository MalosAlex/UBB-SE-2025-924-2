using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLayer.Models;
using BusinessLayer.Exceptions;

namespace BusinessLayer.Services.Proxies
{
    public class ServiceProxy
    {
        protected readonly HttpClient httpClient;
        protected readonly string baseUrl;
        private static string authToken;

        // Store the session info on the client side
        protected static UserWithSessionDetails CurrentUser { get; private set; }

        public ServiceProxy(string baseUrl = "http://localhost:7262/api/")
        {
            this.baseUrl = baseUrl;
            var handler = new HttpClientHandler()
            {
                UseProxy = false,
                // Add these crucial lines for local development:
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                // ^ This bypasses SSL certificate validation, use only in development!
            };
            httpClient = new HttpClient(handler);
            // Set reasonable timeouts
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // If we have an auth token, include it with every request
            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", authToken);
            }
        }

        protected async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await httpClient.GetAsync($"{baseUrl}{endpoint}").ConfigureAwait(false);
                return await HandleResponse<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException($"Network error: {ex.Message}", ex);
            }
        }

        protected async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var requestUri = $"{baseUrl}{endpoint}";
            using var content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json");

            // Use 30-second timeout for troubleshooting
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                Console.WriteLine($"Starting POST to {requestUri} at {DateTime.Now:HH:mm:ss.fff}");

                // Set explicit timeout on the request level as well
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var response = await httpClient.PostAsync(requestUri, content, cts.Token).ConfigureAwait(false);

                Console.WriteLine($"Response received at {DateTime.Now:HH:mm:ss.fff} with status: {response.StatusCode}");

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response deserialized, content length: {json.Length}");

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Request timed out after 30 seconds");
                throw new ServiceException("The request timed out after 30 seconds.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                throw new ServiceException($"HTTP request failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.GetType().Name}: {ex.Message}");
                throw new ServiceException($"Request failed: {ex.Message}", ex);
            }
        }

        protected async Task PostAsync(string endpoint, object data)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync($"{baseUrl}{endpoint}", content).ConfigureAwait(false);
                await EnsureSuccessStatusCode(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException($"Network error: {ex.Message}", ex);
            }
        }

        protected async Task<T> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PutAsync($"{baseUrl}{endpoint}", content).ConfigureAwait(false);
                return await HandleResponse<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException($"Network error: {ex.Message}", ex);
            }
        }

        protected async Task<T> DeleteAsync<T>(string endpoint)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"{baseUrl}{endpoint}").ConfigureAwait(false);
                return await HandleResponse<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ServiceException($"Network error: {ex.Message}", ex);
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            await EnsureSuccessStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException("Authentication required");
                    case System.Net.HttpStatusCode.Forbidden:
                        throw new UnauthorizedAccessException("You don't have permission to access this resource");
                    case System.Net.HttpStatusCode.NotFound:
                        throw new RepositoryException("Resource not found");
                    default:
                        throw new ServiceException($"API error: {response.StatusCode}. {content}");
                }
            }
        }

        // Method to set the auth token after successful login
        protected void SetAuthToken(string token)
        {
            authToken = token;
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);
        }

        // Method to store current user session
        protected void SetCurrentUser(UserWithSessionDetails user)
        {
            CurrentUser = user;
        }

        // Method to clear the current user session
        protected void ClearCurrentUser()
        {
            CurrentUser = null;
        }
    }
}