﻿using OpenRPA.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA.Net
{
    public class Message : BaseMessage, IMessage
    {
        internal int sendcount = 0;
        public Message(string command)
        {
            id = Guid.NewGuid().ToString();
            this.command = command;
        }
        public Message(SocketMessage msg)
        {
            id = msg.id;
            replyto = msg.replyto;
            command = msg.command;
            data = msg.data;
        }
        public Message(SocketMessage msg, string data)
        {
            id = msg.id;
            replyto = msg.replyto;
            command = msg.command;
            this.data = data;
        }
        public void SendMessage(WebSocketClient ws, int priority)
        {
            if (string.IsNullOrEmpty(data))
            {
                var message = new SocketMessage(this, priority);
                message.Send(ws);
                sendcount++;
                // Log.Information("SendMessage(" + id + " / " + sendcount + ") " + command);
                return;
            }
            var messages = data.Split(ws.websocket_package_size).ToArray();
            if(command != "pong") Log.Network("Send (" + messages.Length.ToString() + ") " + command + " / " + data);
            for (var i = 0; i < messages.Length; i++)
            {
                var message = new SocketMessage(this, messages[i], messages.Length, i, priority);
                message.Send(ws);
            }
            sendcount++;
            // Log.Information("SendMessage(" + id + " / " + sendcount + ") " + command);
            // new Task(() => { ws.ProcessQueue(); });
            _ = ws.ProcessQueue();
            // 
        }
        public override string ToString()
        {
            if(!string.IsNullOrEmpty(replyto)) return command + " reply to " + replyto;
            return command + " " + id;
        }
    }
}
