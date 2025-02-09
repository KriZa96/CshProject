using Client;

DpiHelper.SetDpiAwareness();

var connection = ConnectionService.CreateConnection();

await connection.OpenCommunication();