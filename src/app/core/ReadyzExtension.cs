// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;

namespace CSE.WebValidate
{
    /// <summary>
    /// Registers aspnet middleware handler that handles /version
    /// </summary>
    public static class ReadyzExtension
    {
        // cached response
        private static readonly byte[] ResponseBytes = System.Text.Encoding.UTF8.GetBytes("ready");

        /// <summary>
        /// Middleware extension method to handle /healthz request
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseReadyz(this IApplicationBuilder builder)
        {
            // implement the middleware
            builder.Use(async (context, next) =>
            {
                // matches /version
                if (context.Request.Path.Value.Equals("/readyz", StringComparison.OrdinalIgnoreCase))
                {
                    // return the version info
                    context.Response.ContentType = "text/plain";
                    await context.Response.Body.WriteAsync(ResponseBytes).ConfigureAwait(false);
                }
                else
                {
                    // not a match, so call next middleware handler
                    await next().ConfigureAwait(false);
                }
            });

            return builder;
        }
    }
}
