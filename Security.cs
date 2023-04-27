using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;

namespace firstAPI
{
	public static class Security
	{

		public static String GetSalt()
		{
			using (var randomNumberGenerator = RandomNumberGenerator.Create())
			{
				byte[] saltValue = new byte[64];
				randomNumberGenerator.GetBytes(saltValue);

				return Convert.ToBase64String(saltValue);

			}
		}

		public static String GetHashedPassword(String password, String salt)
		{
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
			using (var sha256Hash = SHA256.Create())
			{
				for (int i = 0; i < 2; i++)
				{
					passwordBytes = sha256Hash .ComputeHash(passwordBytes);
				}

				return Convert.ToBase64String(passwordBytes);
			}
		}

		public static String GetToken()
		{
			using (var randomNumberGenerator = RandomNumberGenerator.Create())
			{
				byte[] token = new byte[32];
				randomNumberGenerator.GetBytes(token);

				return Convert.ToBase64String(token).Replace('+', '-')
					.Replace('/', '_')
					.TrimEnd('=');
			}
		}
	}
}
