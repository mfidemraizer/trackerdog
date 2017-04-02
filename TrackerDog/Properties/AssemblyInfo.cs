using Castle.Core.Internal;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TrackerDog;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

#if !NETSTANDARD1_3
[assembly: AssemblyTitle("TrackerDog")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("TrackerDog")]
#endif

[assembly: AssemblyCopyright("Copyright ©  2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2600fecf-ef1a-4156-9108-d6c713f704f9")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
#if !NETSTANDARD1_3
[assembly: AssemblyVersion("2.2.1.0")]
[assembly: AssemblyFileVersion("2.2.1.0")]
#endif

#if !SIGNEDRELEASE
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("TrackerDog.Test")]
[assembly: InternalsVisibleTo("TrackerDog.Test.DotNetCore")]
#endif

#if SIGNEDRELEASE
[assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]
[assembly: InternalsVisibleTo("TrackerDog.Test, PublicKey=" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("TrackerDog.Test.DotNetCore, PublicKey=" + AssemblyInfo.PublicKey)]
#endif

namespace TrackerDog
{
    public static class AssemblyInfo
    {
        public const string PublicKey = "0024000004800000940000000602000000240000525341310004000001000100711ff1e6e15f3184545868f7b0abd01ccb5b234b1619856502954c7acb47230cdf0ed001ea22e360da96bd02bf3fa7af0535718b983e0368eb9fd5d9c45f4d838610c1594eb9a9f08fd7bdffc0612816ebc3ba7251aa603e8a52c65eacc7ee04da36653a837c277dc961403f03826b635439171a8f2a8f12ebdf6872c6b99697";
    }
}