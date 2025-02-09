using System.Collections.Concurrent;
using Admin;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Test;

public class AdminTests
{
    private readonly CommunicationHub _hub;
    private const string ConnectionId = "test-connection-id";

    public AdminTests()
    {
        Mock<IHubCallerClients> mockClients = new();
        var mockContext = new Mock<HubCallerContext>();

        mockContext.Setup(c => c.ConnectionId).Returns(ConnectionId);
        
        _hub = new CommunicationHub
        {
            Clients = mockClients.Object,
            Context = mockContext.Object
        };
    }

    [Fact]
    public void SetUserId_AddsToUserIds()
    {
        _hub.SetUserId("testUser");

        var userIdsField = typeof(CommunicationHub)
            .GetField("UserIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var userIds = userIdsField?.GetValue(null) as ConcurrentDictionary<string, string>;
        
        Assert.NotNull(userIds);
        Assert.True(userIds.TryGetValue(ConnectionId, out var storedUserId));
        Assert.Equal("testUser", storedUserId);
    }
    
    [Fact]
    public void SetUserId_DoesNotAddUserId()
    {
        _hub.SetUserId(string.Empty);
        
        var userIdsField = typeof(CommunicationHub)
            .GetField("UserIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var userIds = userIdsField?.GetValue(null) as ConcurrentDictionary<string, string>;

        Assert.False(userIds!.ContainsKey(ConnectionId));
    }
    
    [Fact]
    public async Task OnDisconnectedAsync_RemovesUserFromUserIds()
    {
        _hub.SetUserId("testUser");

        var userIdsField = typeof(CommunicationHub)
            .GetField("UserIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var userIds = userIdsField?.GetValue(null) as ConcurrentDictionary<string, string>;
        
        await _hub.OnDisconnectedAsync(null);

        Assert.False(userIds!.ContainsKey(ConnectionId));
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithNonExistingUser_HandlesGracefully()
    {

        await _hub.OnDisconnectedAsync(null);
        
        var userIdsField = typeof(CommunicationHub)
            .GetField("UserIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var userIds = userIdsField?.GetValue(null) as ConcurrentDictionary<string, string>;
        
        Assert.NotNull(userIds);
        Assert.False(userIds.ContainsKey(ConnectionId));
    }
}