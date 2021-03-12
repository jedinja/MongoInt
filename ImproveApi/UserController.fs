module ImproveApi.UserController

open BCrypt.Net
open ImproveApi.Domain
open ImproveApi.Models
open ImproveApi

open MongoInt.MongoEntityFactory
open Suave
open Suave.Filters
open Suave.Operators
open Response

let private encryptPass (user: UserForLogin) : User =
    { Password = BCrypt.EnhancedHashPassword user.Password
      Name = user.Name }

let private verifyPass (model: Entity<User>) (user: UserForLogin) =
    let dbUserOpt = model.all { Name = user.Name } |> List.tryHead
    match dbUserOpt with
    | None -> Mid.BAD
    | Some dbUser ->
        match BCrypt.EnhancedVerify(user.Password, dbUser.Record.Password) with
        | true -> Mid.SUCCESS
        | false -> Mid.BAD


let private createUser (model: Entity<User>) =
    Mid.requestBody >>
    Mid.FromJSON<UserForLogin> >>
    encryptPass >>
    model.create >>
    Mid.JSON

let private loginUser (model: Entity<User>) =
    Mid.requestBody >>
    Mid.FromJSON<UserForLogin> >>
    verifyPass model

let create factory =

    let UserModel = getUserModel factory
    let create = createUser UserModel

    choose [
        POST >=> request create
    ]