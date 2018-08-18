using System;
using System.Collections.Generic;
using System.Text;

namespace Piksel.GrowlLib.Events
{
    public class RegistrationAttemptEventArgs: EventArgs
    {
        public bool Allowed { get; set; } = false;
    }
}
