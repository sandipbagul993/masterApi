﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Master.API.Models;

public partial class Interviewer
{
    public int InterviewerId { get; set; }

    public int? UserId { get; set; }

    public int? CompanyId { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public int? HrId { get; set; }

    public virtual Company Company { get; set; }

    public virtual Hr Hr { get; set; }

    public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

    public virtual User User { get; set; }
}