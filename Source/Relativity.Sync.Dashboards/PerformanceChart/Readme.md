URL of web page: https://relativitysyncstorage.z19.web.core.windows.net/

How to renew token:
Open portal.azure.com
Navitgate to: Home > Subscriptions > PD - RIP RDC - MSDN - 01 > Resources > relativitysyncstorage > Security + networking > Shared access signature

Allowed Services: Table\
Allowes resource types: Object\
Allowed permissions: Read

Copy SAS token and paste it into `basic.html:10 SAS_TOKEN`\
In Visual Studio Code install extension: Azure Storage.\
On Activity bar, click Azure icon.\
Login into our account for that extension.\
Hover Storage title bar > click Deploy to Static Website via Azure Storage\
Navigate to `relativitysync\Source\Relativity.Sync.Dashboards\PerformanceChart\`\
Pick PD - RIP RDC MSDN - 01 Subscription,\
Storage Account: `relativtysyncstorage`\
Confirm with `Delete and Deploy` button  
