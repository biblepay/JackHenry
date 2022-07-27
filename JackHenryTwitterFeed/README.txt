README
======

1.  Before running the program, please populate your Twitter Bearer Token (112 bytes long) in the appsettings.json file in two places under the key "TwitterCredentialsBearer":
1a. Populate in the Web Project:  JackHenryTwitterFeed/appsettings.json
1b. Populate in the NUnit Test Project:  JackHenryTwitterFeedTests/appsettings.json

2.  There are three projects in the solution:  The web project, the test case project, and the common business logic (class library) project.
2b.  To launch the solution in the VS IDE, find the solution file in the JackHenryTwitterFeed folder (JackHenryTwitterFeed.sln) and launch that file.
     That will load the entire solution.

3.  Architecture:
In the class library, we have one class that is a singleton:  JackHenryTwitterService.
The class is designed to be instantiated once in either the Web project, or from any other project, or from the Test Project.
Once instantiated, it runs in one background thread as a service.  If it encounters an error, it will sleep for one minute,
then recover, by restarting the Twitter HTTPS connection and resuming.
It maintains a reference to one static dictionary which contains the HashTag list, and, the Instance Count of each hashtag.
This class has three methods which will return the requirements:
GetTrendingHashTags - When a 10 is passed in, it will return the top 10 trending hashtags.  Optionally, you may pass a higher number.
GetTotalTweetsReceived - The number of tweets processed.
(BONUS) : GetStorageCount - The distinct number of tweets stored.

4.  Exception Handling
The web project intentionally does not have any error handling as the goal is to allow the error to propagate (if one is discovered).
In the service loop, the twitter HTTPS connection is infinite (in duration), meaning as long as no network error occurs, that loop continues running, and does not throw an exception.
However, we have added an exception handler surrounding the service class loop, which handles any unexpected error, 
 and also handles the case of network connectivity dropping out.  
In this case, the loop will log the error to the console (which we will change to the file LOG in production), then it will pause for 60 seconds, then
recover (by reconnecting to twitter).

5. Unit Tests:
In the NUnit Test Project, we have two unit tests.  The first test, CanBeInstantiated, verifies the most basic instantiation of the class by checking its class release version.
The next test ServiceLoopRunning, this starts the service loop and runs in a busy wait loop (for up to 60 seconds) until it actually
receives Hashtags, then passes (or fails otherwise).

6.  SOLID principles:
The most important step in adhering to SOLID is that we place our TwitterService in its own class library (outside of the web project and outside of the test project).
This allows us to inject the JackHenryTwitterService object into any class in our domain, late bound, and test it.
It also allows us to keep it running in the background as a singleton, which keeps memory utilization efficient and processor utilization
efficient, and keeps our UI layer running without interference by our back-end processes.
This in turn keeps our component loosely coupled in our infrastructure so it is not tied too closely to our UI layer.

How do I test the Web Project?

To test the web project, set the Startup Project to be the JackHenryTwitterFeed project, then run it.
Note that all of the default web pages in this project are the default boilerplate aspnetcore pages included in the standard template.
Ignoring those, a few new simple REST GET API endpoints have been added to allow you to access the Twitter feed data:

From the browser, navigate to each of these URLs:
1.  localhost:44396/twitter/gettrendinghashtags
This endpoint responds with the top 10 hashtags sorted by descending popularity.  Each hashtag object includes both its name and the occurrence count.  The response is in JSON.
2.  localhost:44396/twitter/gethashtagswithlimit/nLimit
Replace nLimit with the desired tag limit.  This endpoint responds with the hashtags sorted descending, with a limit capped by the number specified in the URL.
3.  localhost:44396/twitter/gettotaltweetsreceived
This endpoint responds with the number of tweets processed by the service.
4.  localhost:44396/twitter/getstoragecount
Responds with the number of tweets stored in the dictionary.  This number is lower than total tweets received simply because it contains
the distinct tweet count.

How do I test the Test project?

To test the test project, right click on the project and click Run tests.
(Be sure to have the bearer token populated in the appsettings.json in the test project, as it is a different file).



