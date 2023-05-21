namespace DungeonWarAPI.Middleware;

public static class UserAuthenticationExtensions
{
	public static IApplicationBuilder UseUserAuthentication(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<UserAuthentication>();
	}
}