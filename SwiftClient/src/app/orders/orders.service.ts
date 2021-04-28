import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrdersService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getOrdersForUser() {
    return this.http.get(this.baseUrl + 'orders');
  }

  getOrders() {
    return this.http.get(this.baseUrl + 'orders/GetOrders');
  }

  getOrderPerUser(id: number) {
    return this.http.get(this.baseUrl + 'orders/' + id);
  }
  getOrderDetailed(id: number) {
     return this.http.get(this.baseUrl + 'Orders/GetOrderById/' + id);
  }

}
