
using Connr.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddNotificationService();

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
    var notificationSvc = app.Services.GetRequiredService<NotificationService>();
    Console.WriteLine("Application stopping... waiting 1500ms for stop");
    notificationSvc.NotifyStopping();
    Thread.Sleep(1500);
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    Console.WriteLine("Application stopped... hopefully all child processes are dead");
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();