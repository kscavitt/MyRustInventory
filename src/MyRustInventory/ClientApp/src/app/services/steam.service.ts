import { Injectable, Inject, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MarketDataResponse } from '../interfaces/market-data-response';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { RustItemDto } from '../interfaces/rust-item-dto'
import { Observable } from 'rxjs/internal/Observable';
import { SettingsService } from './settings.service';
import { Router } from "@angular/router";
import { LocalStorageService } from './local-storage.service';
import { LoggerService } from './logger.service';

const initalRustItems: RustItemDto[] = [];
const key:string = 'steamId';
@Injectable({
  providedIn: 'root'
})

export class SteamService implements OnDestroy {

  public steamId$: BehaviorSubject<string> = new BehaviorSubject<string>('');
  public steamId: string = '';
  public totalTradableItems$: BehaviorSubject<number> = new BehaviorSubject<number>(0);
  public currentPercentage$: BehaviorSubject<number> = new BehaviorSubject<number>(0);
  private totalTradableItems: number = 0;
  private _sleepDuration: number = 1000;
  private rustItems: RustItemDto[] = [];
  private readonly items$: BehaviorSubject<RustItemDto[]> = new BehaviorSubject<RustItemDto[]>(initalRustItems);

  constructor(private router: Router,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private settingsService: SettingsService,
    private localStorageService: LocalStorageService,
    private loggerService: LoggerService) {
    console.log('starting SteamService');

    // try to get the steamId from local storaage
    this.steamId = localStorageService.getString(key);

    // subscribe to change in the steamId
    this.settingsService.steamId$.subscribe(id => {
     console.log('updated SteamService Ref for steamId: {' + id + '}');
       this.steamId = id;
    });
   }

   ngOnDestroy() {
    this.settingsService.steamId$.unsubscribe()
   }

  getInventory() {

    if(!this.steamId)
    {
      this.loggerService.logWarning('steamId required. redirecting to settings page.');
      this.router.navigate(['/settings']);
    }
    //this.notificationService.showInfo('Fetching Rust Inventory...', 'Fetching Data')
    this.http.get<RustItemDto[]>(this.baseUrl + 'Steam/GetInventory/' +this.steamId)
      .subscribe(receivedItems => {
        this.rustItems = receivedItems;

        this.items$.next(this.rustItems);

        // kick off fetching of the market data
        this.getMarketData();
      });
  }

  // get market data
  private async getMarketData(): Promise<void> {

    //this.notificationService.showInfo('Fetching Market Data...','Fetching Data');
    // null check
    if (this.rustItems && this.rustItems.length > 0) {

      // get only tradable items

      // filter to get only the tradable items
      let tradableItems = this.rustItems.filter(items => items.tradable === 1);

      // get the total number of tradable items
      this.totalTradableItems = tradableItems.length

      // broadcast the total tradable items count chagnge
      this.totalTradableItems$.next(tradableItems.length);

      // log market call
      this.loggerService.logInfo('staring market call for ' + this.totalTradableItems + ' tradable items.');

      // loop tradable inventory
      for (var i = 0; i < (this.totalTradableItems); i++) {

        // get the market hash name
        let marketHashName = tradableItems[i].marketHashName ?? '';

        // get the assitid
        let assetid = tradableItems[i].assetid;

        // create url base and URI encode the market hash name
        let url = this.baseUrl + 'Steam/GetMarketData/' + encodeURI(marketHashName) + '/' + tradableItems[i].assetid + '/1';

        // get the market data
        this.http.get<MarketDataResponse>(url).subscribe(result => {

          // declare market data from the results
          let marketData = result;

          // log market details
          this.loggerService.logInfo('found market details for ' + marketHashName + ' lowest price = ' + marketData.lowest_Price);

          // get from original arry
          // find index from main list
          var foundIndex = this.rustItems.findIndex(x => x.assetid == assetid);

          // update the orig list with the new price value
          this.rustItems[foundIndex].lowestPrice = marketData.lowest_Price;

          // log price to console
          console.log('set price data to  ' + marketData.lowest_Price);

        }, error => console.error(error));

        // log current process number
        console.log('processed ' + i + ' of ' + (this.totalTradableItems - 1));

        let currentPercentage = this.getCurrentPercentage(i, this.totalTradableItems);

        this.currentPercentage$.next(currentPercentage);
        // log sleep
        this.loggerService.logInfo('sleeping for  ' + this._sleepDuration + ' ms');

        //! NOTE: we have to sleep to avoid steams rate limiter which throws a 429 response
        await new Promise(f => setTimeout(f, this._sleepDuration))
      }
      this.currentPercentage$.next(100);
      this.loggerService.logInfo('completed market data lookup');
      this.items$.next(this.rustItems);
      //this.notificationService.showSuccess('Finished Collecting Market Data','Success(');
    }
  }

  getCurrentPercentage(a: number, b: number): number {
    let getPercent = Math.round((a / b) * 100) ;
    this.loggerService.logInfo('current percentage: ' + getPercent +'%');
    return getPercent;
  }

  get rustInventoryItems(): Observable<RustItemDto[]> {
    return this.items$.asObservable();
  }

  get totalInventoryItems(): Observable<number> {
    return this.totalTradableItems$.asObservable();
  }

  get total(): Observable<number> {
    return this.totalTradableItems$.asObservable();
  }

  get percentage(): Observable<number> {
    return this.currentPercentage$.asObservable();
  }
}

