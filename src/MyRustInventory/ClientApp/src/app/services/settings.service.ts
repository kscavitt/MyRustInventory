import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { Settings } from '../interfaces/settings';
import { LocalStorageService } from './local-storage.service';
import { LoggerService } from './logger.service';

const settings: Settings = { steamId: ''};
const key: string = 'steamId';
@Injectable({

  providedIn: 'root'
})

export class SettingsService {
  public steamId$: BehaviorSubject<string> = new BehaviorSubject<string>('');
  constructor(private router: Router,
    private localStorageService: LocalStorageService,
    private loggerService: LoggerService) {

    // get from local storage
    let steamId: string  = this.localStorageService.getString("steamId");
    if(steamId)
     this.setSteamId(steamId);
   }

 steamId(): Observable<string> {
    return this.steamId$.asObservable();
  }

  setSteamId(newSteamId:string) {

    this.loggerService.logInfo('recieved steamId ' + newSteamId);
    this.steamId$.next(newSteamId);

    // add key to local service
    this.localStorageService.setString(key, newSteamId);
    this.router.navigate(['/']);
  }
}
