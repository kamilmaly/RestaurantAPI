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
        private Stopwatch _stopwatch;

        public TimeHandlingMiddleware(ILogger<TimeHandlingMiddleware> logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                _stopwatch.Start();
                await next.Invoke(context);
                _stopwatch.Stop();
                var time = _stopwatch.Elapsed;

                TimeSpan timeLimit = TimeSpan.FromMilliseconds(200);

                if (time > timeLimit)
                {
                    _logger.LogWarning($"Request too long > 200 Milliseconds: {time} - {context.Request.Path} - {context.Request.Method}");
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


