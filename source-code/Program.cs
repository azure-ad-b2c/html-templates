using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace html_templates_downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            String[] htmlFiles = new String[] {
                // Classic
                "https://login.microsoftonline.com/static/tenant/default/unified.cshtml",
                "https://login.microsoftonline.com/static/tenant/default/idpSelector.cshtml",
                "https://login.microsoftonline.com/static/tenant/default/selfAsserted.cshtml",
                "https://login.microsoftonline.com/static/tenant/default/updateProfile.cshtml", //deprecated
                "https://login.microsoftonline.com/static/tenant/default/multifactor-1.0.0.cshtml",
                "https://login.microsoftonline.com/static/tenant/default/exception.cshtml",

                // Blue
                "https://login.microsoftonline.com/static/tenant/templates/AzureBlue/unified.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/AzureBlue/idpSelector.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/AzureBlue/selfAsserted.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/AzureBlue/multifactor-1.0.0.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/AzureBlue/exception.cshtml",

                // Gray
                "https://login.microsoftonline.com/static/tenant/templates/MSA/unified.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/MSA/idpSelector.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/MSA/selfAsserted.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/MSA/multifactor-1.0.0.cshtml",
                "https://login.microsoftonline.com/static/tenant/templates/MSA/exception.cshtml"
                 };

            foreach (string htmlFileURI in htmlFiles)
            {
                using (var client = new WebClient())
                {
                    // Build the local path
                    var localpath = htmlFileURI.Replace("https://login.microsoftonline.com/static/tenant/", "");
                    if (htmlFileURI.IndexOf("templates/") < 0)
                    {
                        // Add the templates to the classic temlate
                        localpath = "templates/" + localpath.Replace("default", "classic");
                    }

                    // Replace the cshtml file name to html
                    localpath = localpath.Replace("cshtml", "html");

                    // Add the app path
                    localpath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), localpath);

                    // Create the directory
                    if (!Directory.Exists(Path.GetDirectoryName(localpath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(localpath));
                    }

                    string reply = client.DownloadString(htmlFileURI);

                    // Get the resources (URIs)
                    MatchCollection mc = Regex.Matches(reply, @"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})");
                    foreach (Match m in mc)
                    {
                        var resourceLocalPath = m.Value.ToString();
                        var resourceRemotePath = m.Value.ToString();

                        // Skip external resources
                        if (!resourceLocalPath.StartsWith("https://login.microsoftonline.com"))
                            continue;

                        // Remove the domain name in the local path
                        resourceLocalPath = resourceLocalPath.Replace("https://login.microsoftonline.com/static/tenant/", "");

                        // Remove the last character
                        if (resourceRemotePath.EndsWith("\""))
                        {
                            resourceLocalPath = resourceLocalPath.Replace("\"", "");
                            resourceRemotePath = resourceRemotePath.Replace("\"", "");
                        }

                        if (resourceRemotePath.EndsWith(");"))
                        {
                            resourceLocalPath = resourceLocalPath.Replace(");", "");
                            resourceRemotePath = resourceRemotePath.Replace(");", "");

                        }

                        // Change the directory name for the legacy template
                        if (resourceLocalPath.IndexOf("templates/") < 0)
                        {
                            // Add the templates to the classic temlate
                            resourceLocalPath = "templates/" + resourceLocalPath.Replace("default", "classic");
                        }

                        // Add the src folder to the local resource file
                        resourceLocalPath = resourceLocalPath.Replace("templates/", "templates/src/");

                        // Add the app path to the local resource file
                        resourceLocalPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), resourceLocalPath);

                        if (!File.Exists(resourceLocalPath))
                        {
                            // Create the directory
                            if (!Directory.Exists(Path.GetDirectoryName(resourceLocalPath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(resourceLocalPath));
                            }

                            client.DownloadFile(resourceRemotePath, resourceLocalPath);

                            Console.WriteLine(m);
                        }
                    }

                    // Change the directory name for the legacy template
                    if (htmlFileURI.IndexOf("templates/") < 0)
                    {
                        reply = reply.Replace("https://login.microsoftonline.com/static/tenant/default/img/",
                                      "https://login.microsoftonline.com/static/tenant/templates/classic/img/");
                    }

                    // Remove the static tenant
                    reply = reply.Replace("/static/tenant/", "/");

                    // Add the src folder to the path
                    reply = reply.Replace("/templates/", "/templates/src/");

                    File.WriteAllText(localpath, reply);
                }
            }

            Console.ReadLine();
        }
    }
}
