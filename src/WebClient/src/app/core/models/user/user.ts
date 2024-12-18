import { PhotoResponse } from "./photoResponse";

export interface User {
  userId: string;
  email: string;
  displayName: string;
  createdAt: string;
  roles: string[];
  photo?: PhotoResponse;
}