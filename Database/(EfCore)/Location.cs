﻿using System;
using System.Collections.Generic;

#nullable disable

namespace ChekInsExtension.Database
{
    public partial class Location
    {
        public Location()
        {
            Attendances = new HashSet<Attendance>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}