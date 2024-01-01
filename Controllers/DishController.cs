using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using System.Collections.Generic;

namespace RestaurantAPI.Controllers
{
    [Route("api/restaurant/{restaurantId}/dish")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private readonly IDishService _dishService;

        public DishController(IDishService dishService)
        {
            _dishService = dishService;
        }

        [HttpDelete]
        public ActionResult DeleteAll([FromRoute] int restaurantId)
        {

            _dishService.DeleteAll(restaurantId);


            return NoContent();
        }

        [HttpDelete("{dishId}")]
        public ActionResult DeleteById([FromRoute] int restaurantId, [FromRoute] int dishId)
        {

            _dishService.DeleteById(restaurantId, dishId);


            return NoContent();
        }

        [HttpGet("{dishId}")]
        public ActionResult<DishDto> Get([FromRoute] int restaurantId, [FromRoute] int dishId)
        {

            var dishDtos = _dishService.GetById(restaurantId, dishId);

            return Ok(dishDtos);
        }

        [HttpGet]
        public ActionResult<IEnumerable<DishDto>> GetAll([FromRoute] int restaurantId)
        {

            var dishDtos = _dishService.GetAll(restaurantId);

            return Ok(dishDtos);
        }

        [HttpPost]
        public ActionResult Post([FromRoute] int restaurantId, [FromBody] CreateDishDto dto)
        { 

            var id = _dishService.Create(restaurantId, dto);

            return Created($"api/restaurant/{restaurantId}/dish/{id}", null);
        }
    }
}
