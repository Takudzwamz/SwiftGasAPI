import { Component, OnInit, ViewChild } from '@angular/core';

import { AccountService } from '../account/account.service';
import { IOrder } from '../shared/models/order';
import { OrdersService } from './orders.service';


@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnInit {
  orders: IOrder[];
  isAdmin: boolean;
  // buyerEmail: any;


  constructor(
    private ordersService: OrdersService,
    private accountService: AccountService,
  ) {}

  ngOnInit() {
    /* this.ordersService.getOrders().subscribe((response) => {
      (orders: IOrder[]) => {
        this.orders = orders;
      }
    }); */
    this.accountService.isAdmin$.subscribe((data) => {
      this.isAdmin = data;
    });
    if (this.isAdmin) {
      this.getOrders();
    } else {
      this.getOrdersPerUser();
    }
  }


 /*   Search() {
    if (this.buyerEmail === ' ') {
      this.getOrders();
    } else {
      this.orders = this.orders.filter(res => {
        return res.buyerEmail.toLocaleLowerCase().match(this.buyerEmail.toLocaleLowerCase());
      });
    }
  }*/


  getOrdersPerUser() {
    
    this.ordersService.getOrdersForUser().subscribe(
      (orders: IOrder[]) => {
        this.orders = orders;
      },
      (error) => {
        console.log(error);
      }
    );
  }
  getOrders() {
    
    this.ordersService.getOrders().subscribe(
      (orders: IOrder[]) => {
        this.orders = orders;
      },
      (error) => {
        console.log(error);
      }
    );
  }
}
