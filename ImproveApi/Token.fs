module ImproveApi.Token

open System

open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open Microsoft.IdentityModel.Tokens

let private secretKey = "VerySecretKeyase23423dq3ew3e23e23ewdqwerqw3akukuruku"
let private signingKey = SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
let private expiresHours = 8
let private issuer = "improve.api"
let private audience = "improve.api"

let Build id =
    let claims = [
        Claim(JwtRegisteredClaimNames.Sub, id, ClaimValueTypes.String, issuer);
    ]

    let (now: Nullable<DateTime>) =  Nullable<DateTime>(DateTime.UtcNow)
    let expireTime = Nullable<DateTime>(now.Value.Add(TimeSpan(0, expiresHours, 0, 0)))
    let jwt = JwtSecurityToken(issuer, audience, claims, now, expireTime, SigningCredentials(signingKey, SecurityAlgorithms.HmacSha384 ))

    JwtSecurityTokenHandler().WriteToken(jwt)

let ValidateAndGetUserId (token:string) =
    let jwtSecurityTokenHandler = JwtSecurityTokenHandler()

    let pars = TokenValidationParameters()
    pars.IssuerSigningKey <- signingKey
    pars.ValidAudience <- audience
    pars.ValidIssuer <- issuer

    try
        let (_, secToken) = jwtSecurityTokenHandler.ValidateToken(token, pars)

        let jwtToken = secToken :?> JwtSecurityToken
        Some jwtToken.Subject
    with
        | _ -> None