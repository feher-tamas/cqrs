using FunctionExtensions.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FT.CQRS
{
    public interface IBus
    {
        Result Send(ICommand command);

        T Send<T>(IQuery<T> query);

        void Send(IEnumerable<IEvent> events);

        void Send(IEvent appevent);
    }

}
