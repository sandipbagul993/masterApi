﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Master.API.Models;

public partial class Flight
{
    public int FlightId { get; set; }

    public string FlightNumber { get; set; }

    public int AirlineId { get; set; }

    public int DepartureAirportId { get; set; }

    public int ArrivalAirportId { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public string Duration { get; set; }

    public decimal EconomyClass { get; set; }

    public decimal BusinessClass { get; set; }

    public virtual Airline Airline { get; set; }

    public virtual Airport ArrivalAirport { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Airport DepartureAirport { get; set; }
}