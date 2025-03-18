using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfxSocketServer.Session.Model
{
    /// <summary>
    /// Poco for parsing incoming messages
    /// </summary>
    public class SessionInfo
    {
        /// <summary>
        /// Session id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set;  }

        /// <summary>
        /// Query string params
        /// </summary>
        public Dictionary<string, string> QueryStringParams { get; set; }

        /// <summary>
        /// The channel I'm a member of
        /// </summary>
        public Guid? ChannelId { get; set; }
    }
}
