﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Master.API.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int? UserId { get; set; }

    public string Mobile { get; set; }

    public string CollegeName { get; set; }

    public string University { get; set; }

    public string City { get; set; }

    public string Graduation { get; set; }

    public string Branch { get; set; }

    public string GraduationYear { get; set; }

    public virtual User User { get; set; }
}