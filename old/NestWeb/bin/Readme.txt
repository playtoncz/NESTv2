BusyBoxDotNet QuickStart guide
-------------------------------

- Include the webcontrols in you pages by importing them in Visual Studio toolbox and dragging them or by handwriting code.

- Set the doctype of the page to be at least XHTML 1.0 transitional:

	<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 	"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

- Add this HttpHandler declaration to your web.config's <system.web> section

	<system.web> 
	...

	<httpHandlers>
      		<add verb="*" path="BusyBoxDotNet.axd" 
      		type="BusyBoxDotNet.ResourceHttpHandler, BusyBoxDotNet" />
    </httpHandlers>

	...
	</system.web>
    

- Check out the quickstart guide at http://busybox.sourceforge.net/quickstart.html