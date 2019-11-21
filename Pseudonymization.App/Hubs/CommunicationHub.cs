using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Pseudonymization.App.Models;

namespace Pseudonymization.App.Hubs
{
    public class CommunicationHub : Hub
    {
        static List<NotificationUser> Users = new List<NotificationUser>();

        public void Connect()
        {
            var id = Context.ConnectionId;

            if (!Users.Any(x => x.ConnectionId == id))
            {
                Users.Add(new NotificationUser { ConnectionId = id });
            }
        }

        public void Success(string connectionToken)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CommunicationHub>();
            var usr = Users.FirstOrDefault(u => u.ConnectionToken == connectionToken);

            if (usr != null)
            {
                context.Clients.Client(usr.ConnectionId).HandleSuccess();
            }
        }

        public void Failure(string connectionToken)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CommunicationHub>();
            var usr = Users.FirstOrDefault(u => u.ConnectionToken == connectionToken);

            if (usr != null)
            {
                context.Clients.Client(usr.ConnectionId).HandleFailed();
            }
        }

        public void updateProgress(int percetageValue, string connectionToken)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CommunicationHub>();
            var usr = Users.FirstOrDefault(u => u.ConnectionToken == connectionToken);

            if (usr != null)
            {
                context.Clients.Client(usr.ConnectionId).updateProgress(percetageValue);
            }
        }

        public void ChangeToken(string newToken)
        {
            var usr = Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);

            if (usr != null)
            {
                usr.ConnectionToken = newToken;
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var item = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (item != null)
            {
                Users.Remove(item);
                var id = Context.ConnectionId;
                //Clients.All.onUserDisconnected(id, item.Name);
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}