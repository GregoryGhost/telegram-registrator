create table Users
( 
	Id int primary key autoincrement,
	Surname string(256), 
	Name string(128),
	Patronymic string(128),
	DateOfBirth datetime,
	unique (Surname, Name, Patronymic, DateOfBirth)
);

create table TelegramUsers
(
	IdTelegramUser int primary key,
	IdUser int,
	foreign key (IdUser) references Users (Id)
);