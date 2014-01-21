Steps to get this project running:

  - Make sure IIS Express is clean by deleting the IIS folder (Default location is Documents)
  - Open the solution in Visual Studio let it create the Virtual Directory
  - Right click on CustomSTS and click properties, set "Use Local ISS Web server" path to http://localhost:8090/customsts/
    (Make sure the "Use IIS Express" flag is set)
  - Select the CustomSTS project and look at the "Properties" window, "SSL Enabled" should be set to true.
  - Set CustomSTS as start-up project and run. (If the project does not build due to missing IdentityModel reference, download "Windows Identity Foundation" from http://www.microsoft.com/en-us/download/details.aspx?id=17331

Optional:
  - Right click on CustomsSTS.SampleRP and click properties, set "Use Local ISS Web server" path to http://localhost:8091/
 
Accessing the site from remote:

CustomSTS:
 
 1. Run "netsh http add urlacl url=https://*:44300/ user=Everyone" as administrator
    - Please note that group names are localized as well when using Windows in a foreign language (e.g. "Everyone" becomes "Alle" for Danish).
 2. Open IISExpress config file and set bindingInformation on site to "*:44300:"

CustomSTS.SampleRP:

 1. Run "netsh http add urlacl url=http://*:8091/ user=Everyone" as administrator
 2. Open IISExpress config file and set bindingInformation on site to "*:8091:"
 
Resolve issues:
  - Make sure the project file is configured correctly, ie. the "Use Local ISS Web server" should not be an https path
  - Close Visual Studio
  - Delete C:\Users\<user>\Documents\IISExpress and C:\Users\<user>\Documents\My Web Sites
  - Run "netsh http delete urlacl url=https://*:44300/"
  - Open Visual Studio again
  - Retry setup

 