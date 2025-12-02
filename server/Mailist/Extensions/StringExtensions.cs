namespace Mailist.Extensions;

public static class StringExtensions
{
	public static string Take(this string value, int count)
	{
		if (value.Length > count) 
			return value[0..count];
		else 
			return value;
	}
}
