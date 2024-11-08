namespace VeeamReports_Service;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(30);

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string result = ExecutePowerShell(SendVeeamData.Script);
                _logger.LogInformation("PowerShell Output: {result}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing PowerShell");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
    
    private string ExecutePowerShell(string scriptText)
    {
        using (PowerShell powerShell = PowerShell.Create())
        {
            powerShell.AddScript(scriptText);
            var results = powerShell.Invoke();

            if (powerShell.HadErrors)
            {
                throw new InvalidOperationException("PowerShell script execution had errors.");
            }

            // Collect the results into a string
            var output = string.Join(Environment.NewLine, results.Select(r => r.ToString()));
            return output;
        }
    }
}