using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixthCircle
{
    public class HandlerDescriptor
    {
        HandlerDescriptor ()
        { }

        public static HandlerDescriptor FromReader (DisReader reader)
        {
            return new HandlerDescriptor ();
        }
    }
}
