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


type private GetRegistratedUserById = SQL<"""
        SELECT * FROM Users
	    WHERE [Id] = @IdUser
        LIMIT 1
    """>


type private GetRegistratedUserByTelegramUser = SQL<"""
        SELECT [IdUser]
        FROM TelegramUsers
	    WHERE [IdTelegramUser] = @IdTelegramUser
        LIMIT 1
    """>


type private SetUser = SQL<"""
        insert into Users 
        ( Surname, Name, Patronymic, DateOfBirth)
        values (@SurName, @Name, @Patronymic, @DateOfBirth);
        select scope_identity() as InsertedId;
    """>


type private SetUserForTelegramUser = SQL<"""
        INSERT INTO TelegramUsers
        (IdTelegramUser, IdUser)
        VALUES (@IdTelegramUser, @IdUser)
    """>


type private RemoveUserForTelegramUser = SQL<"""
        DELETE FROM Users
        WHERE [Id] IN (SELECT [IdUser]
            FROM TelegramUsers
            WHERE [IdTelegramUser] = @IdTelegramUser);
    """>

/// Сервис по работе с телеграммовскими пользовательскими данными.
type UserService() = 
    let context = new ConnectionContext()

    let getUserData(user: User) = (
        user.DateOfBirth,
        user.Name,
        user.Patronymic,
        user.SurName )

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

    let getRegisratedUserById idUser =
        GetRegistratedUserById
            .Command(idUser)
            .ExecuteTryExactlyOne(context)

    let getRegistratedUserByTelegramUser idTelegramUser =
        GetRegistratedUserByTelegramUser
            .Command(idTelegramUser)
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

    let fromUserRecord(dbItem: GetRegistratedUserById.Row) = {
            SurName = dbItem.Surname;
            Name = dbItem.Name;
            Patronymic = dbItem.Patronymic;
            DateOfBirth = dbItem.DateOfBirth }

    member __.PrintAllUsers = 
        let users = getAllUsers()
        printfn "<-- Users -->"
        for userDbItem in users do
            printfn "User: %A" (userDbItem |> toUser)

    /// Зарегистрировать пользовательские данные для телеграмм пользователя.
    member __.Registrate(idTelegramUser: int, user: User): bool =
        let checkRegisteredUser() = 
            let res = 
                getRegistratedUser user
            let registeredTelegramUser = 
                getRegistratedUserByTelegramUser idTelegramUser
            let isRegistrated = 
                Option.isSome res || Option.isSome registeredTelegramUser
            isRegistrated
        if checkRegisteredUser() then
            false
        else
            let idUser =
                registrate user
            (idTelegramUser, idUser) ||> setUserForTelegramUser
            true

    /// Получить пользовательские данные по идентификатору телеграмм пользователя.
    member __.GetUserById(idTelegramUser: int): User option =
        let idUser =
            idTelegramUser
            |> getRegistratedUserByTelegramUser
            |> Option.bind (fun x -> Some x.IdUser)
        let user = 
            idUser
            |> Option.bind (fun x -> getRegisratedUserById x)
            |> Option.bind (fun x -> x |> fromUserRecord |> Some)
        user
    
    /// Удалить пользовательские данные по идентификатору телеграмм пользователя.
    member __.Remove(idTelegramUser: int) =
        RemoveUserForTelegramUser
            .Command(idTelegramUser)
            .Execute(context)