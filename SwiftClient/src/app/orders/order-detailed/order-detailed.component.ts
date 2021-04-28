import { Component, OnInit } from '@angular/core';

import { AccountService } from 'src/app/account/account.service';
import { ActivatedRoute } from '@angular/router';
import { BreadcrumbService } from 'xng-breadcrumb';
import { IOrder } from 'src/app/shared/models/order';
import { OrdersService } from '../orders.service';

@Component({
  selector: 'app-order-detailed',
  templateUrl: './order-detailed.component.html',
  styleUrls: ['./order-detailed.component.scss']
})
export class OrderDetailedComponent implements OnInit {
  order: IOrder;
  id: any;
  isAdmin: boolean
  constructor(private route: ActivatedRoute,
    private breadcrumbService: BreadcrumbService,
    private ordersService: OrdersService,
    private accountService: AccountService) {
    this.breadcrumbService.set('@OrderDetailed', '');
  }

  ngOnInit() {
    this.id = +this.route.snapshot.paramMap.get('id');
    this.accountService.isAdmin$.subscribe(data => {
      this.isAdmin = data;
    })
    if (this.isAdmin) {
     this.getDetailsPerId()
    }
    else {
     this.getDetailsPerUserAndId();
    }

  }
  getDetailsPerUserAndId() {
    this.ordersService.getOrderPerUser(this.id).subscribe((order: IOrder) => {
      this.order = order;
      this.breadcrumbService.set('@OrderDetailed', `Order# ${order.id} - ${order.status}`);
    }, error => {
      console.log(error);
    });
  }
  getDetailsPerId() {
    this.ordersService.getOrderDetailed(this.id).subscribe((order: IOrder) => {
      this.order = order;
      this.breadcrumbService.set('@OrderDetailed', `Order# ${order.id} - ${order.status}`);
    }, error => {
      console.log(error);
    });
  }
}
