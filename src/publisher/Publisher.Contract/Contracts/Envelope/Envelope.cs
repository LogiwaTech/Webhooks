using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Contract.Contracts.Envelope
{
    public class Envelope<T>
    {
        public Envelope(T result, string message = null)
        {
            Message = message;
            Result = result;
        }

        public string Message { get; set; }
        public DateTime Timestamp => DateTime.Now;
        public T Result { get; set; }
    }
}
