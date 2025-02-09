using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
namespace Admin;

public class CommunicationHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> UserIds = new();
    private static readonly string BaseImagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Images");

    public CommunicationHub()
    {
        if (!Directory.Exists(BaseImagesFolder))
        {
            Directory.CreateDirectory(BaseImagesFolder);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine(UserIds.TryRemove(Context.ConnectionId, out var userId)
            ? $"User disconnected: {userId} (Connection ID: {Context.ConnectionId})"
            : $"User with Connection ID {Context.ConnectionId} was not found in UserIds.");

        await base.OnDisconnectedAsync(exception);
    }

    public void SetUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            Console.WriteLine("User ID cannot be null or empty.");
            return;
        }

        UserIds[Context.ConnectionId] = userId;

        Console.WriteLine($"User with Connection ID {Context.ConnectionId} set their ID to: {userId}");
    }

    public async Task SendMessage(string clientId, string message)
    {
        Console.WriteLine($"Received from {clientId}: {message}");
        await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveMessage", clientId, message);
    }

    private static string GetScreenshotPath(string userId)
    {
        var userFolder = Path.Combine(BaseImagesFolder, userId);
        Console.WriteLine($"User folder is {userFolder}");

        if (!Directory.Exists(userFolder))
        {
            Console.WriteLine($"User folder {userFolder} does not exist. Creating it.");
            Directory.CreateDirectory(userFolder);
        }
        
        return Path.Combine(userFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.jpg");
    }


    private static async Task WriteImageToFile(ChannelReader<byte[]> stream, string filePath)
    {
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        while (await stream.WaitToReadAsync())
        {
            while (stream.TryRead(out var chunk))
            {
                await fileStream.WriteAsync(chunk.AsMemory(), CancellationToken.None);
            }
        }
    }

    public async Task ReceiveScreenshot(ChannelReader<byte[]> stream)
    {
        UserIds.TryGetValue(Context.ConnectionId, out var userId);

        try
        {
            Console.WriteLine("Writing image...");
            await WriteImageToFile(stream, GetScreenshotPath(userId!));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write image: {ex.Message}");
        }
    }
}