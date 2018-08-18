using System;
using System.Collections.Generic;
using System.Text;

namespace Piksel.GrowlLib.Common
{
    public enum StatusCode
    {
        Ok = 200,

        InvalidRequest = 300,
        UnknownRequest = 301,
        VersionNotSupported = 302,

        Unauthorized = 400,
        UnknownApp = 401,

        InternalServerError = 500,
    }
}
