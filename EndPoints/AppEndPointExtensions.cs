namespace dotnet_api_extensions.EndPoints
{
    /// <summary>
    /// Adds end points dynamically.
    /// </summary>
    public static class AppEndPointExtensions
    {
        /// <summary>
        /// Adds dynamic end point for specified path with provided middleware code.
        /// </summary>
        /// <typeparam name="TEndPointMiddleware">
        /// Either it should implement IEndPointMiddleware or has either of two methods "public Task &lt;object&gt; GetResultAsync() and "public object GetResult()""
        /// </typeparam>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IApplicationBuilder AddEndPoint<TEndPointMiddleware>(this IApplicationBuilder app, string path)
        {
            return app.MapWhen(GetPathPredicate(SanitizePath(path)), EndPointExecuter<TEndPointMiddleware>(path));
        }

        /// <summary>
        /// Adds dynamic end point for specified path predicate with provided middleware code.
        /// </summary>
        /// <typeparam name="TEndPointMiddleware">
        /// Either it should implement IEndPointMiddleware or has either of two methods "public Task &lt;object&gt; GetResultAsync() and "public object GetResult()""
        /// </typeparam>
        /// <param name="app"></param>
        /// <param name="pathPredicate">This predicate should take httpContext as parameter and return tuple of (bool Passed, string Path) as response</param>
        /// <returns></returns>
        public static IApplicationBuilder AddEndPoint<TEndPointMiddleware>(this IApplicationBuilder app, Func<HttpContext, (bool Passed, string Path)> pathPredicate)
        {
            return app.Use(next =>
            {
                return async context =>
                {

                    var (Passed, Path) = pathPredicate(context);
                    if (Passed)
                    {
                        // create branch to get a new application pipeline
                        //Idea taken from: https://github.com/dotnet/aspnetcore/blob/master/src/Http/Http.Abstractions/src/Extensions/MapWhenExtensions.cs

                        var branchBuilder = app.New();
                        EndPointExecuter<TEndPointMiddleware>(Path)(branchBuilder);
                        var branch = branchBuilder.Build();

                        await branch.Invoke(context);
                    }
                    else
                    {
                        await next.Invoke(context);
                    }
                };
            });
        }

        private static Action<IApplicationBuilder> EndPointExecuter<TEndPointMiddleware>(string path)
        {
            return app =>
            {
                app.Run(
                    async context =>
                    {
                        var endPointMiddlewareHandler = context?.RequestServices.GetService<EndPointMiddlewareHandler<TEndPointMiddleware>>();
                        if (endPointMiddlewareHandler != null)
                        {
                            await endPointMiddlewareHandler.HandleRequest(context, path);
                        }
                    }
                    );
            };
        }

        private static Func<HttpContext, bool> GetPathPredicate(string path)
        {
            return (c) =>
            {
                // We allow you to listen on all URLs by providing the empty PathString.
                return string.IsNullOrEmpty(path) ||

                    // If you do provide a PathString, want to handle all of the special cases that
                    // StartsWithSegments handles, but we also want it to have exact match semantics.
                    //
                    // Ex: /Foo/ == /Foo (true)
                    // Ex: /Foo/Bar == /Foo (false)
                    c.Request.Path.StartsWithSegments(path, out var remaining) &&
                    string.IsNullOrEmpty(remaining);
            };
        }

        private static string SanitizePath(string path)
        {
            return path.StartsWith("/") ? path : "/" + path;
        }
    }
}