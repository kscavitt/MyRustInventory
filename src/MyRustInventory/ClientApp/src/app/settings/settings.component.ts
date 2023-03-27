import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { LocalStorageService } from '../services/local-storage.service';
import { SettingsService } from '../services/settings.service';

const key: string = 'steamId';
@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {
  public steamId: string = '';

  constructor(private settingsService: SettingsService, private localStorageService: LocalStorageService) { }

  ngOnInit(): void {
    // try to get the steam key from the local strorage
    let id = this.localStorageService.getString(key);
    if (id) {
      this.steamId = id;
    }
  }

  saveClicked(): void {
    console.log('seding ' + this.steamId);
    this.settingsService.setSteamId(this.steamId);
  }
}
