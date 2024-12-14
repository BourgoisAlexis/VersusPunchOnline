using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class SimpleTCPConnection : TCPConnection {
    public override async Task SendMessage(object obj, params TcpClient[] clients) {
        if (clients.Length <= 0)
            throw new Exception("No client connected");

        _swSend.Restart();

        try {
            string json = JsonConvert.SerializeObject(obj);
            byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(json);
            int dataLength = dataArray.Length;
            byte[] lengthPrefix = BitConverter.GetBytes(dataLength);

            foreach (TcpClient client in clients) {
                NetworkStream stream = client.GetStream();

                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await stream.WriteAsync(dataArray, 0, dataArray.Length);
            }
        }
        catch (Exception ex) {
            Utils.LogError($"{ex.Message}");
            throw ex;
        }

        _swSend.Stop();
        Utils.Log($"MessageSent in {_swSend.ElapsedMilliseconds} ms");
    }

    protected override async Task ReadMessage(NetworkStream stream) {
        _swReceived.Restart();

        byte[] lengthPrefix = new byte[4];
        await stream.ReadAsync(lengthPrefix, 0, lengthPrefix.Length);
        int dataLength = BitConverter.ToInt32(lengthPrefix, 0);

        byte[] dataArray = new byte[dataLength];
        int bytesRead = 0;

        while (bytesRead < dataLength) {
            int read = await stream.ReadAsync(dataArray, bytesRead, dataLength - bytesRead);
            if (read == 0)
                throw new EndOfStreamException("Connection closed before full message was received.");
            bytesRead += read;
        }

        string json = System.Text.Encoding.UTF8.GetString(dataArray);

        try {
            OnMessageReceived?.Invoke(SimpleMessage.FromString<SimpleMessage>(json));
        }
        catch (Exception ex) {
            throw ex;
        }

        _swReceived.Stop();
        Utils.Log($"MessageRead in {_swReceived.ElapsedMilliseconds} ms");
    }
}