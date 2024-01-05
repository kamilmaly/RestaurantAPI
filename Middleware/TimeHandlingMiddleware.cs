using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestaurantAPI.Exceptions;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace RestaurantAPI.Middleware
{
    public class TimeHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<TimeHandlingMiddleware> _logger;
        private Stopwatch _stopWatch;

        public TimeHandlingMiddleware(ILogger<TimeHandlingMiddleware> logger)
        {
            _logger = logger;
            _stopWatch = new Stopwatch();
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                _stopWatch.Start();
                await next.Invoke(context);
                _stopWatch.Stop();

                var elapsedMilliseconds = _stopWatch.ElapsedMilliseconds;
                if (elapsedMilliseconds / 1000 > 4)
                {
                    var message =
                        $"Request [{context.Request.Method}] at {context.Request.Path} took {elapsedMilliseconds} ms";

                    _logger.LogInformation(message);
                }

            }
            catch (ForbidException forbidException)
            {
                context.Response.StatusCode = 403;
            }
            catch (BadRequestException badRequestException)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(badRequestException.Message);
            }
            catch (NotFoundException notFoundException)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(notFoundException.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Something went wrong");
            }
        }
    }
}


