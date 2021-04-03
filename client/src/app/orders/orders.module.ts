import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdersComponent } from './orders.component';
import { OrderDetailedComponent } from './order-detailed/order-detailed.component';
import { OrdersRoutingModule } from './orders-routing.module';
import { SharedModule } from '../shared/shared.module';
import { MaterialModule } from '../material/material.module';

// import { Ng2OrderModule} from 'ng2-order-pipe';
// import { Ng2SearchPipeModule} from 'ng2-search-filter';
// import { NgxPaginationModule} from 'ngx-pagination';


@NgModule({
  declarations: [OrdersComponent, OrderDetailedComponent],
  imports: [
    CommonModule,
    OrdersRoutingModule,
    SharedModule,
    MaterialModule,
   // Ng2OrderModule,
    // Ng2SearchPipeModule,
   // NgxPaginationModule,
  ],
})
export class OrdersModule {}
