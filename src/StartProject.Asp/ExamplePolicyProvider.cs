// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace StartProject.Asp;

internal sealed class ExamplePolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName) => await base.GetPolicyAsync(policyName) ??
            new AuthorizationPolicyBuilder().RequireClaim("Permission", policyName).Build();
}
