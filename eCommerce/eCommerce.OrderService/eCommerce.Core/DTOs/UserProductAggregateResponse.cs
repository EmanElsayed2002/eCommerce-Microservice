using System;
using System.Collections.Generic;

namespace eCommerce.BusinessLogicLayer.DTOs
{
    public record UserProductAggregateResponse(
        Guid UserID,
        IReadOnlyCollection<UserProductAggregateProductResponse> Products);

    public record UserProductAggregateProductResponse(
        Guid ProductID,
        string ProductName,
        int TotalQuantity,
        decimal TotalAmount);
}

