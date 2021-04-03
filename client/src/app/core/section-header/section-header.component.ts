import { Component, OnInit } from "@angular/core";
import { BreadcrumbService } from "xng-breadcrumb";
import { Observable } from "rxjs";
import { IBasket } from "src/app/shared/models/basket";
import { BasketService } from "src/app/basket/basket.service";
import { IUser } from "src/app/shared/models/user";
import { AccountService } from "src/app/account/account.service";

@Component({
  selector: "app-section-header",
  templateUrl: "./section-header.component.html",
  styleUrls: ["./section-header.component.scss"],
})
export class SectionHeaderComponent implements OnInit {
  breadcrumb$: Observable<any[]>;
  basket$: Observable<IBasket>;
  currentUser$: Observable<IUser>;
  isAdmin$: Observable<boolean>;

  constructor(
    private bcService: BreadcrumbService,
    private basketService: BasketService,
    private accountService: AccountService
  ) {}

  ngOnInit() {
    this.breadcrumb$ = this.bcService.breadcrumbs$;
    this.basket$ = this.basketService.basket$;
    this.currentUser$ = this.accountService.currentUser$;
    this.isAdmin$ = this.accountService.isAdmin$;
  }
}
