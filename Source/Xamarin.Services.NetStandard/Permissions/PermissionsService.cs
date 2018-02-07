using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Services.Permissions
{
	public class PermissionsService
#if !EXCLUDE_INTERFACES
		: IPermissionsService
#endif
	{
		public PermissionsService() => throw new NotImplementedException();

		public Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission) => throw new NotImplementedException();

		public Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission) => throw new NotImplementedException();

		public async Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions) => throw new NotImplementedException();

		public bool OpenAppSettings() => throw new NotImplementedException();
	}
}
