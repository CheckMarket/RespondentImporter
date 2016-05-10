using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;

namespace CheckMarket.RespondentImporter.lib
{
    public class MessageChannel
    {
        private static IList<WebSocket> ExistingSockets = new List<WebSocket>();
                
        public static async Task Echo(System.Net.WebSockets.WebSocket webSocket) {
            ExistingSockets.Add(webSocket);
            byte[] buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Test"), 0, Encoding.UTF8.GetBytes("Test").Length), WebSocketMessageType.Text, true, CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            ExistingSockets.Remove(webSocket);
        }

        public static async Task SendMessage(string text) {
            foreach (var socket in ExistingSockets)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
