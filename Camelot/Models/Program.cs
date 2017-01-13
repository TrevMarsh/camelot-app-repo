using Camelot.DAL;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

namespace Camelot.Models
{
    public class Program
    {
        #region Members
        // Singleton instance
        private readonly static Lazy<Program> _instance = new Lazy<Program>(() => new Program(GlobalHost.ConnectionManager.GetHubContext<SessionHub>().Clients));

        private readonly ConcurrentDictionary<string, Session> _sessions = new ConcurrentDictionary<string, Session>();

        private readonly object _updateSessionsLock = new object();

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly Timer _timer;
        private volatile bool _updateSessions = false;

        private CamelotContext db = new CamelotContext();
        #endregion

        private Program(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            _sessions.Clear();

            var sessions = db.Sessions.Where(s => s.Active == true && s.EndTime == null).ToList();
            
            sessions.ForEach(session => _sessions.TryAdd(session.Name, session));

            _timer = new Timer(UpdateSessions, null, _updateInterval, _updateInterval);

        }

        public static Program Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients { get; set; }

        public IEnumerable<Session> GetAllSessions()
        {
            return _sessions.Values;
        }

        private void UpdateSessions(object state)
        {
            lock (_updateSessionsLock)
            {
                if (!_updateSessions)
                {
                    _updateSessions = true;

                    foreach (var session in _sessions.Values)
                    {
                        if (TryUpdateSession(session))
                        {
                            BroadCastSession(session);
                        }
                    }

                    _updateSessions = false;
                }
            }
        }

        private void BroadCastSession(object session)
        {
            Clients.All.updateSession(session);
        }

        private bool TryUpdateSession(Session session)
        {
            return true;
        }
    }
}