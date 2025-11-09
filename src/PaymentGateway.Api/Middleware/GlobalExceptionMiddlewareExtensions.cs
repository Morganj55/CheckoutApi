namespace PaymentGateway.Api.Middleware
{
    /// <summary>
    /// Provides extension methods for adding the <see cref="GlobalExceptionMiddleware"/> 
    /// to the application's request pipeline.
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        #region Extension Methods

        /// <summary>
        /// Adds the custom <see cref="GlobalExceptionMiddleware"/> to the specified <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <returns>The original <see cref="IApplicationBuilder"/> for chaining.</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionMiddleware>();

        #endregion
    }
}
