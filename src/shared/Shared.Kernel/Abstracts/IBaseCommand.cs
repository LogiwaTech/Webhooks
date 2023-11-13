using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Kernel.Abstracts
{
    public interface IBaseCommand<in TRequestModel, TCommand>
    where TRequestModel : IBaseRequestModel
    where TCommand : IBaseCommand<TRequestModel, TCommand>
    { 
        static abstract TCommand FromRequest(TRequestModel request);
    }
}
