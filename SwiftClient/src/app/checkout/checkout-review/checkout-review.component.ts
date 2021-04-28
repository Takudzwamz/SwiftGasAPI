import { Component, Input, OnInit } from '@angular/core';
import { IBasket, IBasketTotals } from 'src/app/shared/models/basket';
import { Router, NavigationExtras } from '@angular/router';
import { AccountService } from 'src/app/account/account.service';
import { BasketService } from 'src/app/basket/basket.service';
import { CdkStepper } from '@angular/cdk/stepper';
import { CheckoutService } from '../checkout.service';
import { Observable } from 'rxjs';
import { PayFastAPI } from '@gettruck/payfast-js';
import { ToastrService } from 'ngx-toastr';
import { order } from './order';

@Component({
  selector: 'app-checkout-review',
  templateUrl: './checkout-review.component.html',
  styleUrls: ['./checkout-review.component.scss'],
})
export class CheckoutReviewComponent implements OnInit {
  @Input() appStepper: CdkStepper;
  basket$: Observable<IBasket>;
  basketTotals$: Observable<IBasketTotals>;
  total: number;
  payfast: any;
  order: order = {};
  Orders: order[] = [];
  constructor(
    private basketService: BasketService,
    private toastr: ToastrService,
    private accountService: AccountService,
    private orderService: CheckoutService
  ) { }

  ngOnInit() {
    this.basket$ = this.basketService.basket$;
    // tslint:disable-next-line: deprecation
    this.basketService.basketTotal$.subscribe((t) => {
      if (t) {
        this.total = t.total;
      } else {
        this.total = 0;
      }
    });
    // tslint:disable-next-line: deprecation
    this.accountService.address$.subscribe(data => {
      if (data) {
        this.order.shipToAddress = data ? data : null;
      }

    });
    // tslint:disable-next-line: deprecation
    this.basket$.subscribe(basket => {
      if (basket) {
        this.order.basketId = basket.id;
        this.order.deliveryMethodId = basket.deliveryMethodId ? basket.deliveryMethodId : 0;

      }
      // basket.items.forEach(item => {
      //   this.Orders.push(this.order);

      // })
    });

    this.getOrders();

    this.payfast = new PayFastAPI({
      merchant_id: '17309722',
      merchant_key: 'sd2wh83wpfttm',
      production: true,
    });
  }

  createPaymentIntent() {
    let check = false;
    if (this.Orders.length) {
      check = this.Orders.some((item) => item.basketId === this.order.basketId);
    }
    if (this.order && !check) {
      // tslint:disable-next-line: deprecation
      this.orderService.creatOrder(this.order).subscribe((data) => {
        console.log(data);
        this.basketService.deleteLocalBasket(this.order.basketId);
      });
    }

    // return this.basketService.createPaymentIntent().subscribe((response: any) => {
    //   this.appStepper.next();
    // }, error => {
    //   console.log(error);
    // });

    // you can set url if you a have page
     // this.payfast.cancelURL("http://178.128.146.75/basket");
    // this.payfast notifyURL(url: string): void;
    // this.router.navigate(["checkout/success"], navigationExtras);
    // const navigationExtras: NavigationExtras = { state: createdOrder };
    // this.payfast.returnURL( 'https://www.tashebb.co.za/checkout/success');
    // this.payfast.returnURL("http://178.128.146.75/checkout/success");
    this.payfast.addPaymentDetails({
      amount: this.total,
      item_name: 'Orders',
      // currency:
    });
    // tslint:disable-next-line: prefer-const
    let url = this.payfast.generateURL();
    window.open(url, '_blank');
  }
  getOrders() {
    // tslint:disable-next-line: deprecation
    this.orderService.Orders().subscribe((data: any) => {
      this.Orders = data;
    });

  }
}
