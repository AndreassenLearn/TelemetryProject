﻿namespace Services.Models
{
    public class Humidex
    {
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
