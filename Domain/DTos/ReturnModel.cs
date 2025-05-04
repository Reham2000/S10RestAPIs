using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTos
{
    public class ReturnModel<T>
    {
        public bool IsSuccessed { get; set; }
        public List<string>? Errors { get; set; }
        public T? Model { get; set; }

    }
}
