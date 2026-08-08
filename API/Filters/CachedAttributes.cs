using System.Text.Json;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace API.Filters
{
    public class CachedAttributes : Attribute, IAsyncActionFilter
    {
        private readonly int _expireTime;

        public CachedAttributes(int expireTime)
        {
            _expireTime = expireTime;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

            var cacheKey = MakeCacheKey(context.HttpContext.Request);

            var cachedResponse = await cacheService.GetAsync<string>(cacheKey);

            // Cache Hit
            if (!string.IsNullOrEmpty(cachedResponse))
            {
                context.Result = new ContentResult
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };

                return;
            }

            // Execute Action
            var executedContext = await next();

            // Cache Miss
            if (executedContext.Result is OkObjectResult response)
            {
                var json = JsonSerializer.Serialize(response.Value);

                await cacheService.SetAsync(
                    cacheKey,
                    json,
                    TimeSpan.FromSeconds(_expireTime));
            }
        }

        private static string MakeCacheKey(HttpRequest request)
        {
            var key = request.Path.ToString();

            foreach (var (name, value) in request.Query.OrderBy(x => x.Key))
            {
                key += $":{name}-{value}";
            }

            return key;
        }
    }
}