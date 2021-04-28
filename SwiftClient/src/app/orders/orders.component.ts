import { Component, NgModule, OnInit, ViewChild } from '@angular/core';
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
  /*  this.orders = this.orders.sort(
     (a: any, b: any) =>
       new Date(a.orderDate).getTime() - new Date(b.orderDate).getTime()
   ); */
    this.accountService.isAdmin$.subscribe((data) => {
      this.isAdmin = data;
    });
    if (this.isAdmin) {
      this.getOrders();
    } else {
      this.getOrdersPerUser();
    }
  }




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




