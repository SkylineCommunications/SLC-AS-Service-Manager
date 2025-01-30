namespace Get_ServiceItemsMultipleSections_1
{
    // Used to process the Service Items

    using System;
    using System.Collections.Generic;
    using System.Linq;
    // Required to mark the interface as a GQI data source 
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Sections;

    using System.Linq;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Apps.Sections.Sections;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Sections;

    using DomHelpers;
    using DomHelpers.SlcServicemanagement;
    using Skyline.DataMiner.Net.Helper;

    [GQIMetaData(Name = "Get_ServiceItemsMultipleSections")]
    public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        // defining input argument, will be converted to guid by OnArgumentsProcessed
        private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
        private readonly GQIStringArgument sectionNameArg = new GQIStringArgument("Section") { IsRequired = true };
        // variable where input argument will be stored
        private Guid instanceDomId;
        private string sectionName;

        private GQIDMS dms;

        private DomInstance _domInstance;

        private DomHelper _DomHelper;


        public GQIColumn[] GetColumns()
        {
            //if (sectionName == "Feeds")
            //{
            //    return new GQIColumn[]
            //            {
            //            new GQIBooleanColumn("Selected"),
            //            new GQIStringColumn("Feed Role"),
            //            new GQIStringColumn("Feed Reference"),
            //            };
            //}
            //else
            //{
            //    return new GQIColumn[]
            //    {
            //        new GQIStringColumn("Not"),
            //        new GQIStringColumn("Supported"),
            //    };
            //}

            return new GQIColumn[]
                {
                    new GQIStringColumn("Label"),
                    new GQIIntColumn("Service Item ID"),
                    new GQIStringColumn("Service Item Type"),
                    new GQIStringColumn("Definition Reference"),
                    new GQIStringColumn("Service Item Script"),
                    new GQIStringColumn("Implementation Reference"),
                };
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] {
                domIdArg,
                sectionNameArg,
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            //GenerateInformationEvent("GetNextPage started");

            return new GQIPage(GetMultiSection())
            {
                HasNextPage = false,
            };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            // adds the input argument to private variable
            if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out instanceDomId))
            {
                instanceDomId = Guid.Empty;
            }

            sectionName = args.GetArgumentValue(sectionNameArg);

            return new OnArgumentsProcessedOutputArgs();
        }

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            dms = args.DMS;

            return default;
        }

        private GQIRow[] GetMultiSection()
        {
            //GenerateInformationEvent("Get Service Items Multisection started");

            // define output list
            var rows = new List<GQIRow>();

            if (instanceDomId == Guid.Empty)
            {
                // return th empty list
                return rows.ToArray();
            }

            // will initiate DomHelper 
            LoadApplicationHandlersAndHelpers();

            var domIntanceId = new DomInstanceId(instanceDomId);
            // create filter to filter event instances with specific dom event ids
            var filter = DomInstanceExposers.Id.Equal(domIntanceId);

            _domInstance = _DomHelper.DomInstances.Read(filter).First<DomInstance>();

            // Service item list to fill with either service or service specifiation's service items
            IList<ServiceItemsSection> serviceItems = new List<ServiceItemsSection>();

            if (_domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
            {
                var instance = new ServicesInstance(_domInstance);

                serviceItems = instance.ServiceItems;

            } 
            else if (_domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
            {
                var instance = new ServiceSpecificationsInstance(_domInstance);
                serviceItems = instance.ServiceItems;
            }


            // GenerateInformationEvent("test");
            serviceItems.ForEach(item =>
            {
                rows.Add(
                    new GQIRow(new[] {
                                new GQICell{ Value = item.Label },
                                new GQICell{ Value = (int)(item.ServiceItemID ?? 0)},
                                new GQICell{ Value = SlcServicemanagementIds.Enums.Serviceitemtypes.ToValue(item.ServiceItemType.Value) ?? String.Empty },
                                new GQICell{ Value = item.DefinitionReference },
                                new GQICell{ Value = item.ServiceItemScript },
                                new GQICell{ Value = item.ImplementationReference },
                    })
                    );

            });

            return rows.ToArray();
        }

        private void LoadApplicationHandlersAndHelpers()
        {
            _DomHelper = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
        }

        public DMSMessage GenerateInformationEvent(string message)
        {
            var generateAlarmMessage = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message);
            return dms.SendMessage(generateAlarmMessage);
        }
    }
}