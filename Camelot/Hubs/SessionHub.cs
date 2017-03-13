using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Camelot.Models;
using System.Diagnostics;

namespace Camelot
{
    [HubName("sessionMini")]
    public class SessionHub : Hub
    {
        private readonly Program _program;

        public SessionHub() : this(Program.Instance) { }

        public SessionHub(Program program)
        {
            _program = program;
        }

        public IEnumerable<Session> GetAllSessions()
        {
            return _program.GetAllSessions();
        }

        [HubMethodName("broadcastData")]
        public static void BroadcastData()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SessionHub>();
            context.Clients.All.updateData();
        }

        [HubMethodName("broadcastActives")]
        public static void BroadcastActives()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SessionHub>();
            context.Clients.All.updateActives();
        }

        [HubMethodName("broadcastTopic")]
        public static void BroadcastTopic(Round round)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SessionHub>();
            context.Clients.All.updateVoteControls(round.ID);
        }

        [HubMethodName("responceToTopic")]
        public static void RespondToTopic(int roundID)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SessionHub>();
            context.Clients.All.updateGraph(roundID);
        }

        [HubMethodName("RepeatTopic")]
        public static void RepeatTopic()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SessionHub>();
            context.Clients.All.updateRepeat();
        }
    }
}