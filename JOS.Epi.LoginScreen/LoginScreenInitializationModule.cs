using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Hosting;

namespace JOS.Epi.LoginScreen
{
	[InitializableModule]
	[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
	public class LoginScreenInitializationModule : IInitializableModule
	{
		private const int NumberOfStockImages = 3;

		public void Initialize(InitializationEngine context)
		{
			var hostingEnvironment = ServiceLocator.Current.GetInstance<IHostingEnvironment>();

			if (hostingEnvironment == null)
			{
				return;
			}

			var provider = new VirtualPathMappedProvider("JOS.LoginScreen", new NameValueCollection());
			MapBackgroundImages(hostingEnvironment, provider);
			MapLogo(hostingEnvironment, provider);
			hostingEnvironment.RegisterVirtualPathProvider(provider);
		}

		public void Uninitialize(InitializationEngine context)
		{
			//Add uninitialization logic
		}

		private static string GetImageFolderPath(IHostingEnvironment hostingEnvironment)
		{
			var relativePath = ConfigurationManager.AppSettings.Get("JOS.LoginScreen.ImageFolder") ??
			                   string.Format("Static{0}img{0}login", Path.DirectorySeparatorChar);
			return Path.Combine(hostingEnvironment.ApplicationPhysicalPath, relativePath);
		}

		private static void MapBackgroundImages(IHostingEnvironment hostingEnvironment, VirtualPathMappedProvider provider)
		{
			var imagesFolderPath = GetImageFolderPath(hostingEnvironment);
			var images = GetImages(imagesFolderPath).ToList();

			if (!images.Any())
			{
				return;
			}

			for (var i = 0; i < NumberOfStockImages; i++)
			{
				var image = images.Count > i ? images[i] : images.First();
				var imageUrl = UrlifyPath(image, hostingEnvironment.ApplicationPhysicalPath);
				var epiPath = $"/Util/images/login/Pictures_Page_{i + 1}-min.jpg";
				provider.PathMappings.Add(epiPath, imageUrl);
			}
		}

		private static void MapLogo(IHostingEnvironment hostingEnvironment, VirtualPathMappedProvider provider)
		{
			var imagesPath = GetImageFolderPath(hostingEnvironment);
		    if (Directory.Exists(imagesPath))
		    {
		        var logoPath = Directory.GetFiles(imagesPath, "epi_login_logo.svg").FirstOrDefault();

		        if (string.IsNullOrWhiteSpace(logoPath))
		        {
		            return;
		        }

		        var logoUrl = UrlifyPath(logoPath, hostingEnvironment.ApplicationPhysicalPath);
		        provider.PathMappings.Add("/Util/images/login/DXC_long.svg", logoUrl);
            }
		    else
		    {
		        Console.WriteLine($"Path {imagesPath} does not exists.");
            }
			
		}

		private static IEnumerable<string> GetImages(string imagesFolderPath)
		{
		    if (Directory.Exists(imagesFolderPath))
		    {
		        var extensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
		        return Directory.EnumerateFiles(imagesFolderPath, "*.*")
		            .Where(x => extensions.Contains(Path.GetExtension(x)))
		            .Take(NumberOfStockImages);
            }
            Console.WriteLine($"Path {imagesFolderPath} does not exists.");
		    return Enumerable.Empty<string>();
		}

		private static string UrlifyPath(string absoluteFilePath, string applicationPhysicalPath)
		{
			var imageUrl = absoluteFilePath
				.Replace(applicationPhysicalPath, string.Empty)
				.Replace(Path.DirectorySeparatorChar, '/');
			return string.Concat("/", imageUrl);
		}
	}
}