using Connr.Process;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddProcessService();
builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Lifetime.ApplicationStopping.Register(() =>
{
    var processService = app.Services.GetRequiredService<IProcessService>();
    if (processService.RunningProcessingCount == 0) return;
    
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Application stopping... stopping {processService.RunningProcessingCount} processes");
    processService.StopAll();
    logger.LogInformation($"-- Stopping: {processService.RunningProcessingCount} processes remaining");
    if (processService.RunningProcessingCount > 0)
    {
        var processIds =
            string.Join(",",
                processService.GetRunningProcesses().Select(s => s.Statistics.ProcessId));
        logger.LogError(
            "Failed to stop all running processes. {processService.RunningProcessingCount} " +
            "processes remaining. Process IDs: {pids}", processService.RunningProcessingCount, processIds);
    }

    Environment.Exit(processService.RunningProcessingCount == 0 ? Codes.Success : Codes.Error);
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();