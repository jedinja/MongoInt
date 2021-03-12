module ImproveApi.LoginController

open BCrypt.Net
open ImproveApi.Domain
open ImproveApi.Models
open MongoInt.MongoEntityFactory

open Suave
open Suave.Filters
open Suave.Operators

let private verifyPass (model: Entity<User>) (user: UserForLogin) =
    let dbUserOpt = model.all { Name = user.Name } |> List.tryHead
    match dbUserOpt with
    | None -> Mid.BAD
    | Some dbUser ->
        match BCrypt.EnhancedVerify(user.Password, dbUser.Record.Password) with
        | true -> { Token = "asd" } |> Mid.JSON
        | false -> Mid.BAD

let private loginUser (model: Entity<User>) =
    Mid.requestBody >>
    Mid.FromJSON<UserForLogin> >>
    verifyPass model

let login factory =

    let UserModel = getUserModel factory
    let login = loginUser UserModel

    choose [
        POST >=> request login
    ]