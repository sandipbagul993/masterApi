﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Master.API.Models;

public partial class City
{
    public int CityId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Airport> Airports { get; set; } = new List<Airport>();
}