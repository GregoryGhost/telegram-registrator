namespace TelegramBot.Registrator.Db.Models

open System

/// Модель данных о пользователе.
type User =
    {
        /// Фамилия.
        SurName: string
        /// Имя.
        Name: string
        /// Отчество.
        Patronymic: string
        /// Дата рождения.
        DateOfBirth: DateTime
    }