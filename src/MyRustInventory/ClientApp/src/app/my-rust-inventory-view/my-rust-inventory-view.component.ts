import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { SteamService } from '../services/steam.service';
import { Observable } from 'rxjs';
import { RustItemDto } from '../interfaces/rust-item-dto';
import { LoggerService } from '../services/logger.service';

@Component({
  selector: 'app-my-rust-inventory-view',
  templateUrl: './my-rust-inventory-view.component.html',
  styleUrls: ['./my-rust-inventory-view.component.css']
})
export class MyRustInventoryViewComponent implements OnInit, OnDestroy {

  public totalTradableItems: Observable<number> = new Observable<number>();
  public currentPercentage: number = 0;
  public rustItems: RustItemDto[] = [];
  public value: number = 0;
  constructor(readonly steamService: SteamService) { }

  ngOnInit(): void {
    this.currentPercentage = 0;
    this.steamService.getInventory();

    this.steamService.currentPercentage$.subscribe(percentage => {
      this.currentPercentage = Math.round(percentage);
    });

    this.steamService.rustInventoryItems.subscribe(items => {
      this.rustItems = items
    });
    this.totalTradableItems = this.steamService.totalInventoryItems;

  }

  ngOnDestroy() {
    this.steamService.currentPercentage$.unsubscribe();
}

  get total() {
    return this.rustItems.map(p => (p.lowestPrice * p.quantity)).reduce((a, b) => a + b, 0);
  }

}
