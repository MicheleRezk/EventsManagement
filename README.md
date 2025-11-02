# Events Management Api

## Overview 
1. Create an ASP.Net core application.
2. It should have a relational database to persist event registrations.
3. There are two kinds of users
   a. An event creator who has to login
   b. An event participant, who can register for the event
4. The event creator
   a. Can create an event
   b. Can see all registrations for an event
5. The event participant
   a. Can see all events,
   b. Choose one event and fill the registration form for it
6. An event has the following fields
   a. Name
   b. Description
   c. Location
   d. Start time
   e. End time
7. A registration has the following fields
   a. Name
   b. Phone number
   c. Email address