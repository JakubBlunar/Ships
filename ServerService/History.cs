using System;
using System.ComponentModel.DataAnnotations;
using ServerInterfaces;

namespace ServerService
{
    /// <summary>
    /// History represents ended game. 
    /// Player can later watch this game.
    /// </summary>
    public class History
    {
        [Key]
        public Guid Id { get; set; }

        public Game Game { get; set; }
        public DateTime Date { get; set; }
    }
}