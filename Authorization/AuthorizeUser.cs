using System.Security.Claims;

using dotnet_api_extensions.User;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Serilog;

namespace dotnet_api_extensions.Authorization
{
    /// <summary>
    /// Custom authorization attribute which works on allowed user actions.
    /// It does not inherit from AuthorizeAttribute to stop framework from adding out of the box authorization requirements.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class AuthorizeUser : Attribute, IAsyncAuthorizationFilter
    {
        private readonly IList<UserAction> _userActions;
        private readonly bool _allowLeisure;

        /// <summary>
        /// Constructor with allow leisure and user actions
        /// </summary>
        /// <param name="allowLeisure"></param>
        /// <param name="userActions"></param>
        public AuthorizeUser(bool allowLeisure, params UserAction[] userActions)
        {
            _allowLeisure = allowLeisure;
            _userActions = userActions == null ? new List<UserAction>() : new List<UserAction>(userActions);
        }

        /// <summary>
        /// Allowed comma separated roles for the controller/grpc action
        /// </summary>
        public string Roles { get; set; }

        /// <summary>
        /// Constructor with user action list
        /// </summary>
        /// <param name="userActions"></param>
        public AuthorizeUser(params UserAction[] userActions)
        {
            _userActions = userActions == null ? new List<UserAction>() : new List<UserAction>(userActions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var actionResult = await OnAuthorizationAsyncInternal(context.HttpContext);
            if (actionResult != null)
            {
                context.Result = actionResult;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ActionResult> OnAuthorizationAsyncInternal(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            //If allow leisure is true and user is found to be leisure (through some means), then allow
            if (_allowLeisure && IsLeisure(userIdClaim))
            {
                return null;
            }

            if (!context.User.Identity.IsAuthenticated ||
                context.RequestServices.GetService(typeof(IUserInfos)) is not IUserInfos userInfos
                || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return GetUnauthorized(context);
            }

            var user = await userInfos.Get(userIdClaim.Value);

            if (user == null || (!user.AllowedActions?.Any() ?? true))
            {
                return GetForbid(context);
            }

            //If user action list is empty and no Roles are specified, then allow
            if (!_userActions.Any() && string.IsNullOrEmpty(Roles))
            {
                return null;
            }

            var authorized = ValidateUserActions(user) || ValidateUserRoles(user);

            return !authorized ? GetForbid(context) : null;
        }

        private ForbidResult GetForbid(HttpContext context)
        {
            ResetLogPath(context, "Forbidden");
            return new ForbidResult();
        }

        private UnauthorizedResult GetUnauthorized(HttpContext context)
        {
            ResetLogPath(context, "Unauthorized");
            return new UnauthorizedResult();
        }

        private void ResetLogPath(HttpContext context, string actionDetails)
        {
            if (context.RequestServices.GetService(typeof(IDiagnosticContext)) is IDiagnosticContext diagnosticContext)
            {
                diagnosticContext.Set("DetailsFilePath", actionDetails);
            }
        }

        private bool ValidateUserActions(UserInfo user)
        {
            foreach (var userAction in _userActions)
            {
                if (user.AllowedActions.Contains(userAction))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidateUserRoles(UserInfo user)
        {
            var allowedRoles = Roles.Split(",").Select(x => x.Trim()).ToList();

            var userRoles = user.Roles.Select(x => x.Name);

            foreach (var allowedRole in allowedRoles)
            {
                if (userRoles.Contains(allowedRole, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsLeisure(Claim userIdClaim)
        {
            return userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value);
        }
    }
}