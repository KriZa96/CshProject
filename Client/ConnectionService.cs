using Microsoft.AspNetCore.SignalR.Client;

namespace Client;

public class ConnectionService
{
    private readonly string _userName;
    private readonly HubConnection _hubConnection;
    private readonly ScreenshotService _screenshotService;

    private ConnectionService(string userName, HubConnection hubConnection, ScreenshotService screenshotService)
    {
        _userName = userName;
        _hubConnection = hubConnection;
        _screenshotService = screenshotService;
    }
    
    private static HubConnection BuildHubConnection(string hubEndpoint)
    {
        return new HubConnectionBuilder()
            .WithUrl(hubEndpoint)
            .Build();
    }
    
    public static ConnectionService CreateConnection(string hubEndpoint = "http://localhost:5189/communication")
    {
        var connection = BuildHubConnection(hubEndpoint);
        
        string? userName = null;
        while (string.IsNullOrWhiteSpace(userName))
        {
            Console.WriteLine("Enter client username:");
            userName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Username is required.");
            }
        }
        
        return new ConnectionService(userName, connection, new ScreenshotService());
    }
    
    private void RegisterOperations()
    {
        _hubConnection.On<string, string>("ReceiveMessage", (senderId, message) =>
        {
            Console.WriteLine($"Message from {senderId}: {message}");
        });
    }

    private async Task Set_userName()
    {
        await _hubConnection.InvokeAsync("SetUserId", _userName);
    }
    
    private async Task SendMessageAsync(string message)
    {
        await _hubConnection.InvokeAsync("SendMessage", _userName, message);
    }

    private async Task StartConnectionAsync()
    {
        try
        {
            RegisterOperations();
            await _hubConnection.StartAsync();
            await Set_userName();
            Console.WriteLine("Connection established.");
        } 
        catch
        {
            Console.WriteLine("Connection could not be established.");
            await RemoveConnectionAsync();
        }
    }
    
    private async Task RemoveConnectionAsync()
    {
        Console.WriteLine("Closing connection...");
        
        _screenshotService.Stop();
        await _hubConnection.StopAsync();
        await _hubConnection.DisposeAsync();
        
        Console.WriteLine("Connection closed.");
    }

    private static string PrepareMessage()
    {
        string? message = null;
        while (string.IsNullOrWhiteSpace(message))
        {
            message = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("Message cannot be empty. Try again.");
            }
        }

        return message;
    }

    private async Task StartMessagingCommunicationAsync()
    {
        while (true)
        {
            var message = PrepareMessage();
                
            if (string.Equals(message, "exit", StringComparison.OrdinalIgnoreCase))
            {
                await RemoveConnectionAsync();
                break;
            }
                
            await SendMessageAsync(message);
        }
    }

    public async Task OpenCommunication()
    {
        try
        {
            await StartConnectionAsync();
            _screenshotService.StartScreenshotTimer(_hubConnection);
            await StartMessagingCommunicationAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred -> {ex.Message}");
            await RemoveConnectionAsync();
        }
    }
}