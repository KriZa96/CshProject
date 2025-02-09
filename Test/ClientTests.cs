using System.Reflection;
using Client;

namespace Test;

public class ClientTest : IDisposable
{
    private readonly TextReader _originalIn;
    private readonly TextWriter _originalOut;
    
    public ClientTest()
    {
        _originalIn = Console.In;
        _originalOut = Console.Out;
    }

    public void Dispose()
    {
        Console.SetIn(_originalIn);
        Console.SetOut(_originalOut);
    }

    [Fact]
    public void CreateConnection_WithValidUsername_CreatesInstance()
    {
        var input = new StringReader("validUser\n");
        var output = new StringWriter();
        
        Console.SetIn(input);
        Console.SetOut(output);

        var service = ConnectionService.CreateConnection();
        Assert.NotNull(service);
        
        var userNameField = typeof(ConnectionService)
            .GetField("_userName", BindingFlags.NonPublic | BindingFlags.Instance);
        
        var userName = userNameField?.GetValue(service) as string;
        Assert.Equal("validUser", userName);
    }

    [Fact]
    public void CreateConnection_WithEmptyThenValidUsername_PromptsAndCreatesInstance()
    {
        var input = new StringReader("\nvalidUser\n");
        var output = new StringWriter();
        
        Console.SetIn(input);
        Console.SetOut(output);

        var service = ConnectionService.CreateConnection();
        Assert.NotNull(service);
        Assert.Contains("Username is required.", output.ToString());
        
        var userNameField = typeof(ConnectionService)
            .GetField("_userName", BindingFlags.NonPublic | BindingFlags.Instance);
        
        var clientId = userNameField?.GetValue(service) as string;
        Assert.Equal("validUser", clientId);
    }

    [Fact]
    public void PrepareMessage_WithValidMessage_ReturnsMessage()
    {
        var input = new StringReader("valid message\n");
        var output = new StringWriter();
        
        Console.SetIn(input);
        Console.SetOut(output);

        var prepareMessageMethod = typeof(ConnectionService)
            .GetMethod("PrepareMessage", BindingFlags.NonPublic | BindingFlags.Static);

        var result = prepareMessageMethod?.Invoke(null, null) as string;

        Assert.Equal("valid message", result);
    }

    [Fact]
    public void PrepareMessage_WithEmptyThenValidMessage_PromptsAndReturnsMessage()
    {
        var input = new StringReader("\nvalid message\n");
        var output = new StringWriter();
        
        Console.SetIn(input);
        Console.SetOut(output);

        var prepareMessageMethod = typeof(ConnectionService)
            .GetMethod("PrepareMessage", BindingFlags.NonPublic | BindingFlags.Static);

        var result = prepareMessageMethod?.Invoke(null, null) as string;

        Assert.Equal("valid message", result);
        Assert.Contains("Message cannot be empty.", output.ToString());
    }
}