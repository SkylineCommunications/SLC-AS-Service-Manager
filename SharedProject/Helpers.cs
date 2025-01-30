using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

namespace CustomHelpers
{
    internal class Helpers
    {
        private DomInstance FetchDomInstance(DomHelper domHelper, Guid instanceDomId)
        {
            var domIntanceId = new DomInstanceId(instanceDomId);
            // create filter to filter event instances with specific dom event ids
            var filter = DomInstanceExposers.Id.Equal(domIntanceId);

            DomInstance domInstance = domHelper.DomInstances.Read(filter).First<DomInstance>();

            return domInstance;
        }

    }
}
