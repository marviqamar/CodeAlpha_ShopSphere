import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminCouponList } from './admin-coupon-list';

describe('AdminCouponList', () => {
  let component: AdminCouponList;
  let fixture: ComponentFixture<AdminCouponList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminCouponList],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminCouponList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
