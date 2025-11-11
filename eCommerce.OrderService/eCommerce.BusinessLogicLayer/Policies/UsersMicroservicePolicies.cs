using System;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Wrap;

namespace eCommerce.BusinessLogicLayer.Policies
{
    public class UsersMicroservicePolicies : IUsersMicroservicePolicies
    {
        private readonly ILogger<UsersMicroservicePolicies> _logger;
        private readonly IPollyPolicies _pollyPolicies;

        public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger, IPollyPolicies pollyPolicies)
        {
            _logger = logger;
            _pollyPolicies = pollyPolicies;
        }

        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            // Retry: Try 5 times with exponential backoff
            var retryPolicy = _pollyPolicies.GetRetryPolicy(5);

            // Circuit Breaker: Open circuit after 3 failures, wait 2 minutes
            var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));

            // Timeout: Fail if request takes more than 5 seconds
            var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5));

            // Wrap all policies together (executes: Timeout → Circuit Breaker → Retry)
            AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

            return wrappedPolicy;
        }
    }
}


