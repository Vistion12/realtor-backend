using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyStore.Core.Abstractions
{
    public interface ITelegramService
    {
        Task SendMessageAsync(string message);
    }
}
