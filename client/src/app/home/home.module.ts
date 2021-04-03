import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeComponent } from './home.component';
import { SharedModule } from '../shared/shared.module';
import { MaterialModule } from '../material/material.module';
import { CarouselModule } from "ngx-owl-carousel-o";

@NgModule({
  declarations: [HomeComponent],
  imports: [
    CommonModule,
    SharedModule,
    MaterialModule,
    CarouselModule
  ],
  exports: [HomeComponent]
})
export class HomeModule { }
