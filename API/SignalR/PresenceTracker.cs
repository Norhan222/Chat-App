namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string,List<String>> OnlineUsers =
            
           new Dictionary<string,List<String>>();

        public Task<bool> UserConnected(string username,string connectionId)
        {
            bool isOnline=false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<String> { connectionId});
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        { 
            bool isOfffline=false;
            lock (OnlineUsers)
            {
                if(!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOfffline);

                OnlineUsers[(username)].Remove(connectionId);
                if (OnlineUsers[username].Count ==0)
                {
                    OnlineUsers.Remove(username);
                    isOfffline = true;
                }
            }
            return Task.FromResult(isOfffline);
        }

        public Task<String[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers=OnlineUsers.OrderBy(k=>k.Key).Select(k=>k.Key).ToArray(); 
            }
            return  Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                connectionIds=OnlineUsers.GetValueOrDefault(username);
            }
            return Task.FromResult(connectionIds);
        }
    }
}
