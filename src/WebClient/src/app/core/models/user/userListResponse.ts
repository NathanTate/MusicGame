import { BasePagedListResponse } from "../BasePagedListResponse";
import { User } from "./user";

export interface UserListResponse extends BasePagedListResponse {
  items: User[];
}