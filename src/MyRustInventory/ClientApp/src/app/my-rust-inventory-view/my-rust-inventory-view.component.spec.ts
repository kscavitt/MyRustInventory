import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyRustInventoryViewComponent } from './my-rust-inventory-view.component';

describe('MyRustInventoryViewComponent', () => {
  let component: MyRustInventoryViewComponent;
  let fixture: ComponentFixture<MyRustInventoryViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MyRustInventoryViewComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyRustInventoryViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
