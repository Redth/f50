using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Services.Permissions
{
#if !EXCLUDE_INTERFACES
	public interface IPermissionsService
	{
		Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission);

		Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission);

		Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions);

		bool OpenAppSettings();
	}
#endif
}
