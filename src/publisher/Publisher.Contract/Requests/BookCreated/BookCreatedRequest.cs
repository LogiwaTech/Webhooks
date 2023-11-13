using Shared.Kernel.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Publisher.Contract.Requests.BookCreated
{
    public class BookCreatedRequest : IBaseRequestModel
    {
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
