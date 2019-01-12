create table Users
( 
	Id int primary key autoincrement,
	Surname string(256), 
	Name string(128),
	Patronymic string(128),
	DateOfBirth datetime
);