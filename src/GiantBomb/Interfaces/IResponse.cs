﻿using System.Collections.Generic;

namespace GBLive.GiantBomb.Interfaces
{
    public interface IResponse
    {
        Reason Reason { get; set; }
        LiveNowData? LiveNow { get; set; }
        IReadOnlyCollection<IShow> Shows { get; set; }
    }
}
