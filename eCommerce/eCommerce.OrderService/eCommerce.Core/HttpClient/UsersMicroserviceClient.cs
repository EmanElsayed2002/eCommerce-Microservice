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
    public class UsersMicroserviceClient(HttpClient _httpClient , IDistributedCache _cache , ILogger<UsersMicroserviceClient> _logger)
    {

        public record ApiResponse(UserDTO Data,
          string Message
     );

        public async Task<Result<ApiResponse>> GetUserById(Guid UserID)
        {
            try
            {
                var cachekey = $"User: {UserID}";
                var cachedkey = await _cache.GetStringAsync(cachekey);
                if (cachedkey != null)
                {
                    var user = JsonSerializer.Deserialize<ApiResponse>(cachedkey);
                    return Result.Ok(user);
                }

                HttpResponseMessage response = await _httpClient.GetAsync($"/api/User/{UserID}");
                if (!response.IsSuccessStatusCode)
                {
                    return Result.Fail("can not get user microservice");
                }
                var userReturned = await response.Content.ReadFromJsonAsync<ApiResponse>();
                var Userjson = JsonSerializer.Serialize(userReturned);
                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(3));
                await _cache.SetStringAsync(cachekey, Userjson, options);

                return Result.Ok(userReturned);





            }
            catch(BrokenCircuitException ex)
            {
                _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy data.");

                return new ApiResponse(
                    Data: new UserDTO(Guid.NewGuid(), "User1", "Temporal Name", "Temporal Gender"), Message: "hello"
                );
            }

        }



    }
}
