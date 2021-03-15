module ImproveApi.UserController

open BCrypt.Net
open ImproveApi.Domain
open ImproveApi.Models
open ImproveApi

open MongoInt.MongoEntityFactory
open Suave

let private encryptPass (user: UserForLogin) : User =
    { Password = BCrypt.EnhancedHashPassword user.Password
      Name = user.Name }

let private createUser (model: Entity<User>) =
    Mid.requestBody >>
    Mid.FromJSON<UserForLogin> >>
    encryptPass >>
    model.create >>
    Mid.JSON

let create :Factory->WebPart = getUserModel >> createUser >> request

let private getUser (model: Entity<User>) =
    model.get >>
    Mid.JSON

let me db =
    context
        (Mid.getStringSessionValue LoginController.SESSION_USER >> getUser (getUserModel db))