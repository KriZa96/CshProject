var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(
    hubOptions =>
    {
        hubOptions.EnableDetailedErrors = true;
        // Trebalo bi procjeniti najbolji iznos max message i buffer size-a
        hubOptions.MaximumReceiveMessageSize = 128 * 1024;
        hubOptions.StreamBufferCapacity = 1024;
    });

var app = builder.Build();
app.MapHub<Admin.CommunicationHub>("/communication");

await app.RunAsync();