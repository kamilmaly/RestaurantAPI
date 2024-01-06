using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using RestaurantAPI.Entities;
using System.Security.Claims;
using System.Linq;

namespace RestaurantAPI.Authorization
{
    public class MultipleRestaurantsRequirementHandler: AuthorizationHandler<MultipleRestaurantsRequirement>
    {
        private readonly ILogger<MultipleRestaurantsRequirementHandler> _logger;
        private readonly RestaurantDbContext _dbContext;

        public MultipleRestaurantsRequirementHandler(ILogger<MultipleRestaurantsRequirementHandler> logger,RestaurantDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MultipleRestaurantsRequirement requirement)
        {
            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var countUserCreatedRestaurants = _dbContext.Restaurants.Count(r => r.CreatedById == int.Parse(userId));

            if(countUserCreatedRestaurants >= requirement.MinimumCreatedRestaurants)
            {
                _logger.LogInformation("Authorization succeeded: User has created at least two restaurants");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation($"Authorization failed: User has created only {countUserCreatedRestaurants} restaurants");
            }

            return Task.CompletedTask;

        }
    }
}