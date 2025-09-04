using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskpad.Objects
{
    public class ProjectObject
    {
        public string ?Name { get; set; }
        public string ?CreationDate { get; set; }
        public List<TaskObject> ?TaskList { get; set; }
    }
}
