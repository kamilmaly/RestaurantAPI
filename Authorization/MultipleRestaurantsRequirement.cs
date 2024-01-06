using Microsoft.AspNetCore.Authorization;

namespace RestaurantAPI.Authorization
{
    public class MultipleRestaurantsRequirement : IAuthorizationRequirement
    {
        public int MinimumCreatedRestaurants { get; }

    public MultipleRestaurantsRequirement(int minimumCreatedRestaurants)
    {
            MinimumCreatedRestaurants = minimumCreatedRestaurants;
    }
}
}