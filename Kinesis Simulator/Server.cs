using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperWebSocket;
using System.Timers;
using System.Diagnostics; 

namespace Kinesis_Simulator
{
    public class Server
    {
        private List<WebSocketSession> m_Sessions = new List<WebSocketSession>();
        private object m_SessionSyncRoot = new object();
        private object m_SecureSessionSyncRoot = new object();
        private Timer m_SecureSocketPushTime;

        private static WebSocketServer socketServer;

        public Server()
        {
            socketServer = new WebSocketServer();
            socketServer.Setup(new RootConfig(),
                new ServerConfig
                {
                    Name = "SuperWebSocket",
                    Ip = "Any",
                    Port = 2011,
                    Mode = SocketMode.Sync
                }, SocketServerFactory.Instance);
        }

        internal void Start()
        {
            socketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            socketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(socketServer_SessionClosed);
            socketServer.Start();
        }

        internal void Stop()
        {
            if (socketServer.IsRunning)
            {
                socketServer.NewMessageReceived -= socketServer_NewMessageReceived;
                socketServer.NewSessionConnected -= socketServer_NewSessionConnected;
                socketServer.SessionClosed -= socketServer_SessionClosed;
                socketServer.Stop();
            }
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            Console.WriteLine(e);
        }

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine(session);
            lock (m_SessionSyncRoot)
                m_Sessions.Add(session);
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            lock (m_SessionSyncRoot)
                m_Sessions.Remove(session);

            if (reason == CloseReason.ServerShutdown)
                return;
        }

        public void SendToAll(string message)
        {
            lock (m_SessionSyncRoot)
            {
                foreach (var s in m_Sessions)
                {
                    s.SendResponse(message);
                }
            }
        }
    }
}
