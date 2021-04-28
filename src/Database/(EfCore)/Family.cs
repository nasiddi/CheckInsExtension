﻿using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Family
    {
        public Family()
        {
            KidOlds = new HashSet<KidOld>();
        }

        public int Id { get; set; }
        public long HouseholdId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<KidOld> KidOlds { get; set; }
    }
}
