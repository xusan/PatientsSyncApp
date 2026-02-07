using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApp.AppServices.Common
{
    public abstract class AppServiceBase
    {
        private readonly IServiceScopeFactory scopeFactory;
        protected readonly ILogger logger;

        protected AppServiceBase(IServiceScopeFactory scopeFactory, ILogger logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected async Task<ActionResultResponse<TResult>> ExecuteScopedAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var result = await action(scope.ServiceProvider);
                    return new ActionResultResponse<TResult>(result);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{this.GetType().Name}.ExecuteScopedAsync() failed to execute with error");
                return new ActionResultResponse<TResult>(ex);
            }
        }

        protected async Task<ActionResponse> ExecuteScopedAsync(Func<IServiceProvider, Task> action)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    await action(scope.ServiceProvider);
                    return new ActionResponse();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{this.GetType().Name}.ExecuteScopedAsync() failed to execute with error");
                return new ActionResponse(ex);
            }
        }
    }
}
