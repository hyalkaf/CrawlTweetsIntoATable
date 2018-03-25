
open System

exception NoTwitterCredentials of string
exception WrongTwitterCredentials of string    

module Twitter =
    open FSharp.Data.Toolbox.Twitter
    open System.Windows.Forms

    /// <summary>
    /// Authenticate twitter with a user account. Gives full API access
    /// </summary>
    /// <param name="key" type="string">Consumer Key</param>
    /// <param name="secret" type="string">Consumer Secret</param>
    /// <param name="accessToken" type="string">Access Token/Key</param>
    /// <param name="accessSecret" type="string">Access Secret</param>
    let auth key secret accessToken accessSecret = 
        let twitter = Twitter.AuthenticateAppSingleUser(key, secret, accessToken, accessSecret)
        twitter
       
    /// <summary>
    /// Get a certain number of tweet or max displayed. 
    /// Uses count and maxId of tweet though it can start from beginning by passing 
    /// zeros for both of these
    /// </summary>
    /// <param name="twitter" type="Twitter"></param>
    /// <param name="user" type="string"></param>
    /// <param name="count" type="int"></param>
    /// <param name="maxId" type="int64"></param>
    let getANumOfTweets (twitter: Twitter) (user: string) count maxId = 
        if count <> 0 && maxId <> 0L
        then 
            let ts = twitter.Timelines.Timeline(user, count, maxId)
            let data = [ for t in ts -> t.Text, t.Id ]
            let id = 
                if data.Length > 0
                then (data |> List.last |> (fun (_, id) -> id)) - 1L
                else 0L
            data |> List.map (fun (d, _) -> d), id
        else
            let ts = twitter.Timelines.Timeline(user)
            let data = [ for t in ts -> t.Text, t.Id ]
            let id = 
                if data.Length > 0
                then (data |> List.last |> (fun (_, id) -> id)) - 1L
                else 0L
            data |> List.map (fun (d, _) -> d), id
        

    /// <summary>
    /// Get Tweets using a screen name
    /// </summary>
    /// <param name="user" type="string">User's screen name</param>
    /// <param name="twitter" type="Twitter">Twitter Handler type</param>
    let rec getTweets (user: string) (twitter: Twitter) tweets maxID =
        let newTweets, id = getANumOfTweets twitter user 20 maxID
        match id with
        | 0L -> tweets  
        | _ -> getTweets user twitter (newTweets |> List.append tweets) id

[<EntryPoint>]
let main argv = 
    if argv.Length < 5
    then raise (NoTwitterCredentials("Arguments passed are not correct. You should provide a key, secret
    , access token, access secret and user for which you want to crawl"))
    else
        let key, secret, accessToken, accessSecret, user = argv.[0], argv.[1], argv.[2], argv.[3], argv.[4]
        let twitter = Twitter.auth key secret accessToken accessSecret
        let tweets = Twitter.getTweets user twitter [] 0L
        0 