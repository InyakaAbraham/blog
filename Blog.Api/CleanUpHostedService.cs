using Blog.Persistence;

namespace Blog.Api;

public class CleanUpHostedService:BackgroundService, IDisposable
{
    
    private readonly ILogger<CleanUpHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _timer;


    public CleanUpHostedService(ILogger<CleanUpHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
    }
   
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
           
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                OnTick(context);
                
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hosted Service ended");

        return base.StopAsync(cancellationToken);
    }
    
    private void OnTick(DataContext context)
    {
        _logger.LogInformation("Timer ticked at {Now}", DateTime.Now);
        _logger.LogInformation("Number of Admins {Count}", context.Authors.Count());
        //Get all the token from customer table that has 

    }
    
    public sealed override void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}