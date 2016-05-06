# Scrape Gmail

### Summary

This application is designed to login to any Google Mail account and scrape the data from the Sent folder to send to a Node.js Server.  The Node.js server then takes the data and sends it to an API, receives the return and sends it back to the Scrape Gmail application.

------

### Approach Taken

Initially this application was only supposed to be a Server that responded with the email body.  However, after working through the project I decided to build an ASP.NET MVC application that was able to display items on the DOM for users to see.

------

### Technologies Used

- HTML5
- CSS3
- C# & .NET
- Javascript
- jQuery
- D3.js
- Razor
- MVC
- Bootstrap
- OAUTH

------

### Unsolved Issues/Future Plans

- My plan is to deploy this into a future project with a more business centric front end.  The current deployment would be stripped of bootstrap and another Javascript Framework would be used.
- Need to setup a database to store data from each E-mail Scrape.  The preferred method would be use MSSQL but MongoDB will be considered for speed and ease of use.
- Currently all the loading and scraping is done through one route in the HomeController.  Ideally OAUTH would be one route and after the login is complete the token would be used to access the route that does the work.

