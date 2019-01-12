namespace TelegramBot.Registrator.Db.Services

open Rezoom.SQL
open TelegramBot.Registrator.Db.Models
open System
open Rezoom.SQL.Synchronous


type private GetAllUsers = SQL<"
        select * from Users
    ">


type private SetUser = SQL<"""
        insert into Users 
        ( Surname, Name, Patronymic, DateOfBirth)
        values (@Surname, @Name, @Patronymic, @Birth)
    """>


type UserService() = 
    let context = new ConnectionContext()
    
    let getUserData(user: User) = (
        user.DateOfBirth,
        user.SurName,
        user.Name,
        user.Patronymic )

    let registrate user = 
        SetUser
            .Command(user |> getUserData)
            .Execute(context)

    let setTestData =
        let newUser = {
            SurName = "Yugami";
            Name = "Lite";
            Patronymic = "Kira";
            DateOfBirth = DateTime.Now    }
        registrate newUser

    let getAllUsers () =
        GetAllUsers.Command().Execute(context)
    
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

    member __.Registrate(user: User): int option =
        registrate user
        Some 0

    member __.GetUserById(idUser: int): User =
        let newUser = {
            SurName = "Yugami";
            Name = "Lite";
            Patronymic = "Kira";
            DateOfBirth = DateTime.Now    }
        newUser

    member __.Remove(idUser: int) =
        ()