module ImproveApi.Domain

open MongoDB.Bson.Serialization.Attributes

[<BsonIgnoreExtraElements>]
type User = {
    Name: string
    Password: string
}

type UserForLogin = {
    Name: string
    Password: string
}

type UserFilterByName = {
    Name: string
}

type AuthenticationToken = {
    Token: string
}

