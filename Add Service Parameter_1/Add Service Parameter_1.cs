/*
****************************************************************************
*  Copyright (c) 2025,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2025	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace Add_Service_Parameter_1
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
    using System.Linq;
    using System.Text;
    using DomHelpers.SlcServicemanagement;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
	public class Script
	{

		IEngine _engine;
		DomHelper _DomHelper;

        /// <summary>
        /// The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public void Run(IEngine engine)
		{
			try
			{
				_engine = engine;

				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			// TODO: Define code here

			Guid serviceDomID = Guid.Parse(GetSingleStringParam("Dom ID"));
			engine.GenerateInformation($"ID {serviceDomID.ToString()}");
			
			string label = GetSingleStringParam("Label");

			string profileParamId = GetSingleStringParam("Profile parameter ID");

			string value = GetSingleStringParam("Value");

            DomHelper domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);

			DomInstance domInstance = FetchDomInstance(serviceDomID);

			var serviceSpecification = new ServiceSpecificationsInstance(domInstance);

			var configId = serviceSpecification.ServiceSpecificationInfo.ServiceConfiguration ?? Guid.Empty;

			ServiceConfigurationInstance configInstance;

            if (configId  != Guid.Empty)
			{
                // new config should be created 
                engine.GenerateInformation($"ID {configId.ToString()}");
                configInstance = new ServiceConfigurationInstance(FetchDomInstance(configId));
			} else
			{
                engine.GenerateInformation($"Create new config because ID {configId.ToString()} empty");
                configInstance = new ServiceConfigurationInstance();
			}

			var newConfig = new ServiceConfigurationParametersValuesSection();

			newConfig.Label = label;
			newConfig.ServiceParameterID = Guid.NewGuid().ToString();
			newConfig.ProfileParameterID = profileParamId;
			newConfig.StringValue = value;

			configInstance.ServiceConfigurationParametersValues.Add(newConfig);

			configInstance.Save(_DomHelper);

			serviceSpecification.ServiceSpecificationInfo.ServiceConfiguration = configInstance.ID.Id;

			serviceSpecification.Save(_DomHelper);
        }

		private string GetSingleStringParam(string name)
		{
            string inputParam = _engine.GetScriptParam(name).Value;
            string value = JsonConvert.DeserializeObject<List<string>>(inputParam).Single();

			return value;
        }


        private void InitHelpers(IEngine engine)
        {
            _DomHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
        }

        private DomInstance FetchDomInstance(Guid instanceDomId)
        {
            var domIntanceId = new DomInstanceId(instanceDomId);
            // create filter to filter event instances with specific dom event ids
            var filter = DomInstanceExposers.Id.Equal(domIntanceId);

            DomInstance domInstance = _DomHelper.DomInstances.Read(filter).First<DomInstance>();

            return domInstance;
        }

    }
}