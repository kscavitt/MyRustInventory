import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RustItemsResponse} from '../interfaces/rust-items-response';
import { MarketDataResponse } from '../interfaces/market-data-response';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {

  private _sleepDuration: number = 1000;
  private _baseUrl: string = '';
  public rustItems: RustItemsResponse = <RustItemsResponse>{};

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {

    // get my rust inventory
    http.get<RustItemsResponse>(baseUrl + 'Steam/GetInventory').subscribe(async result => {

      // capture results
      this.rustItems = result;

      // null check
      if (this.rustItems.inventoryItems) {

        
        // get only tradable items
        let filteredArray = this.rustItems.inventoryItems.filter(function (val, i, a) {
          return val.tradable === 1;
        });

        // set initial lowest price
        this.rustItems.inventoryItems.map((obj) => {
          obj.lowestPrice = '--';
          return obj;
        })

        // log market call
        console.log('staring market call for ' + (filteredArray.length) + ' items.');

        // loop inventory
        for (var i = 0; i < filteredArray.length; i++) {

          // get the market hash name
          let mhn = filteredArray[i].marketHashName ?? '';

          // create url base and URI encode the market hash name
          let url = baseUrl + 'Steam/GetMarketData/' + encodeURI(mhn) + '/' + filteredArray[i].assetid + '/1';

          // get the market data
          http.get<MarketDataResponse>(url).subscribe(result => {

            // declare market data from the results
            let marketData = result;
            // log market details
            console.log('found market details for ' + mhn + ' lowest price = ' + marketData.lowest_Price);

            // get from original arry
            if (this.rustItems.inventoryItems) {

              // find index from main list
              var foundIndex = this.rustItems.inventoryItems.findIndex(x => x.assetid == filteredArray[i].assetid);

              // update the orig list with the new price value
              this.rustItems.inventoryItems[foundIndex].lowestPrice = marketData.lowest_Price;

              // log price to console
              console.log('set price data to  ' + marketData.lowest_Price);
            }
          }, error => console.error(error));

          // log current process number
          console.log('processed ' + i + ' of ' + (filteredArray.length - 1));

          // log sleep
          console.log('sleeping for  ' + this._sleepDuration + ' ms');

          //! sleep to avoid steam rate limiter
          await new Promise(f => setTimeout(f, this._sleepDuration))
        }
      }
    }, error => console.error(error));

    console.log('finished fetching market data.');
    //todo  Sum the total of lowest price property
    //if (this.rustItems.inventoryItems)
    //  console.log(this.rustItems.inventoryItems.reduce((n, { lowestPrice }) => n + lowestPrice ?? 0, 0));
  }
}
