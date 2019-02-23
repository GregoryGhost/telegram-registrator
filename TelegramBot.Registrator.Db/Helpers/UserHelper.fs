namespace TelegramBot.Registrator.Db.Helpers

module User = 
    open System
    open TelegramBot.Registrator.Db.Models

    let datePattern = "dd-MM-yyyy"

    /// Построить пользователя из исходных данных.
    let ofSourceData (surname: string) (name: string) (patronymic: string) (birthDate: DateTime): User =
        let user = 
            { SurName = surname
              Name = name
              Patronymic = patronymic
              DateOfBirth = birthDate }
            
        user

    let toString (user: User): string = 
        sprintf "ФИО=%s %s %s, Дата_рождения=%s" 
        <| user.SurName <| user.Name <| user.Patronymic
        <| user.DateOfBirth.ToString(datePattern)