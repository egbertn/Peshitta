
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
namespace peshitta.nl.Api.Helpers;
public class PathHelper(IWebHostEnvironment _env)
{
  /// <summary>
  ///  when we configure our app as a Windows service
  ///  the current directory will be C:\\Windows\\system32
  ///  The content root path should be like e:\service\myapp
  /// </summary>

  public string GetContentRootPath()
  {
    return _env.ContentRootPath;
  }
  /// <summary>
  /// returns a fully qualified path for copy/read operations
  /// e.g. ./app_data/media could be expanded to
  /// c:\\services\\service\\app_data\\media
  /// </summary>
  /// <param name="path"></param>

  public string ExpandPath(string path)
  {
    return ExpandPath(path, _env.ContentRootPath);
  }
  /// <summary>
  /// creates a relative path if it is fully qualified
  /// e.g. c:\services\service\app\app_data
  /// should become app_data
  /// </summary>
  /// <param name="path"></param>
  public static string EnsureRelativePath(string path)
  {
    return Path.IsPathFullyQualified(path) ? Path.GetFileName(path) : path;
  }
  /// <summary>
  /// retrieves fully qualified path for a given relative path
  /// e.g. "app_data", "c:\\services\someservice" would return "c:\services\someservice\app_data"
  /// ./app_data identical
  /// ../somepath, c:\\services\\someservice would return c:\\services\\somepath
  /// a fully qualified path is kept untouched
  /// e.g. c:\\somefolder\\lahdidah remains that
  /// note, does not create path
  /// </summary>
  /// <param name="path">e.g. "./app_data"</param>
  /// <param name="contentPath">cannot be null</param>
  public static string ExpandPath(string path, string contentPath)
  {
    if (string.IsNullOrEmpty(path))
    {
      return null;
    }
    if (string.IsNullOrEmpty(contentPath))
    {
      throw new ArgumentNullException(contentPath);
    }
    return !Path.IsPathFullyQualified(path) ?
         Path.GetFullPath(path, contentPath) :
         path;
  }
}
