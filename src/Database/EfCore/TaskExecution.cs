﻿using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class TaskExecution
    {
        public int Id { get; set; }
        public DateTime InsertDate { get; set; }
        public bool IsSuccess { get; set; }
        public int UpdateCount { get; set; }
        public string Environment { get; set; }
        public string TaskName { get; set; }
    }
}
