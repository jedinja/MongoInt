module ImproveApi.Token

open System

open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.IdentityModel.Tokens

let buildToken id =
    printfn "0"
    let issuer = "improve.api"
    let claims = [
        Claim(JwtRegisteredClaimNames.Sub, id, ClaimValueTypes.String, issuer);
    ]

    printfn "1"

    let secretKey = "VerySecretKeyase23423dq3ew3e23e23ewdqwerqw3akukuruku"
    let signingKey = SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
    let expiresHours = 8
    let (now: Nullable<DateTime>) =  Nullable<DateTime>(DateTime.UtcNow)
    let expireTime = Nullable<DateTime>(now.Value.Add(TimeSpan(0, expiresHours, 0, 0)))
    let jwt = JwtSecurityToken(issuer, "improve.api", claims, now, expireTime, SigningCredentials(signingKey, SecurityAlgorithms.HmacSha384 ))
    printfn "2"
    let jwtSecurityTokenHandler = JwtSecurityTokenHandler()

    printfn "3"
    let encodedJwt = jwtSecurityTokenHandler.WriteToken(jwt)
    printfn "4"
    encodedJwt

let validateAndGetUserId (token:string) =
    let jwtSecurityTokenHandler = JwtSecurityTokenHandler()

    let secretKey = "VerySecretKeyase23423dq3ew3e23e23ewdqwerqw3akukuruku"
    let signingKey = SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))

    let pars = TokenValidationParameters()
    pars.IssuerSigningKey <- signingKey
    pars.ValidAudience <- "improve.api"
    pars.ValidIssuer <- "improve.api"

    let (_, secToken) = jwtSecurityTokenHandler.ValidateToken(token, pars)

    let jwtToken = secToken :?> JwtSecurityToken
    jwtToken.Subject