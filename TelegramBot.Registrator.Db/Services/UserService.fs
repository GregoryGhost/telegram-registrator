namespace TelegramBot.Registrator.Db.Services

open Rezoom.SQL
open TelegramBot.Registrator.Db.Models
open System
open Rezoom.SQL.Synchronous


type private GetAllUsers = SQL<"
        select * from Users
    ">


type private GetRegistratedUser = SQL<"""
        SELECT [Id]
        FROM Users
	    WHERE [Surname] = @Surname
            and [Name] = @Name
            and [Patronymic] = @Patronymic
            and [DateOfBirth] = @DateOfBirth
        LIMIT 1
    """>


type private SetUser = SQL<"""
        insert into Users 
        ( Surname, Name, Patronymic, DateOfBirth)
        values (@Surname, @Name, @Patronymic, @Birth);
        select scope_identity() as InsertedId;
    """>


type private SetUserForTelegramUser = SQL<"""
        INSERT INTO TelegramUsers
        (IdTelegramUser, IdUser)
        VALUES (@IdTelegramUser, @IdUser)
    """>

/// Сервис по работе с телеграммовскими пользовательскими данными.
type UserService() = 
    let context = new ConnectionContext()
    
    let getUserData(user: User) = (
        user.DateOfBirth,
        user.SurName,
        user.Name,
        user.Patronymic )

    let registrate (user: User): int = 
        let regUser = 
            SetUser
                .Command(user |> getUserData)
                .ExecuteScalar(context)
        regUser

    let getAllUsers () =
        GetAllUsers.Command().Execute(context)

    let getRegistratedUser user =
        GetRegistratedUser
            .Command(user |> getUserData)
            .ExecuteTryExactlyOne(context)
       
    let setUserForTelegramUser idTelegramUser idUser =
        SetUserForTelegramUser
            .Command(idTelegramUser, idUser)
            .Execute(context)
    
    let toUser(dbItem: GetAllUsers.Row) = {
            SurName = dbItem.Surname;
            Name = dbItem.Name;
            Patronymic = dbItem.Patronymic;
            DateOfBirth = dbItem.DateOfBirth }

    member __.PrintAllUsers = 
        let users = getAllUsers()
        printfn "<-- Users -->"
        for userDbItem in users do
            printfn "User: %A" (userDbItem |> toUser)

    /// Зарегистировать пользовательские данные для телеграмм пользователя.
    member __.Registrate(idTelegramUser: int, user: User): bool =
        let checkRegisteredUser = 
                let res = getRegistratedUser user
                Option.isSome res
        // TODO: добавить проверку есть ли связка
        //  телеграмм пользователь и пользовательские данные
        //  в таблице TelegramUsers.
        if checkRegisteredUser then
            false
        else
            let idUser =
                registrate user
            (idTelegramUser, idUser) ||> setUserForTelegramUser
            true

    member __.GetUserById(idUser: int): User =
        let newUser = {
            SurName = "Yugami";
            Name = "Lite";
            Patronymic = "Kira";
            DateOfBirth = DateTime.Now    }
        newUser

    member __.Remove(idUser: int) =
        ()