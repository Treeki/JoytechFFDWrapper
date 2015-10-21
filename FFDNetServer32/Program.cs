using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace FFDNetServer32
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new TcpListener(System.Net.IPAddress.Loopback, 9911);
            listener.Start();

            var toRead = new List<Socket>();
            var clients = new List<Instance>();
            var socketMap = new Dictionary<Socket, Instance>();

            for (; ; )
            {
                toRead.Clear();
                toRead.Add(listener.Server);
                foreach (var client in clients)
                    toRead.Add(client.Socket);

                Socket.Select(toRead, null, null, -1);

                foreach (var socket in toRead)
                {
                    if (socket == listener.Server)
                    {
                        var client = new Instance(listener.AcceptTcpClient());
                        clients.Add(client);
                        socketMap[client.Socket] = client;
                        Console.WriteLine("connection obtained");
                    }
                    else
                    {
                        var client = socketMap[socket];
                        if (!client.HandleRead())
                        {
                            Console.WriteLine("it's over");
                            socketMap.Remove(client.Socket);
                            clients.Remove(client);
                            client.Bye();
                        }
                    }
                }
            }
        }
    }
}
