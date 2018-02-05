using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Services.Permissions
{
    /// <summary>
    /// Implementation for Permissions
    /// </summary>
    public class PermissionsService
#if !EXCLUDE_INTERFACES
        : IPermissionsService
#endif
    {
        /// <summary>
        /// Constructor for implementation
        /// </summary>
        public PermissionsService() => throw new NotImplementedException();

        /// <summary>
        /// Request to see if you should show a rationale for requesting permission
        /// Only on Android
        /// </summary>
        /// <returns>True or false to show rationale</returns>
        /// <param name="permission">Permission to check.</param>
        public Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission) => throw new NotImplementedException();

        /// <summary>
        /// Determines whether this instance has permission the specified permission.
        /// </summary>
        /// <returns><c>true</c> if this instance has permission the specified permission; otherwise, <c>false</c>.</returns>
        /// <param name="permission">Permission to check.</param>
        public Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission) => throw new NotImplementedException();

        /// <summary>
        /// Requests the permissions from the users
        /// </summary>
        /// <returns>The permissions and their status.</returns>
        /// <param name="permissions">Permissions to request.</param>
        public async Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions) => throw new NotImplementedException();

        public bool OpenAppSettings() => throw new NotImplementedException();
    }
}
