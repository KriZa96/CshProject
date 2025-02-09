
using System.Drawing.Imaging;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR.Client;
using Point = System.Drawing.Point;

namespace Client;

public class ScreenshotService
{ 
    private System.Threading.Timer? _screenshotTimer;

    public void StartScreenshotTimer(HubConnection hubConnection)
    {
        Stop();  // U slucaju da vec imamo pokrenut timer zaustavljamo postojeci prije kreiranja novog
        _screenshotTimer = new System.Threading.Timer(async _ =>
        {
            var screenshot = CaptureScreen();
            await SendImageAsync(hubConnection, ImageToByteArray(screenshot));
            screenshot.Dispose();
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    public void Stop()
    {
        _screenshotTimer?.Change(Timeout.Infinite, 0);
        _screenshotTimer?.Dispose();
    }
    
    private static Bitmap CaptureScreen()
    {
        var bounds = Screen.PrimaryScreen!.Bounds;
        var bitmap = new Bitmap(bounds.Width, bounds.Height);

        try
        {
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(
                upperLeftSource:  bounds.Location, 
                upperLeftDestination: Point.Empty, 
                blockRegionSize: bounds.Size
            );
            return bitmap;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            bitmap.Dispose();
            throw;
        }
    }

    private static byte[] ImageToByteArray(Bitmap image)
    {
        using var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Jpeg);
        return ms.ToArray();
    }

    private async Task SendImageAsync(HubConnection hubConnection, byte[] imageData)
    {
        var channel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(capacity: 10)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
        
        try
        {
            await Task.Run(async () =>
            {
                var totalBytes = imageData.Length;
                var position = 0;

                while (position < totalBytes)
                {
                    var bytesToSend = Math.Min(64 * 1024, totalBytes - position);
                    var chunk = new byte[bytesToSend];
                    Array.Copy(imageData, position, chunk, 0, bytesToSend);

                    await channel.Writer.WriteAsync(chunk);
                    position += bytesToSend;
                }

                channel.Writer.Complete();
            });

            await hubConnection.InvokeAsync("ReceiveScreenshot", channel.Reader);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending image: {ex.Message}");
            Console.WriteLine("Stopping screenshot service...");
            Stop();
        }
        
    }
}