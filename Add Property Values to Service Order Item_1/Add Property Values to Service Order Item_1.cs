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

namespace Add_Property_Values_to_Service_Order_Item_1
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

    using DomHelpers.SlcServicemanagement;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		/// 

		DomHelper _DomHelper;

		public void Run(IEngine engine)
		{
			try
			{
				InitHelpers(engine);

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

            string propertiesParam = engine.GetScriptParam("Property Values ID").Value;
            Guid propertiesDomId = JsonConvert.DeserializeObject<List<Guid>>(propertiesParam).Single();

            string objectParam = engine.GetScriptParam("Object ID").Value;
            Guid domId = JsonConvert.DeserializeObject<List<Guid>>(objectParam).Single();

            engine.GenerateInformation($"{propertiesDomId.ToString()} - {domId.ToString()}");

			//var domPropertiesIntanceId = new DomInstanceId(Guid.Parse(propertiesParam));
			//// create filter to filter event instances with specific dom event ids
			//var filter = DomInstanceExposers.Id.Equal(domPropertiesIntanceId);
			//var domPropertiesInstance = _DomHelper.DomInstances.Read(filter).First<DomInstance>();
			//var servicePropertyValuesinstance = new ServicePropertyValuesInstance(domPropertiesInstance);


			var domIntanceId = new DomInstanceId(domId);
            // create filter to filter event instances with specific dom event ids
            var filter = DomInstanceExposers.Id.Equal(domIntanceId);
            var domInstance = _DomHelper.DomInstances.Read(filter).First<DomInstance>();

			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceOrderItems.Id)
			{
                var serviceOrderItemInstance = new ServiceOrderItemsInstance(domInstance);
                serviceOrderItemInstance.ServiceOrderItemServiceInfo.Properties = propertiesDomId;
                serviceOrderItemInstance.Save(_DomHelper);
            } 
			else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
                var serviceSpecInstance = new ServiceSpecificationsInstance(domInstance);
                serviceSpecInstance.ServiceSpecificationInfo.ServiceProperties = propertiesDomId;
                serviceSpecInstance.Save(_DomHelper);
            }
			else
			{
				throw new Exception($"DOM definition {domInstance.DomDefinitionId.ToString()} linked to instance with Object ID not supported");
			}

        }

		private void InitHelpers(IEngine engine)
		{
            _DomHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
        }

	}
}