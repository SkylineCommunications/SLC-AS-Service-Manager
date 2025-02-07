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
namespace SLC_SM_Create_Service_Inventory_Item_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		DomHelper _domHelper;

		/// <summary>
		///     The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
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
				engine.ExitFail(e.Message);
			}
		}

		private void InitHelpers(IEngine engine)
		{
			_domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
		}

		private void RunSafe(IEngine engine)
		{
			string serviceName = JsonConvert.DeserializeObject<List<string>>(engine.GetScriptParam("Service Name").Value).FirstOrDefault()
								 ?? throw new InvalidOperationException("No Service Name provided as input");

			if (!Guid.TryParse(engine.GetScriptParam("Service Category").Value.Trim('"', '[', ']'), out Guid categoryDomId))
			{
				throw new InvalidOperationException("No Category specified as input");
			}

			if (!Guid.TryParse(engine.GetScriptParam("Service Specification").Value.Trim('"', '[', ']'), out Guid specDomId))
			{
				throw new InvalidOperationException("No Service Specification specified as input");
			}

			long startRaw = Convert.ToInt64(engine.GetScriptParam("Date Start").Value.Trim('"', '[', ']'));
			DateTimeOffset start = DateTimeOffset.FromUnixTimeMilliseconds(startRaw);

			long endRaw = Convert.ToInt64(engine.GetScriptParam("Date End").Value.Trim('"', '[', ']'));
			DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds(endRaw);

			var domInstance = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(specDomId)).FirstOrDefault()
							  ?? throw new InvalidOperationException($"No Service Specification found with ID '{specDomId}'.");
			var serviceSpecificationInstance = new ServiceSpecificationsInstance(domInstance);

			var serviceInstance = new ServicesInstance
			{
				ServiceInfo =
				{
					Name = serviceName,
					ServiceStartTime = start.UtcDateTime,
					ServiceEndTime = end.UtcDateTime,
					Icon = serviceSpecificationInstance.ServiceSpecificationInfo.Icon,
					Description = serviceSpecificationInstance.ServiceSpecificationInfo.Description,
					ServiceCategory = categoryDomId,
					ServiceSpecifcation = specDomId,
					ServiceProperties = serviceSpecificationInstance.ServiceSpecificationInfo.ServiceProperties,
					ServiceConfiguration = serviceSpecificationInstance.ServiceSpecificationInfo.ServiceConfiguration,
				},
			};

			foreach (var relationship in serviceSpecificationInstance.ServiceItemRelationship)
			{
				serviceInstance.ServiceItemRelationship.Add(relationship);
			}

			foreach (var item in serviceSpecificationInstance.ServiceItems)
			{
				serviceInstance.ServiceItems.Add(item);
			}

			serviceInstance.Save(_domHelper);
		}
	}
}