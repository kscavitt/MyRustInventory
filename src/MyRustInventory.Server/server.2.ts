
// const express = require('express');
// const  ReadLine = require('readline');
// const app = express();

// var rl = ReadLine.createInterface({
// 	"input": process.stdin,
// 	"output": process.stdout
// });
// var SteamCommunity = require('steamcommunity');
// var SteamID = SteamCommunity.SteamID;



// let community = new SteamCommunity();

// function login(accountName: string, password: string, authCode?: string, twoFactorCode?:string = '', captcha?: string): void {

//     var details = {
// 		"accountName": accountName,
// 		"password": password,
// 		"authCode": authCode,
// 		"twoFactorCode": twoFactorCode,
// 		"captcha": captcha
// 	}
//     community.login(details, (err, sessionID, cookies, steamguard) => {
//         console.log(details);
//         if(err) {
//             if(err.message == 'SteamGuardMobile') {
//                 rl.question("Steam Authenticator Code: ", function(code) {
//                    login(accountName, password, null, code);
//                 });
    
//                 return;
//             }
    
//             if(err.message == 'SteamGuard') {
//                 console.log("An email has been sent to your address at " + err.emaildomain);
//                 rl.question("Steam Guard Code: ", function(code) {
//                     this.login(accountName, password, code);
//                 });
    
//                 return;
//             }
    
//             if(err.message == 'CAPTCHA') {
//                 console.log(err.captchaurl);
//                 rl.question("CAPTCHA: ", function(captchaInput) {
//                     this.login(accountName, password, authCode, twoFactorCode, captchaInput);
//                 });

//                 console.log(err);
//                 return;
//             }
//         }
//         console.log("Logged on! sessionID = " + sessionID);
//     });
    
//     var sid = new SteamID('76561198012083287');
//     console.log(sid.toString()); // 76561198006409530
// };


// app.get('/',(req, res)=>{

// res.send('hello world..');
// });

// app.listen(3000, ()=>{
//    // console.log('starting api server');
// });


// rl.question("Username: ", function(accountName) {
// 	rl.question("Password: ", function(password) {
// 		this.login(accountName, password);
// 	});
// });
