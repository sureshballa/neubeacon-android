using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeuBeacons.Core {
	/// <summary>
	/// Manager classes are an abstraction on the data access layers
	/// </summary>
	public static class BeaconManager {

		static BeaconManager ()
		{
		}
		
		public static Beacon GetBeaconAsync(String id)
		{
			return BeaconRepositoryADO.GetBeaconAsync(id);
		}
		
		public async static Task<IList<Beacon>> GetBeaconsAsync ()
		{
			return await BeaconRepositoryADO.GetBeaconsAsync();
		}

		public static void SaveBeaconAsync (Beacon item)
		{
			BeaconRepositoryADO.SaveBeaconAsync(item);
		}
		
		public async static Task DeleteBeaconAsync(String id)
		{
			await BeaconRepositoryADO.DeleteBeaconAsync(id);
		}
	}
}