using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;

using FluentResults;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Polly.Bulkhead;
using Polly.CircuitBreaker;


namespace eCommerce.BusinessLogicLayer.HttpClientt
{
    public class UsersMicroserviceClient(HttpClient _httpClient, IDistributedCache _cache, ILogger<UsersMicroserviceClient> _logger)
    {
        public record ApiResponse(UserDTO Data, string Message);

        public async Task<Result<ApiResponse>> GetUserById(Guid userId)
        {
            try
            {
                string cacheKey = $"user:{userId}";

                // 1. Check Cache
                string cached = await _cache.GetStringAsync(cacheKey);
                if (cached != null)
                {
                    var cachedObj = JsonSerializer.Deserialize<ApiResponse>(cached);
                    return Result.Ok(cachedObj);
                }

                // 2. Call API
                HttpResponseMessage response = await _httpClient.GetAsync($"/api/User/{userId}");

                if (!response.IsSuccessStatusCode)
                    return Result.Fail("Cannot communicate with User microservice");

                // 3. Parse
                var obj = await response.Content.ReadFromJsonAsync<ApiResponse>();

                if (obj == null || obj.Data == null)
                    return Result.Fail("User API returned null response");

                // 4. Cache result
                var cacheJson = JsonSerializer.Serialize(obj);
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

                await _cache.SetStringAsync(cacheKey, cacheJson, options);

                return Result.Ok(obj);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Circuit breaker OPEN. Returning fallback user.");

                return Result.Ok(
                    new ApiResponse(
                        new UserDTO(Guid.NewGuid(), "fallback@email.com", "Temporary User", "Unknown"),
                        "Fallback mode"
                    )
                );
            }
        }
    }

}
