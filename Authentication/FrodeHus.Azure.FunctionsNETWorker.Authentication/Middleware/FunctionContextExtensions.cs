using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;

namespace FrodeHus.Azure.FunctionsNETWorker.Authentication
{
    public static class FunctionContextExtensions
    {
        public static bool IsInRole(this FunctionContext context, IEnumerable<string> roles)
        {
            return roles.Any(r => context.IsInRole(r));
        }
        public static bool IsInRole(this FunctionContext context, string role)
        {
            if (!context.IsAuthenticated())
            {
                return false;
            }

            if (!context.Items.ContainsKey("identity"))
            {
                return false;
            }
            if (context.Items["identity"] is not ClaimsPrincipal principal)
            {
                return false;
            }

            return principal.FindAll(c => c.Type == ClaimTypes.Role).Any(c => c.Value == role);
        }

        public static bool IsAuthenticated(this FunctionContext context)
        {
            if (!context.Items.ContainsKey("isAuthenticated"))
            {
                return false;
            }
            if (context.Items["isAuthenticated"] is not bool authenticated)
            {
                return false;
            }

            return authenticated;
        }

        public static string GetAuthenticationError(this FunctionContext context)
        {
            if (!context.Items.ContainsKey("Auth.Error"))
            {
                return null;
            }
            return context.Items["Auth.Error"] as string;
        }

        public static bool VerifyUserRoles(this FunctionContext context)
        {
            var requiredRoles = GetRolesFromAttribute(context.FunctionDefinition.EntryPoint);
            return context.IsInRole(requiredRoles);
        }

        private static IEnumerable<string> GetRolesFromAttribute(string entryPoint)
        {
            var functionType = entryPoint[..entryPoint.LastIndexOf('.')];
            var t = Assembly.GetEntryAssembly().GetType(functionType);
            var functionMethod = entryPoint[(entryPoint.LastIndexOf('.') + 1)..];
            var runMethod = t.GetMethod(functionMethod);
            if (Array.Find(runMethod.GetCustomAttributes(false), a => a is RequireRoleAttribute) is not RequireRoleAttribute roleAttribute)
            {
                return null;
            }
            return roleAttribute.Roles.Split(',');
        }
    }
}