This is a quick description of kCura.IntegrationPoints.Contracts.Domain namespace modules/resources.

All modules here are directly or indirectly (as references) involved in AppDomain handling processes. 
The main reason they all have been assembled here under kCura.IntegrationPoints.Contracts.dll was as a quick and short term solution 
to deal with problem of loading the same types multiple times into the same AppDomain due to having instances 
of single dll vs IlMerged dlls (kCura.IntegrationPoints.Core.dll in domain1 vs. kCura.IntegrationPoints.dll in domain2) referenced 
during runtime of cross-AppDomain calls. As a result, such Dlls were loaded into the same AppDomain, 
and also was resulting in execution of the same Windsor Installers multiple times and as such, failing on a second pass, since references were already registered.

kCura.IntegrationPoints.Contracts.Domain modules/resources are for internal use only and should not be publicly advertised nor documented. 
The only reason some classes/interfaces under this namespace have public access modifier, is because it was impossible at the time when problem was identified, 
refactor all the code effected and minimize scope of references to these members.




As a reminder to myself:
All references involved in cross-AppDomain operations should reside in the same DLLs on both side of AppDomains. So, if I have the following class:

public class MyType1 : IMyInterface1
{
	public IMyInterface2 ExecuteMethod1(MyType2 param1, MyType3 param2)
	{
	}
	public IMyInterface3 ExecuteMethod1(MyType4 param1, MyType5 param2)
	{
	}
}

…, and I make a cross-AppDomain call:

var instance = domain2.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);

… all DLLs that contain IMyInterface1, IMyInterface2, IMyInterface3, MyType1, MyType2, MyType3, MyType4, MyType5 looks like will need to be loaded not only initially in the second AppDomain, 
but during the method call in the first one as well. I made this conclusion based on results during my testing (debugging) and watching corresponding resources/references. 
Unfortunately, I could not find corresponding online references and could not confirm this conclusion. 
It is important to note and remember that the main reason for initial issue was the fact that we were calling the method of a class from kCura.IntegrationPoints.Core.dll. 
However, AppDomain1 was loaded with kCura.IntegrationPoints.Core.dll and AppDomain2 was loaded with kCura.IntegrationPoints.dll (IlMerged dll). 
When we instantiated AppDomain2::Class1 inside of AppDomain1, kCura.IntegrationPoints.dll (IlMerged dll) assembly was also loaded into AppDomain1 (because of the way our ResolveAssembly is setup). 
Then, when WindsorContainer.Install was executed, the same Installers were run first from kCura.IntegrationPoints.Core.dll and then from kCura.IntegrationPoints.dll (IlMerged dll).

