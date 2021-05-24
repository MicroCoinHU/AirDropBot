using System;

namespace RewardBot
{
    public class AirDrop
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string Account { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
    }
}
