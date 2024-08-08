using System.Net.Sockets;
using System;
using System.Threading.Tasks;

public class TCPBandWidthTester : TCPConnectionManager {
    private long _totalBytesSent;
    private long _totalBytesReceived;

    protected override async Task ReadMessage(NetworkStream stream) {
        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        _totalBytesReceived += bytesRead;

        if (_swReceived.ElapsedMilliseconds >= 1000) {
            Utils.Log(this, "Read", $"Time={_swReceived.ElapsedMilliseconds} Bytes={_totalBytesReceived.ToString("N0")} Messages={_totalBytesReceived / 1024}");
            _swReceived.Restart();
            _totalBytesReceived = 0;
        }
    }

    public override async Task SendMessage(object obj) {
        if (_guests.Count <= 0)
            throw new Exception("No client");

        byte[] buffer = new byte[1024];

        try {
            foreach (TcpClient client in _guests) {
                NetworkStream stream = client.GetStream();

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }

            _totalBytesSent += buffer.Length;

            if (_swSend.ElapsedMilliseconds >= 1000) {
                Utils.Log(this, "Write", $"Time={_swSend.ElapsedMilliseconds} Bytes={_totalBytesSent.ToString("N0")} Messages={_totalBytesSent / 1024}");
                _swSend.Restart();
                _totalBytesSent = 0;
            }
        }
        catch (Exception ex) {
            Utils.LogError(this, "SendMessage", ex.Message);
            throw ex;
        }
    }
}