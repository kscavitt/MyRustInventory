import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {

  constructor() { }

  public setItem(key: string, data: any): void {
    localStorage.setItem(key, JSON.stringify(data));
  }
  public setString(key: string, data: string): void {
    localStorage.setItem(key, data);
  }

  public getItem(key: string): any {
    let json: string = localStorage.getItem(key) ?? '{}';
    return JSON.parse(json);
  }
  public getString(key: string): string {
    return localStorage.getItem(key) ?? ''
  }

  public removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  public clear() {
    localStorage.clear();
  }
}
