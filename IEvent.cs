using FunctionExtensions.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT.CQRS
{
    public interface IEvent
    {

    }

    public interface IEventHandler<TEvent>
      where TEvent : IEvent
    {
        Result Handle(TEvent domainEvent);
    }

}
