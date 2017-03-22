using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace ServerInterfaces
{
    /// <summary>
    /// Class that represents one move in game, which player did.
    /// </summary>
    [DataContract]
    public class GameMove
    {
        [Column(Order = 1)]
        [Key]
        [DataMember(Name = "moveId")]
        public int Id { get; set; }

        [Column(Order = 2)]
        [Key]
        [DataMember(Name = "gameid")]
        public Guid GameId { get; set; }

        [DataMember(Name = "player")]
        public string PlayerName { get; set; }

        [DataMember(Name = "x")]
        public int X { get; set; }

        [DataMember(Name = "y")]
        public int Y { get; set; }

        [DataMember(Name = "result")]
        public int Result { get; set; }
    }
}