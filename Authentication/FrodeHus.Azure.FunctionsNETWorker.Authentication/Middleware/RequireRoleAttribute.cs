using System;
using Microsoft.Azure.Functions.Worker;

namespace FrodeHus.Azure.FunctionsNETWorker.Authentication
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute
    {
        public RequireRoleAttribute(string roles) : base()
        {
            Roles = roles;
        }

        public string Roles { get; }
    }
}