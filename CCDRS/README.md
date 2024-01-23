# CCDRS.Net

The 2022 re-implementation of CCDRs

# Building CCDRS

- Visual Studio 2022 with 17.4.0+
- PostgreSQL version 14: We use the relational database PostgreSQL to store the data
- PgAdmin: A windows GUI application tool that can connect to PostgreSQL. A useful tool to run queries and see the table relationships and displays errors, an alternative to writing queries in a terminal. 
- - MongoDB: This is only needed for local development to store the user name and passwords. In production mode there is no need to install MongoDB on the production or test machines.
- Asp.Net Core Razor Web App: We are using Razor pages to build the front end. 
- Ubuntu22 WSL2 VM Machine: On Windows 10 you will need to install Ubuntu 22 with WSL2 enabled. 
 

## Setting up Development Environment 

Note that these instructions are for setting up the system in Windows 10.


### Install and Setup Ubuntu WSL2

WSl2 is the Windows Subsystem for Linux to run Linux binaries on Windows. Ubuntu 22 is the operating system used for installing the PostgreSQL and MongoDB databases and for deployment.
Please refer and follow the steps below in the link below on how to install Ubuntu for WSL2.
https://ubuntu.com/tutorials/install-ubuntu-on-wsl2-on-windows-10#1-overview

In some case you may need to upgrade WSL1 to WSL2 by installing the WSL2 kernel package as posted in the link below in the section.
This needs to be done otherwise MongoDB won't install and run properly. 
Note to install the latest version of Ubuntu which is version Ubuntu 22. 
- https://learn.microsoft.com/en-us/windows/wsl/install#upgrade-version-from-wsl-1-to-wsl-2
- https://learn.microsoft.com/en-us/windows/wsl/install-manual#step-4---download-the-linux-kernel-update-package

Open a terminal and switch to Ubuntu to open an Ubuntu terminal. To change directories inside the Windows machine
use ==/mnt/c==.

### Install and Setup PostgreSQL

Follow the links to install the latest version of PostgreSQL on the Ubuntu 22 system
https://www.postgresql.org/download/linux/ubuntu/

#### Start and check PostgreSQL

Check the status of PostgreSQL to see if the PostgreSQL service is enabled and active
`sudo service postgresql status`

If the result displayed is `14/main (port 5432): down` then run the following command to 
enable and start the service:

`sudo service postgresql start`

#### Change PostgreSQL user password

By default, PostgreSQL comes with a default admin user account called postgres however a password needs to be assigned to the default user.
This is required otherwise the Asp Net core connection strings which connect to the database will fail to work. 
- In your Linux terminal run the command `sudo -i -u postgres` to switch to the postgres account
- Run the command `psql` to access posgres inside your Linux terminal 
- Inside the postgres terminal write the following command 

`ALTER USER postgres PASSWORD 'myPassword’;` where myPassword is the password you choose to add. Remember to add the semicolon at the end or the command will fail to load.
- Exit the psql client by writing `\q` in the terminal. 

#### Install PgAdmin

Go to the main site of PgAdmin https://www.pgadmin.org/ and under the Download section download the version of PgAdmin required.
https://www.pgadmin.org/download/


### Install and setup MongoDB

 MongoDB is being used for user authentication. This database stores the username and password and setup similar to the production system currently being 
 used and implemented. This step is required if the developer is working on code associated with user authentication otherwise
 it can be skipped. 

 Follow the steps on the official MongoDB website on installing and setting up MongoDB
 https://www.MongoDB.com/docs/manual/tutorial/install-MongoDB-on-ubuntu/

#### Start and check MongoDB Service

 - run the command `$ sudo service MongoDB status` to check the status of the service
 - if it displays ` * Checking status of database MongoDB  [fail]` enable the service
 - run the command `sudo service MongoDB start` to enable the PostgreSQL service 
 - once the service is successfully running in the same terminal run the command `mongo` to run the service. Keep this terminal open
 - Open a second Ubuntu terminal and type in `mongo` to access the mongo shell
 
#### Access Mongo shell to create database and single user entry

 - We need to create one database. inside the opened mongo shell type `use myshinynewdb` where myshinynewdb is the name
 of the database you wish to use. In the development environment it is called myshinynewdb.
 - Now create a collection with the single data entry of the username and password. This information will be 
provided by DMG upon request

`db.createCollection( Usertable,
   {
     username: <string>,
     password: <string>,
   }
)`

You may need to restart the mongo service `sudo service MongoDB restart` to see the updated changes


### Setup Solution to run the project

1. Git clone the repo and open it in Visual Studio. 
1. Right click on the project label CCDRS and click on the setting **Manager User Secrets**
    1. User secrets is a file where database connection strings can be safely stored and not publicly shared without being displayed to users or pushed to GitHub
    1. in the User Secrets copy the following information to the system:

` {
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "TestDBContext": "Server=localhost;Port=5432;Database=ccdrsv1;User Id=postgres;Password=yourpassword"
  },
  "MongoDB": {
    "ConnectionString": "MongoDB://localhost:27017",
    "DatabaseName": "myshinynewdb",
    "UserCollectionsName": "UserTable"
  }
}
`
Where your password is the password to your postgres user account.
- save the secrets.json file and run the project by pressing F5 or Ctrl-F5. A successful load will display the login page.
- If you wish to disable login/authentication, open the file Program.cs
-  comment out lines 47-56. These lines add authorization to the pages so by commenting them out authorization 
is disabled on these pages.

`builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizePage("/Index");
        options.Conventions.AuthorizePage("/AllStation");
        options.Conventions.AuthorizePage("/AllScreenline");
        options.Conventions.AuthorizePage("/SpecificStation");
        options.Conventions.AuthorizePage("/SpecificScreenline");
    }
);
`

## Setting up Production Environment

- using Visual studio publish the project using the following settings:
- Target Framework: net 7.0
- Deployment mode: self-contained
- Target runtime: Linux-x64 or whichever target you need
- Delete all existing files prior to publish
- Click on publish and it produces a folder with the CCDRS.sh executable. 



## Database Scripts

The database scripts are Linux bash scripts that are responsible for destroying the database 
and rebuilding the database from scratch.
The files are located in the CCDRS folder. 

The main file is called `CreateDatabase.sh` which performs the following actions 
1. cleans and deletes all tables in the database
1. rebuild the table schemas using the create table command
1. insert data back into the tables. This is done by either inserting table directly 
or creating temporary tables read from csv or txt files and then insert the data 
into the original table
1. database cleanup, typically deleting all temporary tables that were created. 

For screenline data, a python script exists to transform the data into a version that 
PostgreSQL copy command can work with.

**Note that it is important that no duplicate data exists in the text and csv files
because the transferring of data will then crash. Also it is important to note that no rows
are left blank**


## Database Architecture

The database architecture is now third normalized and consists of eleven tables as shown in
the figure below.
Note that due to PostgreSQL convention, all table names are lower cases and all foreign
keys end with _id.
![Database Architecture Diagram](Img/dbArchitecture.jpg)

##### direction

The direction table lists the four directions available for the user to select. It stores the name
and the abbreviation of the direction e.g. 'North', 'N'. Note that the direction table is not 
associated or connected to any table, it is just used to display options to the front end.

#### region

The region table lists every region available a user can search data for. It is also one
of the main primary tables as this table contains no foreign keys to any other table.

#### survey

The survey table contains a list of all the years associated with a given region.
The survey table has two attributes the year stored as an integer and a foreign key 
called region_id which points to the primary key of the region table.

#### vehicle

The vehicle table lists all possible vehicle that are available.  
Examples of technologies are auto, bus, light truck. Note that the number of occupants
is not stored in this table. 

#### vehicle_count_type

The vehicle_count_type table describes the vehicles. The vehicle_count_type table 
stores the occupancy a given vehicle can occupy, a description of the vehicle, a 
count_type to determine the type of vehicle, and a foreign key vehicle_id which points 
to the primary key of the vehicle table. There is a unique constraint on the table where 
the vehicle_id and occupancy have to be unique.

#### station

The station data lists all the stations associated with a given region. 
The station data has the following attributes:
the station_code, direction, the station number associated with the station, and a
foreign key region_id associated with the primary key of the region table. 

#### survey_station

The survey_station table is a junction table of a many to many relationship associating 
the stations with the survey, the years a given station is associated to. 
The survey_station does have a unique constraint of the foreign key of station_id and
foreign key of survey_id both being unique. 

#### screeline

The screenline table lists all screenlines associated with a given region. The screenline
table has a unique constraint that each region and screenline code be unique. 

#### screenline_station

The screenline_station table is a junction many to many table that links each station to 
a given screenline

#### survey_count_observation

The survey_count_observation table stores the data for each fifteen-minute time period for every
recorded observation for a given station. The station has two foreign keys a foreign
key called survey_Station_id associated to the survey_station so we can find the user selected
stations or screenlines, and a foreign key to the vehicle_count_type table to determine
the user selected vehicles. The time is stored as an integer and starts at 600 with 
fifteen-minute interval increases. 

#### individual_categories

The individual_categories table is a table/view that is used by the ASP.Net Entity
Framework Core to generate a list of all vehicles and their associated occupancy levels 
with a human readable description. For example, 'light truck with 1 occupancy'.

# CCDRS ManagerTool

# Developer Notes