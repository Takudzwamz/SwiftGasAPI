import { Component, Input, OnInit } from '@angular/core';

import { AccountService } from 'src/app/account/account.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-order-totals',
  templateUrl: './order-totals.component.html',
  styleUrls: ['./order-totals.component.scss']
})
export class OrderTotalsComponent implements OnInit {
  @Input() shippingPrice: number;
  @Input() subtotal: number;
  @Input() total: number;
  @Input()address:any;
  isAdmin$: Observable<boolean>;

  constructor(private accountService: AccountService) { }

  ngOnInit() {
    this.isAdmin$ = this.accountService.isAdmin$;
  }

}
