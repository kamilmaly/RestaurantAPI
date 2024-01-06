using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RestaurantAPI.Controllers
{
    [Route("api/restaurant")]
    [ApiController]
    [Authorize]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }
        [HttpGet]
        [Authorize(Policy = "MultipleRestaurants")]
        //[AllowAnonymous]
        public ActionResult<IEnumerable<RestaurantDto>> GetAll([FromQuery]string searchPhrase)
        {


            var restaurantsDtos = _restaurantService.GetAll(searchPhrase);

            return Ok(restaurantsDtos);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete([FromRoute] int id)
        {

            _restaurantService.Delete(id);


            return NoContent();
        }   

        [HttpGet("{id}")]
        public ActionResult<RestaurantDto> Get([FromRoute] int id)
        {
 
            var restaurantsDtos = _restaurantService.GetById(id);

            return Ok(restaurantsDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult CreateRestaurant([FromBody] CreateRestaurantDto dto)
        {
            var userId = int.Parse(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var id = _restaurantService.Create(dto);

            return Created($"api/restaurant/{id}",null);
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public ActionResult PutRestaurant([FromBody] PutRestaurantDto dto, [FromRoute] int id)
        {

            _restaurantService.Put(dto, id);

            return Ok();
        }



    }
}
