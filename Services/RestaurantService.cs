using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.LayoutRenderers.Wrappers;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        int Create(CreateRestaurantDto dto);
        IEnumerable<RestaurantDto> GetAll(string searchPhrase);
        RestaurantDto GetById(int id);

        void Put(PutRestaurantDto dto,int id);

       void Delete(int id);
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RestaurantService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public RestaurantService(RestaurantDbContext dbContext, IMapper mapper, ILogger<RestaurantService> logger
            , IAuthorizationService authorizationService,IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
        }


        public void Put(PutRestaurantDto dto,int id)
        {
            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(x => x.Id == id);

            if (restaurant is null)
                 throw new NotFoundException("Restaurant not found");

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurant,
                new ResourceOperationRequirement(ResourceOperation.Update)).Result;

            if(!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            _mapper.Map(dto, restaurant);

            _dbContext.Restaurants.Update(restaurant);
            _dbContext.SaveChanges();

        }


        public void Delete(int id)
        {
            _logger.LogWarning($"Its delete function. This is {id}");

            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(x => x.Id == id);

            if (restaurant is null)
                throw new NotFoundException("Restaurant not found");

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurant,
              new ResourceOperationRequirement(ResourceOperation.Delete)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            _dbContext.Restaurants.Remove(restaurant);
            _dbContext.SaveChanges();

        }

        public RestaurantDto GetById(int id)
        {
            var restaurant = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .FirstOrDefault(x => x.Id == id);

            if (restaurant is null)
                throw new NotFoundException("Restaurant not found");

            var restaurantsDtos = _mapper.Map<RestaurantDto>(restaurant);

            return restaurantsDtos;
        }

        public IEnumerable<RestaurantDto> GetAll(string searchPhrase)
        {
            var restaurants = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .Where(r=> searchPhrase == null || (r.Name.ToLower().Contains(searchPhrase.ToLower()) 
                || r.Description.ToLower().Contains(searchPhrase.ToLower())))
                .ToList();

            var restaurantsDtos = _mapper.Map<List<RestaurantDto>>(restaurants);

            return restaurantsDtos;
        }

        public int Create(CreateRestaurantDto dto)
        {

            var restaurantsDtos = _mapper.Map<Restaurant>(dto);
            restaurantsDtos.CreatedById = _userContextService.GetUserId;
            _dbContext.Restaurants.Add(restaurantsDtos);
            _dbContext.SaveChanges();

            return restaurantsDtos.Id;

        }

    }
}
