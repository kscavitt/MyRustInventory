import { Injectable } from '@angular/core';
import { LogMessageType } from '../enums/log-message-type';

@Injectable({
  providedIn: 'root'
})
export class LoggerService {

  constructor() { }

  log(logMessageType: LogMessageType, message: string): void {

    switch (logMessageType) {
      case LogMessageType.Info:
        this.logInfo(message);
        break;
      case LogMessageType.Debug:
        this.logDebug(message);
        break;
      case LogMessageType.Warning:
        this.logWarning(message);
        break;
      case LogMessageType.Error:
        this.logError(message);
        break;
      default:
        this.logInfo(message);
        break;
    }
  }

  logInfo(message: string): void {
    console.info(message);
  }

  logDebug(message: string): void {
    console.debug(message);
  }

  logWarning(message: string): void {
    console.warn(message);
  }

  logError(message: string): void {
    console.error(message);
  }
}
