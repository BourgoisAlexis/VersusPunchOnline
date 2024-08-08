using System.Net.Sockets;
using System;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

public class TCPGameplay<T> : TCPConnectionManager where T : class {
    public Action<T> onMessageReceived;

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
            T data = JsonUtility.FromJson<T>(json);
            onMessageReceived?.Invoke(data);
        }
        catch (Exception ex) {
            throw ex;
        }

        _swReceived.Stop();
        Utils.Log(this, "Reading Time", $"{_swReceived.ElapsedMilliseconds}");
    }

    public override async Task SendMessage(object obj) {
        if (_guests.Count <= 0)
            throw new Exception("No client");

        _swSend.Restart();

        try {
            string json = JsonUtility.ToJson(obj);
            byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(json);
            int dataLength = dataArray.Length;
            byte[] lengthPrefix = BitConverter.GetBytes(dataLength);

            foreach (TcpClient client in _guests) {
                NetworkStream stream = client.GetStream();

                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await stream.WriteAsync(dataArray, 0, dataArray.Length);
            }
        }
        catch (Exception ex) {
            Utils.LogError(this, "SendMessage", ex.Message);
            throw ex;
        }

        _swSend.Stop();
        Utils.Log(this, "Writing Time", $"{_swSend.ElapsedMilliseconds}");
    }
}