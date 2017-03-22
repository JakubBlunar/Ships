using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServerInterfaces
{
    /// <summary>
    /// Event represents data sended from server to client
    /// Id specifies type of event, client know how to react 
    /// on specific id
    /// </summary>
    [DataContract]
    public class Event : EventArgs
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "players")]
        public List<string> Players { get; set; }

        [DataMember(Name = "game")]
        public Game Game { get; set; }

        [DataMember(Name = "guid")]
        public Guid Guid { get; set; }

        [DataMember(Name = "guids")]
        public List<Guid> Guids { get; set; }

        [DataMember(Name = "result")]
        public bool Result { get; set; }
    }
}