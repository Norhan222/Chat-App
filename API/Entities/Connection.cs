namespace API.Entities
{
    public class Connection
    {
        public Connection()
        {
            
        }
        public Connection(string connection,string username)
        {
            ConnectionId=connection;
            UserName=username;
        }

        public string ConnectionId { get; set; }

        public string UserName { get; set; }
    }
}