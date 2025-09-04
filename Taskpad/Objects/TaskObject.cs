using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskpad.Objects
{
    public enum TaskPriority
    {
        Low,
        Normal,
        Medium,
        High,
        None

    }
    public class TaskObject()
    {
        public ProjectObject ?Project { get; set; }
        public string ?Name { get; set; }
        public string ?DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public bool Completed { get; set; }
    }
}