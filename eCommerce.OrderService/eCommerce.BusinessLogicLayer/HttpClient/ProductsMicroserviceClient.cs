using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using eCommerce.BusinessLogicLayer.DTOs;

using FluentResults;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using MongoDB.Driver.Core.Operations;

using Polly.Bulkhead;

namespace eCommerce.BusinessLogicLayer.HttpClientt
{
    public class ProductsMicroserviceClient(HttpClient _httpClient, IDistributedCache cache , ILogger<ProductsMicroserviceClient>logger)
    {
        public async Task<Result<ProductDTO>> GetProductByID(Guid productID)
        {
            try
            {
                string? cacheKey = $"product: {productID}";
                string cachedProduct = await cache.GetStringAsync(cacheKey);
                if (cachedProduct != null)
                {
                    var res = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                    return Result.Ok(res);
                }

                HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/products/search/product-id/{productID}");
                if (!response.IsSuccessStatusCode)
                {
                    return Result.Fail("Can not make communication with product microservice");
                }
                var p = await response.Content.ReadFromJsonAsync<ProductDTO>();
                var productJson = JsonSerializer.Serialize(p);
                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300));
                await cache.SetStringAsync(cacheKey , productJson , options);
                return Result.Ok(p);
            }
            catch (BulkheadRejectedException ex)
            {
                logger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full");

                return new ProductDTO(
                  ProductID: Guid.NewGuid(),
                  ProductName: "Temporarily Unavailable (Bulkhead)",
                  Category: "Temporarily Unavailable (Bulkhead)",
                  UnitPrice: 0,
                  QuantityInStock: 0);
            }

        }
        
    }
  
}
