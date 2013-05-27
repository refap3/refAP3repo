Steps to reproduce
----------------------

x install SQLexpress or SQL Server 
x add IIS Role (with IIS 6 Metabase and IIS 6 Configuration Compatibility !!) 
  i had to: [C:\Windows\Microsoft.NET\Framework\v4.0.30319]aspnet_regiis -i (otherwise script content would not be served) 
x install WCF REST Service Template 40 (CS) in VS 2010 

x new Solution/Project 
  WCF Rest Service Application 
  Windows Form Application (Test Client) 
  .net Gadgeteer Application 4.2


x in WCF Rest Service Application 
  delete SampleItem Class
  in Service1 Class delete all methods but POST 
  change code for POST method ...
  add pics folder to web site 
  add pics display page (copy 3 files and D&D to solution) 
  note that the STREAM with POST needs to be reset to copy to 2 loctions (file & db)  


  publish Website to local IIS: 
  set this in publish/package settings: items to depoloy: all files in this project folder
  add 4.0 Application Pool (integrated) 
  change user to fhs (or whatever needed for windows integrated security DB access !!)
  change Default Web Site (adv settings) to Application  4.0 pool 
  with WebDeploy // localhost // Default Web Site/gadg
  must give user App Pool 4 (or whatever) FULL CONTROL on pics folder (create and delete) 

  add BankingComponents Transfer Code ... BEWARE SQL Server access is thru App Pool User !! (see above) 


x  Windows Form Application (Test Client) 
   add button1 & client code
   create c:\temp\aa.bmp , bb.bmp (large& small)  ARE in solution directory !




x .net Gadgeteer Application 4.2
   copy and D&D to Solution 
   adapt service URIs 
   add DNS resolution for used FQDN (spider.hv.internal) 


x SQL Server DB 
  create new Data Connection & new Database 
  create appropriate table in DB (TData) 
  there is a TData.sql script in Solution Directory 
  run it with sqlcmd -S .[\sqlexpress] -i TData.sql


HHTP POST will crash when posting more than 64k DATA!
solution: add last attribute in web.config file: 

<standardEndpoint name="" helpEnabled="true" automaticFormatSelectionEnabled="true" maxReceivedMessageSize="33554432"/>


x OH MY GAD !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
-------------------------------------------------

 if you ever get the CLR_E_FAIL error when copying between machines 
 DELETE bin and object directories in gadgeteer project 
 what a F.CK!








