using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.SessionComps.Models;

namespace Eem.Thraxus.Bots.SessionComps.Support
{
	public static class GlobalEvents
	{
		// Prefabs use this to request information on a list of targets
		public static event Action<long, long> OnRequestTargetInformation;

		public static void TargetInformationRequest(long requestorId, long request)
		{
			OnRequestTargetInformation?.Invoke(requestorId, request);
		}

		// Entity Tracker uses this to respond to information requests
		public static event Action<long, EntityModel> OnResponseTargetInformation;

		public static void TargetInformationResponse(long requestorId, EntityModel response)
		{
			OnResponseTargetInformation?.Invoke(requestorId, response);
		}
	}
}