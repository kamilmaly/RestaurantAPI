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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        int Create(CreateRestaurantDto dto);
        PagedResult<RestaurantDto> GetAll(RestaurantQuery restaurantQuery);
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

        public PagedResult<RestaurantDto> GetAll(RestaurantQuery restaurantQuery)
        {
            var baseQuery = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .Where(r => restaurantQuery.SearchPhrase == null || (r.Name.ToLower().Contains(restaurantQuery.SearchPhrase.ToLower())
                || r.Description.ToLower().Contains(restaurantQuery.SearchPhrase.ToLower())));

            if (!string.IsNullOrEmpty(restaurantQuery.SortBy))
            {
                var columnsSelectors = new Dictionary<string, Expression<Func<Restaurant, object>>>
                {
                    { nameof(Restaurant.Name), r => r.Name},
                    { nameof(Restaurant.Description), r => r.Description},
                    { nameof(Restaurant.Category), r => r.Category},
                };

                var selectedColumn = columnsSelectors[restaurantQuery.SortBy];

                baseQuery = restaurantQuery.SortDirection == SortDirection.ASC ?
                    baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            var restaurants = baseQuery
                .Skip(restaurantQuery.PageSize * (restaurantQuery.PageNumber - 1))
                .Take(restaurantQuery.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();

            var restaurantsDtos = _mapper.Map<List<RestaurantDto>>(restaurants);

            var result = new PagedResult<RestaurantDto>(restaurantsDtos,totalItemsCount,restaurantQuery.PageSize,restaurantQuery.PageNumber);

            return result;
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
