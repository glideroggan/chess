using Microsoft.Extensions.Logging;

namespace app.Services
{
    public abstract class BaseService<T>
    {
        protected BaseService(ICacher cacher, ILogger<T> logger, IPerfService perfService)
        {
            Cacher = cacher;
            Logger = logger;
            PerfService = perfService;
        }

        protected ICacher Cacher { get; }
        protected ILogger<T> Logger { get; set; }
        protected static IPerfService PerfService { get; set; }
        
    }
}