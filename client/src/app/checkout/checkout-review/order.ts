import { IAddress } from "src/app/shared/models/address";

export interface order {
  basketId?: string;
  deliveryMethodId?: number,
  shipToAddress?: IAddress
  //{
  //   firstName: string;
  //   lastName: string;
  //   street: string;
  //   city: string;
  //   state: string;
  //   zipcode: string;
  // }

}
