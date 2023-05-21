using System.Security.Cryptography;
using System.Text;

namespace DungeonWarAPI;

public static class Security
{
	public static string GetNewSalt()
	{
		using (var randomNumberGenerator = RandomNumberGenerator.Create())
		{
			var saltValue = new byte[64];
			randomNumberGenerator.GetBytes(saltValue);

			return Convert.ToBase64String(saltValue);
		}
	}

	public static string CalcHashedPassword(string password, string salt)
	{
		var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
		using (var sha256Hash = SHA256.Create())
		{
			for (var i = 0; i < 2; i++) passwordBytes = sha256Hash.ComputeHash(passwordBytes);

			return Convert.ToBase64String(passwordBytes);
		}
	}

	public static string GetNewAuthToken()
	{
		using (var randomNumberGenerator = RandomNumberGenerator.Create())
		{
			var token = new byte[32];
			randomNumberGenerator.GetBytes(token);

			return Convert.ToBase64String(token);
		}
	}

	public static byte[] GetNewGUID()
	{
		var guid = Guid.NewGuid();

		return guid.ToByteArray();
	}
}