using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
namespace peshitta.nl.Api.Helpers;
public static class DataMapper
{
public static string ExtensionMap(string fileName)
		{
			var result = MimeTypes.GetMimeType(fileName);
			if (string.IsNullOrEmpty(result))
			{
				result = "application/octet";
			}
			return result ;
		}	
}
