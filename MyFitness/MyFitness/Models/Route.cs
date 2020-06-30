using System;

namespace MyFitness.Models
{
    [Serializable]
    public class Route
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string Value { get; set; }
    }
}